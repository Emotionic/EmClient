using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoManager : MonoBehaviour
{

    public bool isCustomizeMode;

    private void Start()
    {
        DontDestroyOnLoad(this);
        Debug.Log("Demo mode started.");
    }

}


