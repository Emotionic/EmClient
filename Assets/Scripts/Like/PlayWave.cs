using UnityEngine;
using System;

public class PlayWave : MonoBehaviour
{
    const double PI = Math.PI;

    public double gain = 3;
    private double increment;
    private double sampling_frequency = 48000;
    private double time;
    private PlayState playState = PlayState.None;
    private float playTime = 0;
    private float fadeScale;

    void SineWave(float[] data, int channels, double frequency)
    {
        increment = frequency * 2 * PI / sampling_frequency;
        for (var i = 0; i < data.Length; i = i + channels)
        {
            time = time + increment;
            data[i] = (float)(gain * Math.Sin(time) * fadeScale);

            if (playTime > 0)
            {
                fadeScale *= 1.5f;
                if (fadeScale > 1.0f)
                    fadeScale = 1.0f;
            }
            else
            {
                fadeScale -= .025f;
                if (fadeScale < 0.001f)
                {
                    fadeScale = 0.0f;
                    playState = PlayState.None;
                }
            }
            if (channels == 2)
                data[i + 1] = data[i];
            if (time > 2 * Math.PI)
                time = 0;
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double scale = 1;
        switch (playState)
        {
            case PlayState.C:
                SineWave(data, channels, 261.6255653005985 * scale);
                break;
            case PlayState.CS:
                SineWave(data, channels, 277.18263097687196 * scale);
                break;
            case PlayState.D:
                SineWave(data, channels, 293.66476791740746 * scale);
                break;
            case PlayState.DS:
                SineWave(data, channels, 311.1269837220808 * scale);
                break;
            case PlayState.E:
                SineWave(data, channels, 329.62755691286986 * scale);
                break;
            case PlayState.F:
                SineWave(data, channels, 349.2282314330038 * scale);
                break;
            case PlayState.FS:
                SineWave(data, channels, 369.99442271163434 * scale);
                break;
            case PlayState.G:
                SineWave(data, channels, 391.99543598174927 * scale);
                break;
            case PlayState.GS:
                SineWave(data, channels, 415.3046975799451 * scale);
                break;
            case PlayState.A:
                SineWave(data, channels, 440.0 * scale);
                break;
            case PlayState.AS:
                SineWave(data, channels, 466.1637615180899 * scale);
                break;
            case PlayState.B:
                SineWave(data, channels, 493.8833012561241 * scale);
                break;
            case PlayState.C2:
                SineWave(data, channels, 523.2511306011974 * scale);
                break;
        }
    }

    public void Play(PlayState _playState)
    {
        if (playState == PlayState.None)
        {
            fadeScale = 0.1f;
            time = 0.0f;
        }
        playState = _playState;
        playTime = 0.25f;
    }

    private void Update()
    {
        if (playTime <= 0)
            return;

        playTime -= Time.deltaTime;

    }

}

public enum PlayState
{
    None = 0,
    C,
    CS,
    D,
    DS,
    E,
    F,
    FS,
    G,
    GS,
    A,
    AS,
    B,
    C2
}