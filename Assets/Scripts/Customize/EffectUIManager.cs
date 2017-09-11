using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectUIManager : MonoBehaviour
{
    #region UnityObjects
    public RectTransform Layout;
    public List<GameObject> Tabs;
    public GameObject Panel;

    public GameObject PartSelector;
    public List<Toggle> TogglesPart;

    public GameObject PoseSelector;
    public Dropdown DDPose;

    public GameObject EffectSelector;
    public Dropdown DDEffect;

    public GameObject CheckEnabled;
    public Toggle ToggleEnabled;

    public Text DescDDColor;
    public Dropdown DDColor;

    public Text DescSliderScale;
    public Slider SliderScale;

    public GameObject BtnDebug;
    #endregion

    private Dictionary<string, EffectOption> _EffectsCustomize;

    private Dictionary<string, string> GestureList;
    private Dictionary<string, string> EffectList;
    private Dictionary<string, Color> ColorList;

    private bool doNotProcessEvent = false;
    private float LogoTapStart;

    private void Start()
    {
        if (GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected)
        {
            var ws = GameObject.Find("WSClient").GetComponent<WSClient>();
            var raw = ws.RequestHTTP(Method.GET, "options.json");
            var conf = JsonUtility.FromJson<CustomOptions>(raw);

            GestureList = conf.Gesture.ToDictionary();
            EffectList = conf.Effect.ToDictionary();
            ColorList = conf.Color.ToDictionary();
        }

        if (GameObject.Find("DEMO") != null || GestureList == null) GestureList = new Dictionary<string, string> { { "スペシウム光線", "Spacium" }, { "左パンチ", "PanchLeft" }, { "右パンチ", "PanchRight" }, { "ジャンプ", "Jump" } };
        if (GameObject.Find("DEMO") != null || EffectList == null) EffectList = new Dictionary<string, string> { { "レーザー", "Laser01" } };
        if (GameObject.Find("DEMO") != null || ColorList == null) ColorList = new Dictionary<string, Color> { { "赤", Color.red }, { "青", Color.blue }, { "黄", Color.yellow }, { "緑", Color.green }, { "黒", Color.black }, { "白", Color.white }, { "虹色", Color.black } };

        DDPose.AddOptions(GestureList.Keys.ToList());
        DDEffect.AddOptions(EffectList.Keys.ToList());
        DDColor.AddOptions(ColorList.Keys.ToList());

        _EffectsCustomize = new Dictionary<string, EffectOption>();
    }

    public string GetEffectsCustomize()
    {
        return JsonUtility.ToJson(new Serialization<string, EffectOption>(_EffectsCustomize));
    }

    public void DDEffect_OnValueChanged()
    {
        // 有効無効を切り替え
        var gestureName = GestureList[DDPose.options[DDPose.value].text];
        var effName = EffectList[DDEffect.options[DDEffect.value].text];
        var enabled = _EffectsCustomize.ContainsKey(gestureName) && _EffectsCustomize[gestureName].Name == effName;
        ToggleEnabled.isOn = DDColor.interactable = SliderScale.interactable = enabled;
        if (enabled)
        {
            foreach(var col in ColorList)
            {
                if (col.Value == _EffectsCustomize[gestureName].Color)
                {
                    DDColor.value = ColorList.Keys.ToList().IndexOf(col.Key);
                    break;
                }
            }
            SliderScale.value = _EffectsCustomize[gestureName].Scale.x;
        }

    }

    public void TogglePart_OnValueChanged()
    {
        if (doNotProcessEvent) return;

        // カスタマイズデータに追加 / 削除
        foreach (var toggle in TogglesPart)
        {
            var jt = toggle.GetComponent<ToggleAllocName>().JointName;
            if (toggle.isOn)
            {
                if (!_EffectsCustomize.ContainsKey("Joint_" + jt))
                    _EffectsCustomize.Add("Joint_" + jt, new EffectOption("LINE", Vector3.one, ColorList[DDColor.options[DDColor.value].text], DDColor.options[DDColor.value].text == "虹色"));
            }
            else
            {
                if (_EffectsCustomize.ContainsKey("Joint_" + jt))
                    _EffectsCustomize.Remove("Joint_" + jt);
            }
        }

        DDColor.interactable = SliderScale.interactable = TogglesPart.Where(t => t.isOn).Count() > 0;
    }

    public void ToggleEnabled_OnValueChanged()
    {
        // カスタマイズデータに追加 / 削除
        var gestureName = GestureList[DDPose.options[DDPose.value].text];
        if (ToggleEnabled.isOn)
        {
            var effName = EffectList[DDEffect.options[DDEffect.value].text];
            var col = ColorList[DDColor.options[DDColor.value].text];

            if (!_EffectsCustomize.ContainsKey(gestureName))
                _EffectsCustomize.Add(gestureName, new EffectOption(effName, col, DDColor.options[DDColor.value].text == "虹色"));
            DDColor.interactable = SliderScale.interactable = true;
        }
        else
        {
            if (_EffectsCustomize.ContainsKey(gestureName))
                _EffectsCustomize.Remove(gestureName);
            DDColor.interactable = SliderScale.interactable = false;
        }

    }

    public void DDColor_OnValueChanged()
    {
        // カスタマイズデータを変更
        var gestureName = GestureList[DDPose.options[DDPose.value].text];
        _EffectsCustomize[gestureName].Color = ColorList[DDColor.options[DDColor.value].text];
        _EffectsCustomize[gestureName].isRainbow = DDColor.options[DDColor.value].text == "虹色";
    }

    public void SliderScale_OnValueChanged()
    {
        // カスタマイズデータを変更
        var gestureName = GestureList[DDPose.options[DDPose.value].text];
        var scale = SliderScale.value;
        _EffectsCustomize[gestureName].Scale = new Vector3(scale, scale);
    }

    public void BtnAllDelete_OnClicked()
    {
        if (!Application.isEditor)
        {
            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSelectDialog("全てのエフェクトを削除しますか？\n(この操作は元に戻せません。)", (ret) =>
            {
                if (ret)
                {
                    _EffectsCustomize.Clear();
                    Tab_SetDisable(0);
                }

            });
        }
        else
        {
            _EffectsCustomize.Clear();
            Tab_SetDisable(0);
        }
    }

    public void Tab_SetEnable(int value)
    {
        var tabname = "Tab" + value.ToString("00");
        foreach (var tab in Tabs)
        {
            tab.SetActive(tab.name == tabname || (tab.name != ("_" + tabname) && tab.name.Contains("_")));
        }

        switch (value)
        {
            case 1:
                PoseSelector.SetActive(true);
                EffectSelector.SetActive(true);
                CheckEnabled.SetActive(true);
                PartSelector.SetActive(false);
                DescDDColor.text = "4. 色を選択";
                DescSliderScale.text = "5. 大きさ";

                var gestureName = GestureList[DDPose.options[DDPose.value].text];
                var effName = EffectList[DDEffect.options[DDEffect.value].text];
                var enabled = _EffectsCustomize.ContainsKey(gestureName) && _EffectsCustomize[gestureName].Name == effName;
                ToggleEnabled.isOn = DDColor.interactable = SliderScale.interactable = enabled;

                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
                break;

            case 2:
                PoseSelector.SetActive(false);
                EffectSelector.SetActive(false);
                CheckEnabled.SetActive(false);
                PartSelector.SetActive(true);
                DescDDColor.text = "2. 色を選択";
                DescSliderScale.text = "3. 大きさ";

                doNotProcessEvent = true;
                foreach (var toggle in TogglesPart)
                {
                    var jt = toggle.GetComponent<ToggleAllocName>().JointName;
                    toggle.isOn = _EffectsCustomize.ContainsKey("Joint_" + jt);
                }
                doNotProcessEvent = false;

                DDColor.interactable = SliderScale.interactable = TogglesPart.Where(t => t.isOn).Count() > 0;

                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 725);
                break;
        }

        Panel.SetActive(true);
        LayoutRebuilder.MarkLayoutForRebuild(Layout);
    }

    public void Tab_SetDisable(int value)
    {
        foreach (var tab in Tabs)
        {
            tab.SetActive(tab.name.Contains("_"));
        }
        Panel.SetActive(false);
        LayoutRebuilder.MarkLayoutForRebuild(Layout);
    }

    public void BtnDebug_OnClicked()
    {
        var network = GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected;
        var debug = string.Format("[{0}] ", DateTime.Now) + Environment.NewLine + "Network : " + (network ? "OK" : "NG") + Environment.NewLine + "EffectCustomize-Json : " + JsonUtility.ToJson(new Serialization<string, EffectOption>(_EffectsCustomize));

        if (Application.isEditor)
        {
            Debug.Log(debug);
        } else
        {
            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSubmitDialog("デバッグ情報", debug, (ret) => { });
        }
    }

    public void Logo_OnPointerDown()
    {
        LogoTapStart = Time.time;
    }

    public void Logo_OnPointerUp()
    {
        if (Time.time - LogoTapStart >= 3.0f)
        {
            BtnDebug.SetActive(true);
        }
    }

    [Serializable]
    public class CustomOptions
    {
        public Serialization<string, string> Gesture;
        public Serialization<string, string> Effect;
        public Serialization<string, Color> Color;
    }

}
