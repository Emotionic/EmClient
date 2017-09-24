using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChooseEffectsBehaviour : MonoBehaviour
{
    public GameObject Template;

    private SpriteLoader sploader;
    private Dictionary<string, bool> Effects;

    public List<string> GetEnableEffects()
    {
        return Effects.Where(eff => eff.Value).Select(eff => eff.Key).ToList();
    }

    private void Start()
    {
        // いいね！スプライトロード
        sploader = new SpriteLoader();
        sploader.Load("LikeEffects");

        // 全てのエフェクトのチェックボックスと画像の表示
        var parent = this.transform.Find("Viewport").Find("Content");
        var effectNames = sploader.GetSpritesName();
        Effects = effectNames.ToDictionary(eff => eff, val => true);
        foreach (var name in effectNames)
        {
            var item = Object.Instantiate(Template, parent) as GameObject;
            item.SetActive(true);
            var image = item.transform.Find("Image").GetComponent<Image>();
            image.sprite = sploader.GetSprite(name);
            image.color = Color.black;
            item.transform.Find("Toggle").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
            {
                Effects[name] = value;
            });
        }

    }
}