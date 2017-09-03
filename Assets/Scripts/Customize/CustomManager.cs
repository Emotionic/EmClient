using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomManager : MonoBehaviour
{
    public Text Title;

    private WSClient ws;
    private CustomData customData;

    private Dictionary<string, GameObject> UIParts = new Dictionary<string, GameObject>();
    private string[] ui_parts_name = { "General", "Other" };
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
            var panel = this.gameObject.transform.Find("Panel_General").transform;
            panel.Find("ToggleDoShare").GetComponent<Toggle>().interactable = false;
            panel.Find("BtnStart").GetComponent<Image>().color = new Color32(236, 255, 206, 255);
            panel.Find("BtnStart").Find("Text").GetComponent<Text>().text = "適用";
            panel.Find("BtnEnd").GetComponent<Button>().interactable = true;

            isFirstSend = false;
        }

        // EmServerにカスタマイズ内容を送信
        if (ws != null)
        {
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
        ChangePanel("General");

        if (GameObject.Find("WSClient") != null)
        {
            ws = GameObject.Find("WSClient").GetComponent<WSClient>();
            customData = ws.CustomDefault;

            if (ws.isAuthenticated)
            {
                var panel = this.gameObject.transform.Find("Panel_General").transform;
                panel.Find("ToggleDoShare").GetComponent<Toggle>().interactable = false;
                panel.Find("BtnStart").GetComponent<Image>().color = new Color32(236, 255, 206, 255);
                panel.Find("BtnStart").Find("Text").GetComponent<Text>().text = "適用";
                panel.Find("BtnEnd").GetComponent<Button>().interactable = true;

                isFirstSend = false;
            }

        }
        else
            Debug.LogWarning("WSClient is null.");

    }

    private void Update ()
    {
		
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
