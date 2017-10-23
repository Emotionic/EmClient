using UnityEngine;
using System.IO;

public class DemoManager : MonoBehaviour
{
    public bool isCustomizeMode;

    private void Start()
    {
        DontDestroyOnLoad(this);

        Debug.Log("Demo mode started.");
    }

}



