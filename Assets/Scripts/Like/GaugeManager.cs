using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    public LikeManager likeManager;
    public AudioClip ChargedSE;
    public AudioClip FillingSE;

    [Range(0.001f, 0.3f)]
    public float Pitch_diff = 0.01f;

    private const float accelerometerUpdateInterval = 1.0f / 60.0f;
    private const float lowPassKernelWidthInSeconds = 1.0f;
    private const float shakeDetectionThreshold = 2.0f * 2.0f;
 
    private float lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    private Vector3 lowPassValue = Vector3.zero;
    private Vector3 acceleration;
    private Vector3 deltaAcceleration;

    private int count = 0; // 振られた数
    private const int threshould = 15; // 何回振られたら一段階上げるか
    private const int level = 10; // 何段階あるか
    private int current_lv = 0; // 今何段階目か
    private bool confirmed = false; // 最初のダイアログが閉じられた

    private Image gauge_bg;

    public void ShakeForDebug()
    {
        if (count >= threshould * level) return;
        count += threshould;
        ProcessScale();
    }

    private void ProcessScale()
    {
        gauge_bg.fillAmount = (float)count / (threshould * level);
        if (count >= threshould * level)
        {
            if (SystemInfo.supportsVibration)
                Handheld.Vibrate();

            this.GetComponent<AudioSource>().pitch = 1.0f;
            this.GetComponent<AudioSource>().PlayOneShot(ChargedSE, 1.0f);

            likeManager.DoTransition();
        } else if ((count / threshould) > current_lv)
        {
            current_lv++;
            // this.GetComponent<PlayWave>().Play((PlayState)(current_lv + 3));
            this.GetComponent<AudioSource>().pitch = 1.0f + current_lv * Pitch_diff;
            this.GetComponent<AudioSource>().PlayOneShot(FillingSE, 1.0f);

        }
    }

    private void Start()
    {
        // シェイク初期化
        lowPassValue = Input.acceleration;

        gauge_bg = this.transform.Find("gauge_bg").GetComponent<Image>();

        // ダイアログの表示
        if (!LikeManager.ReLike)
        {
            DialogManager.Instance.SetLabel("OK", "キャンセル", "閉じる");
            DialogManager.Instance.ShowSubmitDialog("スマホを振って「いいね！」を送りましょう！", (ret) => { confirmed = true; });
        }

    }

    private void Update()
    {
        if (confirmed && count < threshould * level)
        {
            acceleration = Input.acceleration;
            lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
            deltaAcceleration = acceleration - lowPassValue;
            if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
            {
                // 振られた
                count++;
                ProcessScale();

            }
        }

    }
}
