using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QRReader : MonoBehaviour
{
    public bool isDetecting = false;
    public string Result = null;

    private WebCamTexture _webCam;

    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam) == false)
        {
            yield break;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
            yield break;

        _webCam = new WebCamTexture(devices[0].name, 350, 350, 30);
        CamStart();

    }

    private void Update()
    {
        if (_webCam != null)
        {
            Result = QRCodeHelper.Read(_webCam);
        }

    }

    public void CamStart()
    {
        _webCam.Play();
        this.transform.Find("Raw").GetComponent<RawImage>().texture = _webCam;
        this.transform.Find("Raw").GetComponent<RawImage>().transform.localRotation = Quaternion.Euler(0, 0, -90);
        isDetecting = true;
    }

    public void CamStop()
    {
        this.transform.Find("Raw").GetComponent<RawImage>().texture = null;
        _webCam.Stop();
        _webCam = null;
        isDetecting = false;
    }

}