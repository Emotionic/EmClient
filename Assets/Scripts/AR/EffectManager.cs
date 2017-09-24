using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public GameObject Trail;

    private RainbowColor _RbColor;

    /// <summary>
    /// AR情報からエフェクトを生成します。
    /// </summary>
    public void GenEffect(EffectData _eff)
    {
        if (_eff.Name.StartsWith("LINE"))
        {
            // ラインエフェクトの生成

            Transform trail;
            var jointObj = GameObject.Find(_eff.Name).transform;
            jointObj.transform.position = _eff.Position;
            jointObj.transform.localRotation = _eff.Rotation;
            jointObj.transform.localScale = _eff.Scale;

            if (!jointObj.Find(Trail.name))
            {
                Instantiate(Trail, jointObj).name = "Trail";
            }

            trail = jointObj.Find(Trail.name);
            TrailRenderer tr = trail.GetComponent<TrailRenderer>();
            ParticleSystem[] pss =
                {
                    trail.Find("Hand Particle").GetComponent<ParticleSystem>(),
                    trail.Find("NG Hand Particle").GetComponent<ParticleSystem>()
                    };

            if (_eff.IsRainbow)
            {
                tr.startColor = _RbColor.Rainbow;
                foreach (ParticleSystem ps in pss)
                {
                    ps.startColor = _RbColor.Rainbow;
                }
            }
            else
            {
                tr.startColor = _eff.Color;
                foreach (ParticleSystem ps in pss)
                {
                    ps.startColor = _eff.Color;
                }
            }

        }
        else
        {
            PlayEffect(_eff);

        }

    }

    /// <summary>
    /// エフェクトを再生します。
    /// </summary>
    public void PlayEffect(EffectData _eff)
    {
        var target = new GameObject();
        target.transform.position = _eff.Position;
        target.transform.rotation = _eff.Rotation;
        target.transform.localScale = _eff.Scale;

        var effobj = new EffectObject(target, _eff.Name, _eff.DoLoop);
        effects.Add(effobj);
        Play(effobj);

    }

    // ====================
    // PRIVATE
    // ====================

    private List<EffectObject> effects;

    // --------------------
    // Unity Events
    // --------------------

    private void Awake()
    {
        effects = new List<EffectObject>();
        _RbColor = new RainbowColor(0, 0.001f);
    }

    private void Update()
    {
        lock (effects)
        {
            foreach (var eff in effects)
            {
                if (!eff.Handle.HasValue)
                    continue;

                var h = eff.Handle.Value;
                var tran = eff.Target.transform;

                if (h.exists)
                {
                    h.SetLocation(tran.position);
                    h.SetRotation(tran.rotation);
                    h.SetScale(tran.localScale);
                }
                else if (eff.DoLoop)
                {
                    Play(eff);
                }
                else
                {
                    h.Stop();
                    eff.Handle = null;
                }
            }

            // 使い終わったGameObjectの破棄
            foreach(var eff in effects)
            {
                if (eff.Handle == null)
                {
                    Destroy(eff.Target);
                }
            }

            // リストから削除
            effects.RemoveAll(eff => eff.Handle == null);
        }

        _RbColor.Update();

    }

    // --------------------
    // Effect Functions
    // --------------------

    private void Play(EffectObject _eff)
    {
        var tran = _eff.Target.transform;
        var h = EffekseerSystem.PlayEffect(_eff.Name, tran.position);
        h.SetRotation(tran.rotation);
        h.SetScale(tran.localScale);

        _eff.Handle = h;
    }

}

