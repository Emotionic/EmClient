using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class UIManager : MonoBehaviour
{
    public GameObject ModeView;
    public GameObject IPView;
    public GameObject PinView;
    public GameObject CalibWaitView;

    public Text LabelBtn;
    public Text LabelIPError;
    public Text LabelIP;
    public Text LabelPinError;
    public Text LabelQRError;
    public InputField InputIP;
    public InputField InputPin;

    public QRReader QR;

    private WSClient ws;

    private RectTransform rectTransition;
    private string TransitionState = "None";
    private const float TRANCOUNT = 0.5f;
    private float t;

    private bool isCustomizeMode;
    private QRReader _QR;
    private bool isDemo = false;
    private int _LogoTapCount = 0;
    private float _LogoTapped;

    private void Start()
    {
        // 縦画面にする
        Screen.orientation = ScreenOrientation.Portrait;

        _QR = QR.GetComponent<QRReader>();
        ws = GameObject.Find("WSClient").GetComponent<WSClient>();
    }

    private void Update()
    {
        if (_QR.isDetecting && !string.IsNullOrEmpty(_QR.Result) && _QR.Result != "error")
        {
            var qrdata = JsonConvert.DeserializeObject<QRData>(_QR.Result);
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
        if (isDemo)
        {
            GameObject.Find("DEMO").GetComponent<DemoManager>().isCustomizeMode = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("AR");
        }

        isCustomizeMode = false;
        LabelBtn.GetComponent<Text>().text = "接続";
        LabelIPError.GetComponent<Text>().text = "";

        TransitionView("Forward", IPView);
    }

    public void BtnCustom_OnClick()
    {
        if (isDemo)
        {
            GameObject.Find("DEMO").GetComponent<DemoManager>().isCustomizeMode = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Customize");
        }

        isCustomizeMode = true;
        LabelBtn.GetComponent<Text>().text = "次へ";
        LabelIPError.GetComponent<Text>().text = "";

        TransitionView("Forward", IPView);
    }

    public void BtnBack_OnClick()
    {
        TransitionView("Back", IPView);
    }

    public void BtnIP_OnClick()
    {
        ws.Addr = InputIP.GetComponent<InputField>().text;
        LabelIPError.GetComponent<Text>().text = "接続中です...";
        Canvas.ForceUpdateCanvases();

        var res = ws.RequestHTTP(Method.GET, "check");
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
        ws.Connect(isCustomizeMode, res == "authenticated");

    }

    public void BtnPin_OnClick()
    {
        // Pin入力完了
        LabelPinError.GetComponent<Text>().text = "確認中です...";
        Canvas.ForceUpdateCanvases();
        var pin = InputPin.GetComponent<InputField>().text;

        var res = ws.RequestHTTP(Method.POST, "pin", pin);
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
        ws.Connect(true, false);

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
                isDemo = true;

                var demo = new GameObject("DEMO");
                demo.AddComponent<DemoManager>();

                ModeView.transform.Find("Image").GetComponent<Image>().color = new Color32(69, 69, 69, 255);
                ModeView.transform.Find("Text").GetComponent<Text>().color = Color.white;
                TransitionView("Forward", ModeView);
            }
        }

        _LogoTapped = Time.time;
    }

    private void ProcessQR(QRData _data)
    {
        _QR.CamStop();

        ws.Addr = _data.IP;
        LabelQRError.GetComponent<Text>().text = "接続中です...";
        Canvas.ForceUpdateCanvases();

        var res = ws.RequestHTTP(Method.GET, "check");
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
            var pin = _data.PIN;

            res = ws.RequestHTTP(Method.POST, "pin", pin);
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
        ws.Connect(_data.isPerformer, res == "authenticated");

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

}
