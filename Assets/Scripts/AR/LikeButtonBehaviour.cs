using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LikeButtonBehaviour : MonoBehaviour
{
    public void Start()
    {
        // 横画面にする
        Screen.orientation = ScreenOrientation.LandscapeLeft;

    }

    public void LikeButton_OnClick()
    {
        // シーン遷移
        SceneManager.LoadScene("Like");

    }

}
