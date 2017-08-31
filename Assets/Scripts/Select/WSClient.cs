using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class WSClient : MonoBehaviour
{
    public CustomData customData;
    public ARData arData;

    private WebSocket ws = null;
    private Queue msgQueue;
    private bool isPerformer = false;

    private bool waitCalibrate = true;

    public void Connect(string _addr, bool _performer)
    {
        ws = new WebSocket("ws://" + _addr + "/ws");
        isPerformer = _performer;

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

    public void SendLike(string effname)
    {
        string msg = "";
        msg += "SERV\n";
        msg += "LIKE\n";
        msg += effname + "\n";

        ws.Send(msg);
    }

    private void Awake()
    {
        msgQueue = Queue.Synchronized(new Queue());
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        
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
                if (waitCalibrate)
                {
                    if (isPerformer && msg[1] == "CALIB_OK")
                    {
                        waitCalibrate = false;
                        customData = JsonUtility.FromJson<CustomData>(msg[2]);
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
