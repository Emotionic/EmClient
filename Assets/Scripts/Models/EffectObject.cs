using UnityEngine;

public class EffectObject
{
    public GameObject Target;

    public EffekseerHandle? Handle;
    public string Name;

    public EffectObject(GameObject _target, string _name)
    {
        Target = _target;
        Name = _name;
    }

}