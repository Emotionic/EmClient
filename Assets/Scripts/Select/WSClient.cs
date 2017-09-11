using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class WSClient : MonoBehaviour
{
    public CustomData CustomDefault;
    public ARData arData;
    public bool isAuthenticated = false;
    public string Addr = null;

    private WebSocket ws = null;
    private Queue msgQueue;
    private bool isPerformer = false;
    private static bool isEndPerformed = false;

    private bool waitCalibrate = true;

    public void Connect(bool _performer, bool _isAuthenticated)
    {
        ws = new WebSocket("ws://" + Addr + "/ws");
        isPerformer = _performer;
        isAuthenticated = _isAuthenticated;

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open");
            Send(isPerformer ? "CALIB" : "AR", null);
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Data: " + e.Data);
            msgQueue.Enqueue(e.Data);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket Close");
        };

        ws.Connect();

    }

    public bool isConnected
    {
        get { return ws != null && ws.IsAlive; }
    }

    public void Send(string _action, object _data)
    {
        string msg = "";
        msg += "SERV\n";
        msg += _action + "\n";

        if (_data != null)
        {
            if (_data is string)
            {
                msg += _data;
            }
            else
            {
                msg += JsonUtility.ToJson(_data);
            }
        }

        msg += "\n";

        ws.Send(msg);
    }

    public string RequestHTTP(Method method, string action, string value = null)
    {
        string url = "http://" + Addr + "/" + action;

        try
        {
            var wc = new System.Net.WebClient();
            string resText = "";

            switch (method)
            {
                case Method.GET:
                    {
                        byte[] resData = wc.DownloadData(url);
                        resText = System.Text.Encoding.UTF8.GetString(resData);
                    }
                    break;

                case Method.POST:
                    {
                        if (value == null)
                            throw new ArgumentException();

                        var ps = new System.Collections.Specialized.NameValueCollection();
                        ps.Add("value", value);
                        byte[] resData = wc.UploadValues(url, ps);
                        resText = System.Text.Encoding.UTF8.GetString(resData);
                    }
                    break;

                default:
                    throw new ArgumentException();

            }

            wc.Dispose();

            return resText;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void Awake()
    {
        msgQueue = Queue.Synchronized(new Queue());
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (isEndPerformed)
        {
            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSubmitDialog(
                "演技が終了しました。",
                (ret) => { }
            );
            isEndPerformed = false;
        }
    }

    private void Update()
    {
        if (ws == null)
            return;

        lock (msgQueue.SyncRoot)
        {
            foreach (var _msg in msgQueue)
            {
                var msg = ((string)_msg).Split();

                // メッセージの解析・処理
                if (msg[1] == "ENDPERFORM")
                {
                    isEndPerformed = true;
                    SceneManager.LoadScene("Select");
                }
                else if (isPerformer && msg[1] == "CALIB_OK")
                {
                    CustomDefault = JsonUtility.FromJson<CustomData>(msg[2]);
                    if (waitCalibrate)
                    {
                        waitCalibrate = false;
                        SceneManager.LoadScene("Customize");
                    }
                }
                else if (!isPerformer && msg[1] == "AR_OK")
                {
                    arData = JsonUtility.FromJson<ARData>(msg[2]);
                    if (waitCalibrate)
                    {
                        waitCalibrate = false;
                        SceneManager.LoadScene("AR");
                    }
                }
                else if (!isPerformer && !waitCalibrate && msg[1] == "GENEFF" && GameObject.Find("EffectManager") != null)
                {
                    var effmgr = GameObject.Find("EffectManager").GetComponent<EffectManager>();
                    effmgr.GenEffect(JsonUtility.FromJson<EffectData>(msg[2]));
                }

            }

            msgQueue.Clear();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // 復帰
            if (!waitCalibrate && ws != null)
            {
                Send(isPerformer ? "CALIB" : "AR", null);
            }

        }

    }

    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }

    }

}

public enum Method
{
    GET,
    POST
}

