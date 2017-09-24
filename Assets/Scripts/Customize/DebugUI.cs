using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public GameObject Panel_Debug;
    public EffectCustomUI effectCustomUI;
    public Text DebugInfo;
    public Text DebugState;

    private bool prevState = false;

    private void Update()
    {
        if (!prevState && this.gameObject.activeInHierarchy)
        {
            DebugInfo.text = JsonConvert.SerializeObject(effectCustomUI.EffectsCustomize, Formatting.Indented);
            var network = GameObject.Find("WSClient") != null && GameObject.Find("WSClient").GetComponent<WSClient>().isConnected;
            DebugState.text = "Network : " + (network ? "OK" : "NG");
        }

        prevState = this.gameObject.activeInHierarchy;

    }

}
