using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomManager : MonoBehaviour
{
    public Text Title;
    public CustomData customData;

    private WSClient ws;

    private Dictionary<string, GameObject> UIParts = new Dictionary<string, GameObject>();
    private string[] ui_parts_name = { "ShareJoin", "Effect" };
    private GameObject oldPanel = null;
    private Image oldItem = null;

    private bool isFirstSend = true;

    public void ItemClicked(string name)
    {
        ChangePanel(name);
    }

    public void BtnStart_OnClicked()
    {
        if (isFirstSend)
        {
            UIChange_OnPerformStart();
        }

        // EmServerにカスタマイズ内容を送信
        if (GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected)
        {
            var shareJoin = UIParts["ShareJoin"].transform;
            var effect = UIParts["Effect"].transform;
            customData.DoShare = shareJoin.Find("ToggleDoShare").GetComponent<Toggle>().isOn;

            var toggles_jointype = shareJoin.Find("Toggles_JoinType");
            int joinType = 0;
            if (toggles_jointype.Find("ToggleLike").GetComponent<Toggle>().isOn) joinType += 1;
            if (toggles_jointype.Find("ToggleCrap").GetComponent<Toggle>().isOn) joinType += 10;
            if (toggles_jointype.Find("ToggleKinect").GetComponent<Toggle>().isOn) joinType += 100;
            customData.JoinType = joinType;

            customData.EnabledLikes = shareJoin.Find("ScrollView_EnabledEffects").GetComponent<ChooseEffectsBehaviour>().GetEnableEffects();
            customData.EffectsCustomize = effect.GetComponent<EffectUIManager>().GetEffectsCustomize();

            ws.Send("CUSTOMIZE", customData);
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
        } else
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

	private void Start ()
    {
        InitUIParts();
        ChangePanel("ShareJoin");

        if (GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected)
        {
            ws = GameObject.Find("WSClient").GetComponent<WSClient>();
            customData = ws.CustomDefault;
            UIParts["ShareJoin"].transform.Find("ToggleDoShare").GetComponent<Toggle>().isOn = customData.DoShare;

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
        var shareJoin = this.gameObject.transform.Find("Panel_ShareJoin").transform;
        shareJoin.Find("ToggleDoShare").GetComponent<Toggle>().interactable = false;
        var menu = this.gameObject.transform.Find("Menu").transform;
        menu.Find("BtnStart").GetComponent<Image>().color = new Color32(236, 255, 206, 255);
        menu.Find("BtnStart").Find("Text").GetComponent<Text>().text = "適用";
        menu.Find("BtnEnd").GetComponent<Button>().interactable = true;

        isFirstSend = false;
    }

    private void InitUIParts()
    {
        foreach(var name in ui_parts_name)
        {
            var obj = this.gameObject.transform.Find("Panel_" + name);
            if (obj != null)
            {
                UIParts.Add(name, obj.gameObject);
                obj.gameObject.SetActive(false);
            }
        }
    }

    private void ChangePanel(string _name)
    {
        if (!UIParts.ContainsKey(_name)) return;
        if (oldPanel != null) oldPanel.SetActive(false);
        if (oldItem != null) oldItem.color = Color.white;

        var obj = this.gameObject.transform.Find("Panel_" + _name).gameObject;
        obj.SetActive(true);
        var menu = this.gameObject.transform.Find("Menu");
        var btn = menu.Find("Item_" + _name).Find("Background").GetComponent<Image>();
        btn.color = new Color32(213, 234, 251, 255);
        var label = menu.Find("Item_" + _name).Find("Text").GetComponent<Text>();
        Title.GetComponent<Text>().text = label.text;

        oldPanel = obj;
        oldItem = btn;
        
    }

}
