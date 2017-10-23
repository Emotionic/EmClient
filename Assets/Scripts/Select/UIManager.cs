using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class UIManager : MonoBehaviour
{
    public GameObject QRView;
    public GameObject IPModeView;
    public GameObject PinView;
    public GameObject CalibWaitView;
    public GameObject Blocker;

    private Text _LabelQR;
    private Text _LabelIPMode;
    private Text _LabelPin;
    private InputField _InputIP;
    private InputField _InputPin;

    public Text LabelVersion;

    public QRReader QR;
    private WSClient ws;

    private RectTransform rectTransition;
    private string TransitionState = "None";
    private const float TRANCOUNT = 0.5f;
    private float t;

    private bool isCustomizeMode;
    private QRReader _QR;

    // テスト用
    private bool isDemo = false;
    private int _LogoTapCount = 0;
    private float _LogoTapped;
    private int _VersionTapCount = 0;
    private float _VersionTapped;
    private int _CurrentPage = 0;

    private void Start()
    {
        // 縦画面にする
        Screen.orientation = ScreenOrientation.Portrait;

        _QR = QR.GetComponent<QRReader>();
        ws = GameObject.Find("WSClient").GetComponent<WSClient>();

        // バージョン情報の表示
        LabelVersion.text = "Ver. " + Application.version;

        // UIパーツの取得
        _LabelIPMode = IPModeView.transform.Find("Text").GetComponent<Text>();
        _LabelPin = PinView.transform.Find("Text").GetComponent<Text>();
        _LabelQR = QRView.transform.Find("Text").GetComponent<Text>();
        _InputIP = IPModeView.transform.Find("InputField").GetComponent<InputField>();
        _InputPin = PinView.transform.Find("InputField").GetComponent<InputField>();

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
        // QR -> IPMode
        if (_QR.isDetecting) _QR.CamStop();
        TransitionView("Forward", IPModeView);
    }

    public void BtnModeIP_OnClick(bool _isPerformer)
    {
        if (isDemo)
        {
            GameObject.Find("DEMO").GetComponent<DemoManager>().isCustomizeMode = _isPerformer;
            UnityEngine.SceneManagement.SceneManager.LoadScene(_isPerformer ? "Customize" : "Like");
            return;
        }

        ws.Addr = _InputIP.text;
        _LabelIPMode.text = "接続中です...";
        Canvas.ForceUpdateCanvases();

        isCustomizeMode = _isPerformer;

        var res = ws.RequestHTTP(Method.GET, "check");
        if (res == null)
        {
            // 接続不可
            _LabelIPMode.color = Color.red;
            _LabelIPMode.fontSize = 32;
            _LabelIPMode.text = "サーバに接続することができませんでした" + Environment.NewLine + "IPアドレスをご確認ください";
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
        _LabelPin.text = "確認中です...";
        Canvas.ForceUpdateCanvases();
        var pin = _InputPin.text;

        var res = ws.RequestHTTP(Method.POST, "pin", pin);
        if (res == null)
        {
            // 接続エラー
            _LabelPin.color = Color.red;
            _LabelPin.fontSize = 32;
            _LabelPin.text = "接続時にエラーが発生しました";
            return;
        }
        else if (res != "ok")
        {
            // 不正なPIN
            _LabelPin.color = Color.red;
            _LabelPin.fontSize = 32;
            _LabelPin.text = "PINが正しくありません";
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

                if (GameObject.Find("DEMO") == null)
                {
                    var demo = new GameObject("DEMO");
                    demo.AddComponent<DemoManager>();
                }

                IPModeView.transform.Find("Image").GetComponent<Image>().color = new Color32(69, 69, 69, 255);
                _LabelIPMode.text = "DEMO MODE\nモードを選択してください";
                _InputIP.interactable = false;

                TransitionView("Forward", IPModeView);
            }
        }

        _LogoTapped = Time.time;
    }

    public void Version_OnPointerClicked()
    {
        if (isDemo) return;

        if (Time.time - _VersionTapped >= 3.0f)
            _VersionTapCount = 0;
        else
        {
            _VersionTapCount++;
            if (_VersionTapCount >= 3)
            {
                Blocker.SetActive(true);
            }
        }

        _VersionTapped = Time.time;
    }

    public void Blocker_OnPointerClicked()
    {
        _CurrentPage++;
        _CurrentPage %= 4;

        switch (_CurrentPage)
        {
            case 0:
                IPModeView.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                IPModeView.GetComponent<RectTransform>().anchorMax = new Vector2(2, 1);
                PinView.GetComponent<RectTransform>().anchorMin = new Vector2(2, 0);
                PinView.GetComponent<RectTransform>().anchorMax = new Vector2(3, 1);
                CalibWaitView.GetComponent<RectTransform>().anchorMin = new Vector2(3, 0);
                CalibWaitView.GetComponent<RectTransform>().anchorMax = new Vector2(4, 1);
                break;
            case 1: TransitionView("Forward", IPModeView); break;
            case 2: TransitionView("Forward", PinView); break;
            case 3: TransitionView("Forward", CalibWaitView); break;
        }   
    }

    private void ProcessQR(QRData _data)
    {
        _QR.CamStop();

        ws.Addr = _data.IP;
        _LabelQR.text = "接続中です...";
        Canvas.ForceUpdateCanvases();

        var res = ws.RequestHTTP(Method.GET, "check");
        if (res == null)
        {
            // 接続不可
            _LabelQR.color = Color.red;
            _LabelQR.fontSize = 32;
            _LabelQR.text = "サーバに接続することができませんでした";
            _QR.CamStart();
            return;

        }
        else if (res != "authenticated" && _data.isPerformer)
        {
            // パフォーマーモード
            _LabelQR.text = "確認中です...";
            Canvas.ForceUpdateCanvases();
            var pin = _data.PIN;

            res = ws.RequestHTTP(Method.POST, "pin", pin);
            if (res != "ok")
            {
                // 不正なPIN
                _LabelQR.color = Color.red;
                _LabelQR.fontSize = 32;
                _LabelQR.text = "PINが正しくありません";
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
