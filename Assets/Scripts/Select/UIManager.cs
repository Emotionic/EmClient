using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject ModeView;
    public GameObject IPView;
    public GameObject PinView;
    public GameObject CalibWaitView;

    public Text LabelBtn;
    public Text LabelIPError;
    public Text LabelPinError;
    public Text LabelQRError;
    public InputField InputIP;
    public InputField InputPin;
    public Toggle ToggleDoShare;

    public QRReader QR;

    private string Address;
    private string Pin;

    private RectTransform rectTransition;
    private string TransitionState = "None";
    private const float TRANCOUNT = 0.5f;
    private float t;

    private readonly Color PerformerBlack = new Color32(69, 69, 69, 255);
    private readonly Color EmotionicColor = new Color32(213, 234, 251, 255);

    private bool isCustomizeMode;
    private QRReader _QR;

    private void Start()
    {
        _QR = QR.GetComponent<QRReader>();
    }

    private void Update()
    {
        if (_QR.isDetecting && !string.IsNullOrEmpty(_QR.Result) && _QR.Result != "error")
        {
            var qrdata = JsonUtility.FromJson<QRData>(_QR.Result);
            ProcessQR(qrdata);
        }

        if (TransitionState == "None")
            return;

        var sign = TransitionState == "Forward" ? 1 : -1;
        var x = -(Math.PI / 2) + Math.PI * (t / TRANCOUNT);
        var y = sign * (float)Math.Sin(x) * 0.5f + 0.5f;

        rectTransition.anchorMin = new Vector2(y, 0); // 1 -> 0
        rectTransition.anchorMax = new Vector2(1 + y, 1); // 2 -> 1

        if (t == 0)
        {
            TransitionState = "None";
        }
        else
        {
            t -= Time.deltaTime;
            if (t < 0)
                t = 0;
        }

    }

    public void BtnMode_OnClick()
    {
        _QR.CamStop();
        TransitionView("Forward", ModeView);
    }

    public void BtnAR_OnClick()
    {
        isCustomizeMode = false;
        LabelBtn.GetComponent<Text>().text = "接続";
        IPView.transform.Find("Image").GetComponent<Image>().color = EmotionicColor;
        LabelIPError.GetComponent<Text>().text = "";

        TransitionView("Forward", IPView);

    }

    public void BtnCustom_OnClick()
    {
        isCustomizeMode = true;
        LabelBtn.GetComponent<Text>().text = "次へ";
        IPView.transform.Find("Image").GetComponent<Image>().color = PerformerBlack;
        LabelIPError.GetComponent<Text>().text = "";

        TransitionView("Forward", IPView);

    }

    public void BtnBack_OnClick()
    {
        TransitionView("Back", IPView);

    }

    public void BtnIP_OnClick()
    {
        Address = InputIP.GetComponent<InputField>().text;
        if (Address == "")
            Address = "localhost";
        LabelPinError.GetComponent<Text>().text = "接続中です...";
        Canvas.ForceUpdateCanvases();

        var res = CallServer("check");
        if (res == null)
        {
            // 接続不可
            LabelIPError.GetComponent<Text>().text = "サーバに接続することができませんでした" + Environment.NewLine + "IPアドレスをご確認ください";
            return;

        }
        else if (res != "authenticated" && isCustomizeMode)
        {
            // パフォーマーモード
            // Pin認証
            TransitionView("Forward", PinView);

        }

        // キャリブレーション待機画面へ
        TransitionView("Forward", CalibWaitView);
        GameObject.Find("WSClient").GetComponent<WSClient>().Connect(Address, isCustomizeMode);

    }

    public void BtnPin_OnClick()
    {
        // Pin入力完了
        LabelPinError.GetComponent<Text>().text = "確認中です...";
        Canvas.ForceUpdateCanvases();
        Pin = InputPin.GetComponent<InputField>().text;

        var res = CallServer("pin");
        if (res == null)
        {
            // 接続エラー
            LabelPinError.GetComponent<Text>().text = "接続時にエラーが発生しました";
            return;
        }
        else if (res != "ok")
        {
            // 不正なPIN
            LabelPinError.GetComponent<Text>().text = "PINが正しくありません";
            return;
        }

        TransitionView("Forward", CalibWaitView);
        GameObject.Find("WSClient").GetComponent<WSClient>().Connect(Address, true);

    }

    private void ProcessQR(QRData _data)
    {
        _QR.CamStop();

        Address = _data.IP;
        LabelQRError.GetComponent<Text>().text = "接続中です...";
        Canvas.ForceUpdateCanvases();

        var res = CallServer("check");
        if (res == null)
        {
            // 接続不可
            LabelQRError.GetComponent<Text>().color = new Color32(255, 0, 0, 255);
            LabelQRError.GetComponent<Text>().text = "サーバに接続することができませんでした";
            _QR.CamStart();
            return;

        }
        else if (res != "authenticated" && _data.isPerformer)
        {
            // パフォーマーモード
            LabelQRError.GetComponent<Text>().color = new Color32(0, 0, 0, 255);
            LabelQRError.GetComponent<Text>().text = "確認中です...";
            Canvas.ForceUpdateCanvases();
            Pin = _data.PIN;

            res = CallServer("pin");
            if (res != "ok")
            {
                // 不正なPIN
                LabelQRError.GetComponent<Text>().color = new Color32(255, 0, 0, 255);
                LabelQRError.GetComponent<Text>().text = "PINが正しくありません";
                _QR.CamStart();
                return;
            }
        }

        // キャリブレーション・接続待機画面へ
        TransitionView("Forward", CalibWaitView);
        GameObject.Find("WSClient").GetComponent<WSClient>().Connect(Address, _data.isPerformer);

    }

    private void TransitionView(string _state, GameObject _view)
    {
        if (TransitionState == "None")
        {
            TransitionState = _state;
            rectTransition = _view.GetComponent<RectTransform>();
            t = TRANCOUNT;
        }
    }

    private string CallServer(string action)
    {
        string url = "http://" + Address + "/" + action;

        try
        {
            var wc = new System.Net.WebClient();

            string resText = "";
            if (action == "check")
            {
                byte[] resData = wc.DownloadData(url);
                resText = System.Text.Encoding.UTF8.GetString(resData);
            }
            else if (action == "pin")
            {
                var ps = new System.Collections.Specialized.NameValueCollection();
                ps.Add("pin", Pin);
                byte[] resData = wc.UploadValues(url, ps);
                resText = System.Text.Encoding.UTF8.GetString(resData);
            }
            else
            {
                throw new Exception();
            }

            wc.Dispose();

            return resText;
        }
        catch (Exception)
        {
            return null;
        }
    }

}
