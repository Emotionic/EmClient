using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EffectCustomUI : MonoBehaviour
{
    #region UnityObjects
    public Text Title;
    public CustomManager customManager;
    public GameObject PartSelTab;
    public GameObject EffSelTab;
    public GameObject MotionSelTab;
    public GameObject CustomDialog;

    public GameObject MotionSelTab_Content;
    public GameObject EffSelTab_Content;
    public GameObject ListTemplate;
    public GameObject ToggleTemplate;

    public Dropdown DDColor;
    public Slider SliderScale;

    public List<Toggle> TogglesPart;

    #endregion

    public Dictionary<string, Gesture> GestureList; // 動きのリスト
    public Dictionary<string, Effect> EffectList;  // エフェクトのリスト
    public Dictionary<string, Color32> ColorList;  // 色のリスト

    public Dictionary<Gesture, Dictionary<Effect, EffectOption>> EffectsCustomize;

    public Dictionary<string, string> partJpName = new Dictionary<string, string>()
    {
        { "HandTipLeft", "左手" }, { "HandTipRight", "右手" },
        { "FootLeft", "左足" }, { "FootRight", "右足" },
        { "Body", "胴体" }, { "Head", "頭"}
    };

    private int TransitionState = 0; // CustomDialogの遷移モード
    private float dialogAlpha = 0; // CustomDialogの現在の透明度

    private Gesture selectedGesture;
    private List<string> selectedParts;
    private Effect selectedEffect;

    private void Start()
    {
        EffectsCustomize = new Dictionary<Gesture, Dictionary<Effect, EffectOption>>();
        foreach (var g in Enum.GetValues(typeof(Gesture)))
            EffectsCustomize.Add((Gesture)g, new Dictionary<Effect, EffectOption>());
        selectedParts = new List<string>();

        GestureList = new Dictionary<string, Gesture>()
        {
            { "常時", Gesture.Always},
            { "ジャンプ", Gesture.Jump },
            { "スペシウム光線", Gesture.Specium },
            { "パンチ", Gesture.Punch },
            { "ラジオ体操", Gesture.Exercise }
        };

        ColorList = new Dictionary<string, Color32>()
        {
            { "赤", Color.red },
            { "青", Color.blue },
            { "緑", Color.green },
            { "黄", Color.yellow },
            { "白", Color.white },
            { "虹色", Color.black }
        };

        EffectList = new Dictionary<string, Effect>()
        {
            { "ラインエフェクト", Effect.Line },
            { "ビーム", Effect.Beam },
            { "パンチエフェクト", Effect.Punch },
            { "波紋", Effect.Ripple },
            { "爆発", Effect.Impact }
        };

        ReloadUI();
    }

    private void Update()
    {
        if (TransitionState != 0)
        {
            dialogAlpha += Time.deltaTime * TransitionState * 3;
            CustomDialog.GetComponent<CanvasGroup>().alpha = dialogAlpha;
            if (dialogAlpha >= 1.0f)
            {
                TransitionState = 0;
            }
            else if (dialogAlpha < 0)
            {
                CustomDialog.SetActive(false);
                dialogAlpha = 0;
                TransitionState = 0;

                // トグルの有効化
                foreach (Transform _t in EffSelTab_Content.transform)
                {
                    _t.GetComponent<Toggle>().interactable = true;
                }

                // OKボタンの有効化
                EffSelTab.transform.Find("BtnOK").GetComponent<Button>().interactable = true;

                // メニューボタンの有効化
                customManager.ChangeMenuTabsInteractable(true);
            }
        }
    }

    public void ReloadUI()
    {
        // Motion
        // 既存のListを削除
        foreach (Transform obj in MotionSelTab_Content.transform)
        {
            if (obj.name != "ListTemplate")
                Destroy(obj.gameObject);
        }

        // Listを生成
        foreach (var item in GestureList)
        {
            var _list = Instantiate(ListTemplate, MotionSelTab_Content.transform);
            _list.SetActive(true);

            _list.transform.Find("Label").GetComponent<Text>().text = item.Key;

            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((value) =>
            {
                MotionSelTab.SetActive(false);
                PartSelTab.SetActive(true);
                selectedGesture = item.Value;
                Title.text = "部位を選択";
            });

            var trigger = _list.AddComponent<EventTrigger>();
            trigger.triggers.Add(entry);

        }

        // Effect
        // 既存のToggleを削除
        foreach (Transform obj in EffSelTab_Content.transform)
        {
            if (obj.name != "ToggleTemplate")
                Destroy(obj.gameObject);
        }

        // Toggleを生成
        foreach (var item in EffectList)
        {
            var _toggle = Instantiate(ToggleTemplate, EffSelTab_Content.transform);
            _toggle.SetActive(true);
            _toggle.transform.Find("Label").GetComponent<Text>().text = item.Key;
            var toggle = _toggle.GetComponent<Toggle>();
            toggle.isOn = false;

            toggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    CustomDialog.SetActive(true);

                    // カスタマイズデータに追加
                    var option = new EffectOption();
                    option.AttachedParts = selectedParts;
                    option.Color = ColorToFloatList(ColorList[DDColor.options[0].text]);
                    option.IsRainbow = false;
                    EffectsCustomize[selectedGesture].Add(item.Value, option);

                    selectedEffect = item.Value;

                    // 動きを選択するトグルの無効化
                    foreach (Transform _t in EffSelTab_Content.transform)
                    {
                        _t.GetComponent<Toggle>().interactable = false;
                    }

                    // OKボタンの無効化
                    EffSelTab.transform.Find("BtnOK").GetComponent<Button>().interactable = false;

                    // メニューボタンの無効化
                    customManager.ChangeMenuTabsInteractable(false);

                    CustomDialog.transform.Find("Text").GetComponent<Text>().text = item.Key;

                    TransitionState = 1;
                }
                else
                {
                    // カスタマイズデータから削除
                    EffectsCustomize[selectedGesture].Remove(item.Value);
                }
            });
        }

        // DDColor
        DDColor.ClearOptions();
        DDColor.AddOptions(ColorList.Keys.ToList());
        DDColor.RefreshShownValue();
    }

    public void BtnDefault_OnClicked()
    {
        DialogManager.Instance.SetLabel("はい", "いいえ", "閉じる");
        DialogManager.Instance.ShowSelectDialog("最初の設定に戻しますか？", (ret) => { });
    }

    public void BtnClose_OnClicked()
    {
        TransitionState = -1;
    }

    public void BtnToMotion_OnClicked()
    {
        PartSelTab.SetActive(false);
        EffSelTab.SetActive(false);
        MotionSelTab.SetActive(true);
        Title.text = "動きを選択";
    }

    public void BtnToEffect_OnClicked()
    {
        PartSelTab.SetActive(false);

        selectedParts.Clear();
        foreach (var t in TogglesPart)
        {
            if (t.isOn)
            {
                selectedParts.Add(t.name);
            }
        }

        EffSelTab.SetActive(true);
        Title.text = "エフェクトを選択";
    }

    public void DDColor_OnValueChanged()
    {
        EffectsCustomize[selectedGesture][selectedEffect].Color = ColorToFloatList(ColorList[DDColor.options[DDColor.value].text]);
        EffectsCustomize[selectedGesture][selectedEffect].IsRainbow = DDColor.options[DDColor.value].text == "虹色";
    }

    public void SliderScale_OnValueChanged()
    {
        EffectsCustomize[selectedGesture][selectedEffect].Scale = SliderScale.value;
    }

    private List<float> ColorToFloatList(Color _col)
    {
        var _list = new List<float>();
        _list.Add(_col.r);
        _list.Add(_col.g);
        _list.Add(_col.b);
        _list.Add(_col.a);
        return _list;
    }

}
