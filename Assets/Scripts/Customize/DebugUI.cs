using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public GameObject Panel_Debug;
    public CustomManager customMgr;
    public Text DebugInfo;
    public Text DebugState;
    public Dropdown DDChargedSE;
    public Dropdown DDFillingSE;

    private bool prevState = false;

    public void OnTabChanged()
    {
        customMgr.MakeCustomData();
        DebugInfo.text = JsonConvert.SerializeObject(customMgr.customData, Formatting.Indented);
        var network = GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected;
        DebugState.text = "Network : " + (network ? "OK" : "NG");
    }

    public void BtnSave_OnClicked()
    {
        var _lsmgr = GameObject.Find("LocalSettings");
        if (_lsmgr == null)
            return;

        var lsmgr = _lsmgr.GetComponent<LocalSettingsManager>();
        lsmgr.localSettings.ChargedSE = DDChargedSE.options[DDChargedSE.value].text;
        lsmgr.localSettings.FillingSE = DDFillingSE.options[DDFillingSE.value].text;
        lsmgr.Write();
    }

    public void BtnRestart_OnClicked()
    {
        if (GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected)
        {
            GameObject.Find("WSClient").GetComponent<WSClient>().Send("RESTART", "");
            customMgr.Reset_UIChange();

        }
    }

}
