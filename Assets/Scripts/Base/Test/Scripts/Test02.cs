using FairyGUI;
using GameMaker;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class Test02 : MonoBehaviour
{
    // Start is called before the first frame update


    private void Awake()
    {
        Debug.Log(PageManager.Instance.name);
        // EventCenter.Instance.AddEventListener<EventData<string>>("123", OnEventData);
        EventCenter.Instance.AddEventListener<EventData>("123", OnEventData);    
    }
    IEnumerator Start()
    {

        bool isNext = false;



#if true
        yield return GetStreamingAssetsVersion();

        while (!SQLitePlayerPrefs03.Instance.isInit)
        {
            yield return null;
        }

        while (!SQLiteAsyncHelper.Instance.isInit)
        {
            yield return null;
        }
#endif




        ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/_Common/Game Maker/Prefabs/INSTANCE.prefab", (pref) =>
        {
            GameObject go = GameObject.Instantiate(pref);
            go.name = "INSTANCE";
            DontDestroyOnLoad(go);
            isNext = true;
        });

        yield return new WaitUntil(()=>isNext==true);
        isNext = false;

        if (Application.platform == RuntimePlatform.Android)
            yield return new WaitForSeconds(5f);

        Debug.LogWarning("init finish .................");


        // PageManager.Instance.OpenPage(PageName.ConsolePopupI18nTest);
        PageManager.Instance.OpenPage(PageName.PusherEmperorsReinPopupERGameLoading);
        // 默认打开管理后台
        //PageManager.Instance.OpenPage(PageName.ConsolePageConsoleMain);
        //PageLaunch.Instance.Open();
    }


    void OnEventData(EventData res)
    {
        Debug.Log($"{res.value}");
    }


    [Button]
    void TestEventData()
    {
    
        EventCenter.Instance.EventTrigger<EventData<string>>("123",new EventData<string>("456","i am value"));
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    async void OpenABConsoleMainPage()
    {
        var res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePageConsoleMain);
    }

    /*
    private Window _winB;
    [Button]
    void OpenUIConsoleMainPage()
    {
        UIPackage.AddPackage("UI/Console");

        if (_winB == null)
            _winB = new PageConsoleMain();
        _winB.Show();

    }*/



    Dictionary<I18nLang, string> langFile = new Dictionary<I18nLang, string>()
    {
        [I18nLang.en] = "Assets/GameRes/Games/Console/Abs/lang_en.xml",
        [I18nLang.cn] = "Assets/GameRes/Games/Console/Abs/lang_cn.xml",
        [I18nLang.tw] = "Assets/GameRes/Games/Console/Abs/lang_tw.xml",
    };

    [Button]
    void SetI18n(I18nLang lang)
    {
        if (I18nMgr.language == lang) return;

        if (langFile.ContainsKey(lang))
        {
            ResourceManager02.Instance.LoadAsset<TextAsset>(langFile[lang], (res) =>
            {
                SetLanguage(lang, res);
            });
        }
    }

    void SetLanguage(I18nLang lang, TextAsset xmlAsset)
    {
        UIPackage.branch = Enum.GetName(typeof(I18nLang), lang); //分支 

        if (xmlAsset != null)
        {
            //Debug.Log(xmlAsset.text);
            FairyGUI.Utils.XML xml = new FairyGUI.Utils.XML(xmlAsset.text);
            UIPackage.SetStringsSource(xml);
            FguiI18nTextAssistant.Instance.LoadFromXML(xml);
        }

        I18nMgr.ChangeLanguage(lang);
    }

    [Button]
    async void ShowJson()
    {
        TextAsset jsn = await ResourceManager02.Instance.LoadAssetAsync<TextAsset>("Assets/GameRes/_Common/Game Maker/ABs/G152/Datas/game_info_g152.json");
        Debug.LogError(jsn.text);
    }
    /*
    int Time = 0;
    [Button]
    void ShowTip01()
    {
        TipPopupHandler.Instance.OpenPopup($"您好{Time++}");
        TipPopupHandler.Instance.OpenPopupOnce($"在的！");
    }*/












    private IEnumerator GetStreamingAssetsVersion()
    {
        // 拷贝所有dll
        GlobalData.streamingAssetsVersion = null;
        yield return ReadStreamingAsset<string>(PathHelper.versionSAPTH, (obj) =>
        {
            GlobalData.streamingAssetsVersion = JObject.Parse((string)obj);

            GlobalData.version = JObject.Parse((string)obj);
        }, (err) =>
        {
            throw new System.Exception(err);
        });
    }


    public static IEnumerator ReadStreamingAsset<T>(string srcPath, Action<object> onSuccessCallback, Action<string> onErrorCallback)
    {
#if UNITY_ANDROID
        using (UnityWebRequest reqSAAsset = UnityWebRequest.Get(srcPath))
        {
            yield return reqSAAsset.SendWebRequest();

            if (reqSAAsset.result == UnityWebRequest.Result.Success)
            {
                Type type = typeof(T);

                if (type == typeof(string))
                {
                    onSuccessCallback?.Invoke(reqSAAsset.downloadHandler.text);
                }
                else if (type == typeof(byte[]))
                {
                    onSuccessCallback?.Invoke(reqSAAsset.downloadHandler.data);
                }
                else
                {
                    Debug.LogError("T must string or byte[]");
                }
            }
            else
            {
                Debug.LogError($"Copy File Fail: {srcPath} error: {reqSAAsset.error}");
                onErrorCallback?.Invoke(reqSAAsset.error);
            }
        }
#else
        byte[] bytes = File.ReadAllBytes(srcPath);
        onSuccessCallback?.Invoke(bytes);
        yield return new WaitForEndOfFrame();
#endif
    }


}
