using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalSettingsManager : MonoBehaviour
{
    public LocalSettings localSettings;

    private string storagePath;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        storagePath = Application.persistentDataPath + "/local_settings";
    }

    private void Start ()
    {
        Debug.Log(storagePath);

        // デモ設定の読み込み
        Read();
        if (localSettings == null)
        {
            localSettings = new LocalSettings();
            localSettings.ChargedSE = "decision7";
            localSettings.FillingSE = "decision3";
            Write();
        }

    }

    public void Write()
    {
        JsonUtil.Write(storagePath, localSettings);
    }

    public void Read()
    {
        localSettings = JsonUtil.Read<LocalSettings>(storagePath);
    }

}
