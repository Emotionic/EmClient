using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomManager : MonoBehaviour
{
    public Text Label;

    private WSClient ws;
	
	private void Start ()
    {
        ws = GameObject.Find("WSClient").GetComponent<WSClient>();
        Label.GetComponent<Text>().text = JsonUtility.ToJson(ws.customData);
	}
	
	private void Update ()
    {
		
	}
}
