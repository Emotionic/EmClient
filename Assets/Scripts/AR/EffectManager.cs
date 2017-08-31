using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    // ====================
    // PUBLIC
    // ====================

    /// <summary>
    /// AR情報からエフェクトを生成するための情報を計算し、エフェクトを生成します。
    /// </summary>
    public void GenEffect(EffectData _eff)
    {
        // 実装中...
        Debug.Log("GenEffect() が呼び出されました。");
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

