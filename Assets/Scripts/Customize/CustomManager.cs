using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomManager : MonoBehaviour
{
    public Text Title;
    public CustomData customData;
    public GameObject Item_Debug;

    private WSClient ws;

    private Dictionary<string, GameObject> UIParts = new Dictionary<string, GameObject>();
    private string[] ui_parts_name = { "General", "ShareJoin", "Effect", "Debug" };
    private Dictionary<string, string> titles = new Dictionary<string, string>()
    {
        { "General", "全般" },
        { "ShareJoin", "観客参加" },
        { "Effect", "動きを選択" },
        { "Debug", "*** デバッグ ***" }
    };

    private GameObject oldPanel = null;
    private Image oldItem = null;

    private bool isFirstSend = true;
    private float _LogoTapped;
    private int _LogoTapCount = 0;

    private bool interactable = true;

    public void ChangeMenuTabsInteractable(bool _interactable)
    {
        interactable = _interactable;
    }

    public void ItemClicked(string name)
    {
        if (interactable)
            ChangePanel(name);
    }

    public void BtnStart_OnClicked()
    {
        // バリデーション
        var _limit = UIParts["General"].transform.Find("InputLimit").GetComponent<InputField>().text;
        int limit;
        if (!int.TryParse(_limit, out limit) || limit < 0)
        {
            Debug.LogWarning("InputLimit is not valid.");

            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSubmitDialog("エラー", "時間制限には0以上の値を入力してください。", (ret) => {});

            return;
        }

        if (isFirstSend)
        {
            UIChange_OnPerformStart();
        }

        // EmServerにカスタマイズ内容を送信
        if (GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected)
        {
            MakeCustomData();
            ws.Send("CUSTOMIZE", customData, true);
        }

        Debug.Log("Sent CUSTOMIZE");
    }

    public void BtnEnd_OnClicked()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSelectDialog("確認", "演技を終わり、Emotionicを終了します。よろしいですか？", (bool result) =>
            {
                if (result)
                {
                    CloseEmotionic();
                }
            });
        }
        else
        {
            Debug.Log("Omit dialog. Closing Emotionic.");
            CloseEmotionic();
        }
    }

    private void CloseEmotionic()
    {
        if (ws != null)
        {
            // EmServerに送信
            ws.Send("ENDPERFORM", customData.DoShare ? "DOSHARE" : "");

            // WSClientの明示的な破棄
            var wsobj = GameObject.Find("WSClient");
            GameObject.Destroy(wsobj);
        }

        // 最初に戻る
        SceneManager.LoadScene("Select");

    }

    private void Start()
    {
        //横画面にする
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        customData = new CustomData();

        InitUIParts();
        ChangePanel("General");

        if (GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected)
        {
            ws = GameObject.Find("WSClient").GetComponent<WSClient>();

            if (!string.IsNullOrEmpty(ws.EnabledJoinTypes))
            {
                var toggles_jointype = UIParts["ShareJoin"].transform.Find("Toggles_JoinType");
                toggles_jointype.Find("ToggleLike").GetComponent<Toggle>().interactable = ws.EnabledJoinTypes[3] == '1';
                toggles_jointype.Find("ToggleCrap").GetComponent<Toggle>().interactable = ws.EnabledJoinTypes[2] == '1';
                toggles_jointype.Find("ToggleKinect").GetComponent<Toggle>().interactable = ws.EnabledJoinTypes[1] == '1';
                toggles_jointype.Find("ToggleAR").GetComponent<Toggle>().interactable = ws.EnabledJoinTypes[0] == '1';
            }

            if (ws.isAuthenticated)
            {
                UIChange_OnPerformStart();
            }
        }
        else
            Debug.LogWarning("WSClient is null.");

    }

    private void UIChange_OnPerformStart()
    {
        var general = UIParts["General"].transform;
        general.Find("ToggleDoShare").GetComponent<Toggle>().interactable = false;
        general.Find("InputLimit").GetComponent<InputField>().interactable = false;

        var menu = this.gameObject.transform.Find("Menu").transform;
        menu.Find("BtnStart").GetComponent<Image>().color = new Color32(236, 255, 206, 255);
        menu.Find("BtnStart").Find("Text").GetComponent<Text>().text = "適用";
        menu.Find("BtnEnd").GetComponent<Button>().interactable = true;

        isFirstSend = false;
    }

    public void Reset_UIChange()
    {
        var general = UIParts["General"].transform;
        general.Find("ToggleDoShare").GetComponent<Toggle>().interactable = true;
        general.Find("InputLimit").GetComponent<InputField>().interactable = true;

        var menu = this.gameObject.transform.Find("Menu").transform;
        menu.Find("BtnStart").GetComponent<Image>().color = new Color32(206, 231, 255, 255);
        menu.Find("BtnStart").Find("Text").GetComponent<Text>().text = "開始";
        menu.Find("BtnEnd").GetComponent<Button>().interactable = false;

        isFirstSend = true;
    }

    private void InitUIParts()
    {
        foreach (var _name in ui_parts_name)
        {
            var obj = this.gameObject.transform.Find("Panel_" + _name);
            if (obj != null)
            {
                UIParts.Add(_name, obj.gameObject);
                obj.gameObject.SetActive(false);
            }
        }
    }

    private void ChangePanel(string _name)
    {
        if (!UIParts.ContainsKey(_name))
            return;
        if (oldPanel != null)
            oldPanel.SetActive(false);
        if (oldItem != null && oldPanel.name != "Panel_Debug")
            oldItem.color = Color.white;

        var obj = UIParts[_name];
        obj.SetActive(true);
        var menu = this.gameObject.transform.Find("Menu");
        var btn = menu.Find("Item_" + _name).Find("Background").GetComponent<Image>();
        if (_name != "Debug")
            btn.color = new Color32(213, 234, 251, 255);
        Title.text = titles[_name];

        oldPanel = obj;
        oldItem = btn;

        if (_name == "Debug")
        {
            // 変更イベントの通知
            obj.GetComponent<DebugUI>().OnTabChanged();
        }

    }

    public void MakeCustomData()
    {
        var general = UIParts["General"].transform;
        var shareJoin = UIParts["ShareJoin"].transform;
        var effect = UIParts["Effect"].transform;

        customData.DoShare = general.Find("ToggleDoShare").GetComponent<Toggle>().isOn;
        customData.IsZNZOVisibled = general.Find("ToggleZNZO").GetComponent<Toggle>().isOn;
        customData.TimeLimit = int.Parse(general.Find("InputLimit").GetComponent<InputField>().text);
            
        var toggles_jointype = shareJoin.Find("Toggles_JoinType");
        int joinType = 0;
        if (toggles_jointype.Find("ToggleLike").GetComponent<Toggle>().isOn)
            joinType += 1;
        if (toggles_jointype.Find("ToggleCrap").GetComponent<Toggle>().isOn)
            joinType += 10;
        if (toggles_jointype.Find("ToggleKinect").GetComponent<Toggle>().isOn)
            joinType += 100;
        if (toggles_jointype.Find("ToggleAR").GetComponent<Toggle>().isOn)
            joinType += 1000;
        customData.JoinType = joinType;

        customData.EnabledLikes = shareJoin.Find("ScrollView_EnabledEffects").GetComponent<ChooseEffectsBehaviour>().GetEnableEffects();
        customData.EffectsCustomize = effect.GetComponent<EffectCustomUI>().EffectsCustomize;
        
    }

    public void Logo_OnPointerClicked()
    {
        if (Time.time - _LogoTapped >= 3.0f)
            _LogoTapCount = 0;
        else
        {
            _LogoTapCount++;
            if (_LogoTapCount >= 3)
            {
                Item_Debug.SetActive(true);
            }
        }

        _LogoTapped = Time.time;
    }

}
