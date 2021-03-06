﻿using System.Collections;
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
    private bool isDragging = false;
    private Vector3 touchStartPos;
    private Vector3 touchEndPos;
    private Vector3 effectInitPos;

    private readonly static Color32[] colors = { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255), new Color32(255, 255, 0, 255) };
    private int currentColor;
    private string effectName;
    private SpriteLoader sploader;

    private bool dialogShown = false;
    private bool hasFocus = true;

    public void DoTransition()
    {
        if (GameObject.Find("DEMO") != null)
        {
            var list = sploader.GetSpritesName();
            int idx = Random.Range(0, list.Count);
            effectName = list[idx];
        } else
        {
            var wsclient = GameObject.Find("WSClient").GetComponent<WSClient>();
            int idx = Random.Range(0, wsclient.arData.EnabledEffects.Count);
            effectName = wsclient.arData.EnabledEffects[idx];
        }
        
        Debug.Log(effectName);
        currentColor = Random.Range(0, 4);

        Effect.GetComponent<Image>().sprite = sploader.GetSprite(effectName);
        Effect.GetComponent<Image>().color = colors[currentColor];

        isTransition = true;
        Effect.SetActive(true);
        Arrow.SetActive(true);
        effectInitPos = Effect.transform.position;

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
        isDragging = true;
    }

    public void EndDrag()
    {
        if (moveEffect || dialogShown) return;
        isDragging = false;

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

    public void Touch()
    {
        if (moveEffect || dialogShown || isDragging) return;

        Debug.Log("Effect touched.");
        currentColor++;
        currentColor %= 4;
        Effect.GetComponent<Image>().color = colors[currentColor];

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

                if (GameObject.Find("DEMO") == null)
                {
                    // いいね！の送信
                    var data = new LikeData(effectName, colors[currentColor]);
                    GameObject.Find("WSClient").GetComponent<WSClient>().Send("LIKE", data);
                }

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

    private void OnApplicationFocus(bool _hasFocus)
    {
        if (!hasFocus && _hasFocus && dialogShown)
        {
            SceneManager.LoadScene("Like");
        }

        hasFocus = _hasFocus;
    }

}
