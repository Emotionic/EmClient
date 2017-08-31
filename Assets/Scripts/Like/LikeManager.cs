using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LikeManager : MonoBehaviour
{
    public GameObject Overray;
    public GameObject Effect;
    public GameObject Arrow;

    public AudioClip SwipeSE;
    public int moveSpeed = 300;

    public static bool ReLike = false;

    private Image _overray;
    private int alpha = 0;
    private bool isTransition = false;

    private int Swipe_d = 300;
    private bool moveEffect = false;
    private Vector3 touchStartPos;
    private Vector3 touchEndPos;
    private Vector3 effectInitPos;

    private string effectName;
    private SpriteLoader sploader;

    private bool dialogShown = false;

    public void DoTransition()
    {
        var wsclient = GameObject.Find("WSClient").GetComponent<WSClient>();
        int idx = Random.Range(0, wsclient.arData.EnabledEffects.Length);
        effectName = wsclient.arData.EnabledEffects[idx];
        Debug.Log(effectName);
        Effect.GetComponent<Image>().sprite = sploader.GetSprite(effectName);

        isTransition = true;
        Effect.SetActive(true);
        Arrow.SetActive(true);

    }

    public void Drag()
    {
        if (moveEffect || dialogShown) return;
        Effect.transform.position = Input.mousePosition;
    }

    public void BeginDrag()
    {
        if (moveEffect || dialogShown) return;
        touchStartPos = Input.mousePosition;
    }

    public void EndDrag()
    {
        if (moveEffect || dialogShown) return;

        touchEndPos = Input.mousePosition;
        float directionX = touchEndPos.x - touchStartPos.x;
        float directionY = touchEndPos.y - touchStartPos.y;

        if (Mathf.Abs(directionX) < Mathf.Abs(directionY) && directionY > Swipe_d)
        {
            // 上にスワイプ
            Debug.Log("上にスワイプ : " + directionY + " : " + Swipe_d);
            this.GetComponent<AudioSource>().PlayOneShot(SwipeSE, 1.0f);
            moveEffect = true;

        } else
        {
            // 移動変位が足りない
            Effect.transform.position = effectInitPos;

        }

    }

    public void BtnAR_OnClick()
    {
        if (moveEffect || dialogShown) return;

        ReLike = false;
        // シーン遷移
        SceneManager.LoadScene("AR");

    }

    private void Start()
    {
        // いいね！スプライトロード
        sploader = new SpriteLoader();
        sploader.Load("LikeEffects");

        _overray = Overray.GetComponent<Image>();
        Effect.SetActive(false);
        Arrow.SetActive(false);
        effectInitPos = Effect.transform.position;

        // 乱数シードの設定
        Random.InitState(System.Environment.TickCount + 114514);

        // 縦画面にする
        Screen.orientation = ScreenOrientation.Portrait;

    }

    private void Update()
    {
        if (isTransition)
        {
            alpha += 10;
            _overray.color = new Color32(25, 25, 25, (byte)alpha);
            if (alpha >= 160) isTransition = false;
        }

        if (moveEffect)
        {
            var epos = Effect.transform.position;
            Effect.transform.position = new Vector3(epos.x, epos.y + moveSpeed, epos.z);
            if (Effect.transform.position.y >= Screen.height + Effect.GetComponent<RectTransform>().sizeDelta.y)
            {
                moveEffect = false;

                // いいね！の送信
                GameObject.Find("WSClient").GetComponent<WSClient>().SendLike(effectName);

                // ダイアログの表示と画面遷移
                ReLike = true;
                dialogShown = true;
                DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
                DialogManager.Instance.ShowSubmitDialog(
                    "いいね！を送信しました。",
                    (ret) => { SceneManager.LoadScene("Like"); }
                );

            }
        }

    }

}
