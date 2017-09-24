using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehavior : MonoBehaviour
{
    public EffectManager effectManager;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.G))
        {
            var effdata = new EffectData();
            effdata.Name = "LINE_FootLeft";
            effdata.Position = new Vector3(Random.Range(-5.0f, 5.0f), 0, 9);
            effdata.Rotation = Quaternion.identity;
            effdata.Scale = Vector3.one;
            effdata.IsRainbow = true;

            effectManager.GenEffect(effdata);
        }

        if (Input.GetKey(KeyCode.E))
        {
            var effdata = new EffectData();
            effdata.Name = "punch";
            effdata.Position = new Vector3(Random.Range(-5.0f, 5.0f), 0, 9);
            effdata.Rotation = Quaternion.identity;
            effdata.Scale = Vector3.one;

            effectManager.GenEffect(effdata);
        }

	}
}
