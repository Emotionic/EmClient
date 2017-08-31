using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteLoader
{
    Dictionary<string, Sprite> m_dic = new Dictionary<string, Sprite>();

    /**
     * 読み込み関数
     * @param   path    テクスチャのパス
     * @retval  読み込んだSpriteの数(エラーの場合-1)
     */
    public int Load(string path)
    {
        // 読み込み(Resources.LoadAllを使うのがミソ)
        Object[] list = Resources.LoadAll(path, typeof(Sprite));

        // listがnullまたは空ならエラーで返す
        if (list == null || list.Length == 0)
            return -1;

        int i, len = list.Length;

        // listを回してDictionaryに格納
        for (i = 0; i < len; ++i)
        {
            Debug.Log("Add : " + list[i]);

            m_dic.Add(list[i].name, list[i] as Sprite);
        }

        return len;
    }

    /**
     * Sprite取得関数
     * @param   name    取得するスプライト名
     * @retval  該当のSpriteインスタンス(なければnull)
     */
    public Sprite GetSprite(string name)
    {
        if (!m_dic.ContainsKey(name))
            return null;

        return m_dic[name];
    }

    public void Dispose()
    {
        m_dic.Clear();
        m_dic = null;
    }
}