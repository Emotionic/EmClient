using UnityEngine;
using UnityEngine.SceneManagement;

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
