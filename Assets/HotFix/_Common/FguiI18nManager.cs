using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FairyGUI;
using System;
using SBoxApi;
using GameMaker;

public class FguiI18nManager : MonoSingleton<FguiI18nManager>
{
    Dictionary<I18nLang, string> langFile = new Dictionary<I18nLang, string>()
    {
        [I18nLang.en] = "Assets/GameRes/Games/Console/Abs/lang_en.xml",
        [I18nLang.cn] = "Assets/GameRes/Games/Console/Abs/lang_cn.xml",
        [I18nLang.tw] = "Assets/GameRes/Games/Console/Abs/lang_tw.xml",
    };

    private Dictionary<I18nLang, string> langData = new Dictionary<I18nLang, string>();


    private const string PARAM_I18N_LANG = "PARAM_I18N_LANG";
    private void Awake()
    {
        //string defaultLangStr = Enum.GetName(typeof(I18nLang), I18nLang.cn);
        //string langStr = PlayerPrefs.GetString(PARAM_I18N_LANG, defaultLangStr);

        //string langStr = SBoxModel.Instance.language;
        //DebugUtils.LogError($"默认的语言： {langStr}");
        //SetI18n((I18nLang)Enum.Parse(typeof(I18nLang), langStr));
    }


    public I18nLang language => I18nManager.Instance.language;
    public void ChangeLanguage(I18nLang lang, Action onFinishCallback = null) => SetI18n(lang, onFinishCallback);


    // [Button]
    void SetI18n(I18nLang lang, Action onFinishCallback)
    {
        //if (I18nMgr.language == lang) return;

        //PlayerPrefs.SetString(PARAM_I18N_LANG, Enum.GetName(typeof(I18nLang), lang));
        //SBoxModel.Instance.language = Enum.GetName(typeof(I18nLang), lang);

        if (langData.ContainsKey(lang))
        {
            SetLanguage(lang, langData[lang], onFinishCallback);
            return;
        }
        
        if (langFile.ContainsKey(lang))
        {
            ResourceManager02.Instance.LoadAsset<TextAsset>(langFile[lang], (res) =>
            {
                langData.Add(lang, res.text);
                SetLanguage(lang, res.text, onFinishCallback);
            });
        }
    }


    void SetLanguage(I18nLang lang, string xmlData, Action onFinishCallback)
    {

        UIPackage.branch = Enum.GetName(typeof(I18nLang), lang);  //分支 

        if (string.IsNullOrEmpty(xmlData))
        {
            //DebugUtils.Log(xmlAsset.text);
            FairyGUI.Utils.XML xml = new FairyGUI.Utils.XML(xmlData);
            UIPackage.SetStringsSource(xml);
            FguiI18nTextAssistant.Instance.LoadFromXML(xml);
        }

        I18nMgr.ChangeLanguage(lang);

        onFinishCallback?.Invoke();
    }  
}
