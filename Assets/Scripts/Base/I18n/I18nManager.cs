using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using SimpleJSON;

public enum I18nLang
{
    /// <summary> 中文简体 </summary>
    cn = 1,
    /// <summary> 英文 </summary>
    en = 2,
    /// <summary> 台湾繁体 </summary>
    tw = 3,
    /// <summary> 香港 </summary>
    hk = 4,
    /// <summary> 日语 </summary>
    jp = 5,
    /// <summary> 韩语 </summary>
    kor = 6,
    /// <summary> 印尼 </summary>
    id = 7,
    /// <summary> 俄罗斯 </summary>
    ru = 8,
    /// <summary> 泰文 </summary>
    th = 9,
    /// <summary> 马来西亚 </summary>
    mys = 10,
    /// <summary> 越南 </summary>
    vie = 11,
}


public class I18nManager : MonoSingleton<I18nManager>
{

    public const string I18N = "I18N";

    Dictionary<string, Dictionary<string, string>> i18nIDs
        = new Dictionary<string, Dictionary<string, string>>();

    Dictionary<string, JSONNode> i18nDatas = new Dictionary<string, JSONNode>();


    I18nLang _language = I18nLang.en;

    public I18nLang language
    {
        get => _language;
    }


    public JSONNode LoadData(string file)
    {

        JSONNode target = null;
        if (ApplicationSettings.Instance.IsUseHotfixBundle())
        {

            //AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.persistentDataPath + "/GameRes/luban/generatedatas/bytes/" + file + ".unity3d");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(PathHelper.abDirLOCPTH, "luban/generatedatas/bytes/" + file + ".unity3d"));
            TextAsset textAsset = assetBundle.LoadAsset<TextAsset>(file + ".json");
            target = JSON.Parse(textAsset.text);
            assetBundle.Unload(false);
            return target;
        }
        else
        {
#if UNITY_EDITOR
            target = JSON.Parse(File.ReadAllText(Application.dataPath + "/GameRes/LuBan/GenerateDatas/bytes/" + file + ".json", System.Text.Encoding.UTF8));
#endif
        }

        return target;


    }


    public void Add(string fileName, JSONNode data = null)
    {
        if (i18nDatas.ContainsKey(fileName))
            return;

        JSONNode res = data;
        if (res == null)
            res = LoadData(fileName);

        string dat = res.ToString();

        Dictionary<string, string> idMap = new Dictionary<string, string>();
        foreach (JSONNode node in res)
        {
            try
            {
                idMap.Add((string)node["en"], $"{fileName}.{(long)node["id"]}");
            }
            catch (Exception e)
            {
                DebugUtils.LogWarning($"【多语言】：值重复： {(string)node["en"]}");
                //DebugUtil.LogException(e);  
            }
            try
            {
                idMap.Add((string)node["key"], $"{fileName}.{(long)node["id"]}");
            }
            catch (Exception e)
            {
                DebugUtils.LogWarning($"【多语言】：键重复： {(string)node["key"]}");
                //DebugUtil.LogException(e);
            }
        }

        i18nIDs.Add(fileName, idMap);
        i18nDatas.Add(fileName, res);

        EventCenter.Instance.EventTrigger<I18nLang>(I18N, this._language);
    }

    public void Delete(string fileName)
    {
        this.i18nIDs.Remove(fileName);
        this.i18nDatas.Remove(fileName);
    }

    public void ChangeLanguage(I18nLang language, bool isForce = false)
    {
        if (!isForce && this._language == language)
            return;
        this._language = language;

        EventCenter.Instance.EventTrigger<I18nLang>(I18N, this._language);
    }

    public string T(string valueOrKey)
    {
        var values = i18nIDs.Values;
        for (int i = 0; i < values.Count; i++)
        {
            Dictionary<string, string> map = values.ElementAt(i);
            if (map.ContainsKey(valueOrKey))
            {
                string[] info = map[valueOrKey].Split('.');
                string fileName = info[0];
                long id = long.Parse(info[1]);
                JSONNode rows = i18nDatas[fileName];

                for (int j = 0; j < rows.Count; j++)
                {
                    if ((long)rows[j]["id"] == id)
                        return (string)rows[j][Enum.GetName(typeof(I18nLang), language)];
                }
            }
        }
        DebugUtils.LogWarning($"i18n is not find : {valueOrKey}");
        return valueOrKey;
    }
}


public static class I18nMgr
{
    public static string I18N => I18nManager.I18N;

    public static string T(string valueOrKey) => I18nManager.Instance.T(valueOrKey);

    public static void ChangeLanguage(I18nLang language, bool isForce = false) =>
        I18nManager.Instance.ChangeLanguage(language, isForce);

    public static void Delete(string fileName) => I18nManager.Instance.Delete(fileName);


    public static I18nLang language => I18nManager.Instance.language;

    public static void Add(string fileName, JSONNode data = null) => I18nManager.Instance.Add(fileName, data);

}