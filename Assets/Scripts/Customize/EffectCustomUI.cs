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
    public Dictionary<string, Color> ColorList;  // 色のリスト

    public Dictionary<Gesture, Dictionary<Effect, EffectOption>> EffectsCustomize;

    public Dictionary<string, string> partJpName = new Dictionary<string, string>()
    {
        { "HandTipLeft", "左手" }, { "HandTipRight", "右手" },
        { "FootLeft", "左足" }, { "FootRight", "右足" },
        { "Body", "胴体" }, { "Head", "頭"}
    };

    private int TransitionState = 0; // CustomDialogの遷移モード
    private float dialogAlpha = 0; // CustomDialogの現在の透明度

    private Gesture? selectedGesture = null;
    private List<string> selectedParts;
    private Effect selectedEffect;

    private void Start()
    {
        EffectsCustomize = new Dictionary<Gesture, Dictionary<Effect, EffectOption>>();
        foreach (var g in Enum.GetValues(typeof(Gesture)))
            EffectsCustomize.Add((Gesture)g, new Dictionary<Effect, EffectOption>());
        selectedParts = new List<string>();
        SetDefault();

        GestureList = new Dictionary<string, Gesture>()
        {
            { "常時", Gesture.Always},
            { "ジャンプ", Gesture.Jump },
            { "拍手", Gesture.ChimpanzeeClap },
            { "パンチ", Gesture.Punch },
            { "かめはめ波", Gesture.Kamehameha },
            { "スペシウム光線", Gesture.Specium },
            { "DAISUKE", Gesture.Daisuke }
        };

        ColorList = new Dictionary<string, Color>()
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
            { "拍手", Effect.Clap },
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

    private void SetDefault()
    {
        foreach (var item in EffectsCustomize)
        {
            item.Value.Clear();
        }

        // ラインエフェクト
        {
            var eOption = new EffectOption();
            eOption.AttachedParts = new List<string>() { "HandTipLeft", "HandTipRight", "FootLeft", "FootRight" };
            eOption.IsRainbow = true;
            eOption.Scale = 1.0f;
            EffectsCustomize[Gesture.Always].Add(Effect.Line, eOption);
        }

        // パンチ
        {
            var eOption = new EffectOption();
            eOption.AttachedParts = new List<string>() { "HandTipLeft", "HandTipRight" };
            eOption.Color = ColorToFloatList(Color.blue);
            eOption.IsRainbow = false;
            eOption.Scale = 1.0f;
            EffectsCustomize[Gesture.Punch].Add(Effect.Ripple, eOption);
        }

        // ジャンプ
        {
            var eOption = new EffectOption();
            eOption.AttachedParts = new List<string>() { "Body" };
            eOption.Color = ColorToFloatList(Color.yellow);
            eOption.IsRainbow = false;
            eOption.Scale = 1.0f;
            EffectsCustomize[Gesture.Jump].Add(Effect.Impact, eOption);
        }

        // 拍手
        {
            var eOption = new EffectOption();
            eOption.AttachedParts = new List<string>() { "HandTipLeft", "HandTipRight" };
            eOption.Color = ColorToFloatList(Color.yellow);
            eOption.IsRainbow = false;
            eOption.Scale = 1.0f;
            EffectsCustomize[Gesture.ChimpanzeeClap].Add(Effect.Clap, eOption);
        }

        // Daisuke
        {
            var eOption = new EffectOption();
            eOption.AttachedParts = new List<string>() { "Head" };
            eOption.Color = ColorToFloatList(Color.yellow);
            eOption.IsRainbow = false;
            eOption.Scale = 1.0f;
            EffectsCustomize[Gesture.Daisuke].Add(Effect.Impact, eOption);
        }

        // かめはめ波
        {
            var eOption = new EffectOption();
            eOption.AttachedParts = new List<string>() { "HandLeft" };
            eOption.Color = ColorToFloatList(Color.yellow);
            eOption.IsRainbow = false;
            eOption.Scale = 1.0f;
            EffectsCustomize[Gesture.Kamehameha].Add(Effect.Beam, eOption);
        }
    }

    public void ReloadUI()
    {
        ReloadMotionSelTab();
        ReloadEffSelTab();

        // DDColor
        DDColor.ClearOptions();
        DDColor.AddOptions(ColorList.Keys.ToList());
        DDColor.RefreshShownValue();
    }

    private void ReloadMotionSelTab()
    {
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
                selectedGesture = item.Value;
                Title.text = "部位を選択";

                // トグルを全て無効化
                foreach (var t in TogglesPart)
                {
                    t.isOn = false;
                }

                PartSelTab.SetActive(true);
            });

            var trigger = _list.AddComponent<EventTrigger>();
            trigger.triggers.Add(entry);

        }
    }

    private void ReloadEffSelTab()
    {
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
            toggle.isOn = selectedGesture.HasValue && EffectsCustomize[selectedGesture.Value].ContainsKey(item.Value);

            toggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    CustomDialog.SetActive(true);

                    // カスタマイズデータに追加
                    var option = new EffectOption();
                    option.AttachedParts = selectedParts;
                    option.Color = ColorToFloatList(ColorList[DDColor.options[DDColor.value].text]);
                    option.Scale = SliderScale.value;
                    option.IsRainbow = false;
                    EffectsCustomize[selectedGesture.Value].Add(item.Value, option);

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
                    EffectsCustomize[selectedGesture.Value].Remove(item.Value);
                }
            });
        }
    }

    public void BtnDefault_OnClicked()
    {
        DialogManager.Instance.SetLabel("はい", "いいえ", "閉じる");
        DialogManager.Instance.ShowSelectDialog("最初の設定に戻しますか？", (ret) =>
        {
            SetDefault();
        });
    }

    public void BtnClose_OnClicked()
    {
        EffectsCustomize[selectedGesture.Value][selectedEffect].Color = ColorToFloatList(ColorList[DDColor.options[DDColor.value].text]);
        EffectsCustomize[selectedGesture.Value][selectedEffect].IsRainbow = DDColor.options[DDColor.value].text == "虹色";
        EffectsCustomize[selectedGesture.Value][selectedEffect].Scale = SliderScale.value;

        TransitionState = -1;
    }

    public void BtnToMotion_OnClicked()
    {
        PartSelTab.SetActive(false);
        EffSelTab.SetActive(false);
        MotionSelTab.SetActive(true);
        Title.text = "動きを選択";

        selectedGesture = null;
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

        if (selectedParts.Count == 0)
        {
            PartSelTab.SetActive(true);

            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSubmitDialog("エラー", "部位を1つ以上選択してください。", (ret) => { });

            return;
        }

        ReloadEffSelTab();
        EffSelTab.SetActive(true);
        Title.text = "エフェクトを選択";
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
