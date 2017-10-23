using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour
{
    public Text LabelDebug;
    public ARTrackHandler Handler;
    public GameObject Trail;

    private readonly string[] _EffectNames = { "StairBroken", "punch", "laser" };

    private readonly Dictionary<string, Gesture> _GestureRelation = new Dictionary<string, Gesture>()
    {
        { "Jump", Gesture.Jump },
        { "Punch", Gesture.Punch },
        {"ChimpanzeeClap_Left", Gesture.ChimpanzeeClap  },
        {"ChimpanzeeClap_Right", Gesture.ChimpanzeeClap },
        {"Daisuke", Gesture.Daisuke },
        {"Kamehameha", Gesture.Kamehameha },
    };

    private readonly Dictionary<Effect, string> _EffectRelation = new Dictionary<Effect, string>()
    {
        { Effect.Impact, "StairBroken" }
    };

    private Dictionary<Effect, GameObject> _EffectPrefabs;

    /// <summary>
    /// AR情報からエフェクトを生成します。
    /// </summary>
    public void GenEffect(EffectData _eff, bool useAR)
    {
        LabelDebug.text = Handler.IsTracking ? "Tracking" : "Lost";
        if (useAR && !Handler.IsTracking)
            return;

        if (_eff.Name.StartsWith("LINE"))
        {
            // ラインエフェクトの生成
            Transform trail;
            var jointObj = GameObject.Find(_eff.Name).transform;
            jointObj.transform.position = Camera.main.ViewportToWorldPoint(_eff.Position);
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

            tr.startColor = _eff.Color;
            foreach (ParticleSystem ps in pss)
            {
                ps.startColor = _eff.Color;

            }

        }
        else
        {
            //var effect = (Effect)Enum.Parse(typeof(Effect), _eff.Name);
            //if (_EffectRelation.ContainsKey(effect))
            //{
            //    _eff.Name = _EffectRelation[effect];
            //    PlayEffect(_eff);
            //}
            //else
            //{
            //    var effe = Instantiate(_EffectPrefabs[effect]);
            //    effe.transform.position = _eff.Position;
            //    effe.transform.rotation = _eff.Rotation;
            //    effe.GetComponent<ParticleSystem>().Play(true);
            //    Destroy(effe.gameObject, 10);
            //}
        }
    }

    /// <summary>
    /// エフェクトを再生します。
    /// </summary>
    public void PlayEffect(EffectData _eff)
    {
        var target = new GameObject();
        target.transform.position = Camera.main.ViewportToWorldPoint(_eff.Position);
        target.transform.rotation = _eff.Rotation;
        target.transform.localScale = _eff.Scale;

        var effobj = new EffectObject(target, _eff.Name);
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

        _EffectPrefabs = new Dictionary<Effect, GameObject>()
    {
        { Effect.Beam, Resources.Load<GameObject>("Prefabs/KamehameCharge") },
        { Effect.Ripple, Resources.Load<GameObject>("Prefabs/punch")},
        {Effect.Ripple, Resources.Load<GameObject>("Prefabs/clap_effe") }
    };
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
                else
                {
                    h.Stop();
                    eff.Handle = null;
                }
            }

            // 使い終わったGameObjectの破棄
            foreach (var eff in effects)
            {
                if (eff.Handle == null)
                {
                    Destroy(eff.Target);
                }
            }

            // リストから削除
            effects.RemoveAll(eff => eff.Handle == null);
        }

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
