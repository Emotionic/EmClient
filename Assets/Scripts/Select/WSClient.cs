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

    private WebSocket ws = null;
    private Queue msgQueue;
    private bool isPerformer = false;
    private static bool isEndPerformed = false;

    private bool waitCalibrate = true;

    public void Connect(string _addr, bool _performer, bool _isAuthenticated)
    {
        ws = new WebSocket("ws://" + _addr + "/ws");
        isPerformer = _performer;
        isAuthenticated = _isAuthenticated;

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open");

            var msg = "SERV\n";
            msg += isPerformer ? "CALIB\n" : "AR\n";

            ws.Send(msg);

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
            } else
            {
                msg += JsonUtility.ToJson(_data);
            }    
        }

        msg += "\n";

        ws.Send(msg);
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
        if (ws == null) return;

        lock (msgQueue.SyncRoot)
        {
            foreach(var _msg in msgQueue)
            {
                var msg = ((string)_msg).Split();
                // メッセージの解析・処理
                if (msg[1] == "ENDPERFORM")
                {
                    isEndPerformed = true;
                    SceneManager.LoadScene("Select");
                }

                if (waitCalibrate)
                {
                    if (isPerformer && msg[1] == "CALIB_OK")
                    {
                        waitCalibrate = false;
                        CustomDefault = JsonUtility.FromJson<CustomData>(msg[2]);
                        SceneManager.LoadScene("Customize");

                    } else if (!isPerformer && msg[1] == "AR_OK")
                    {
                        waitCalibrate = false;
                        arData = JsonUtility.FromJson<ARData>(msg[2]);
                        SceneManager.LoadScene("AR");

                    }

                } else if (!isPerformer && SceneManager.GetActiveScene().name == "AR" && GameObject.Find("EffectManager") != null)
                {
                    if (msg[1] == "GENEFF")
                    {
                        var effmgr = GameObject.Find("EffectManager").GetComponent<EffectManager>();
                        effmgr.GenEffect(JsonUtility.FromJson<EffectData>(msg[2]));
                    }

                }
                    
            }

            msgQueue.Clear();
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
