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
        if (GameObject.Find("WSClient").GetComponent<WSClient>().arData.isLikeEnabled)
        {
            // シーン遷移
            SceneManager.LoadScene("Like");
        } else
        {
            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSubmitDialog(
                "いいね機能は無効になっています。",
                (ret) => {}
            );
        }

        

    }

}
