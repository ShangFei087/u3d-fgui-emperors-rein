using GameMaker;
using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;


public class SBoxModelController : MonoSingleton<SBoxModelController>
{

    IEnumerator Start()
    {

        yield return OnInitParam();

        EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
    }
    protected override void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);

        base.OnDestroy();
    }

    IEnumerator OnInitParam() { 

        bool isNext = false;


#if !true
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


        LoadingPageAddSetingCount(6);

        LoadingPageInitSeting("open db ...");


        if (!SQLiteAsyncHelper.Instance.isConnect)
        {
            DebugUtils.LogError($"【Check Record】{SQLiteAsyncHelper.databaseName} is close");
            yield break;
        }


        SQLiteHelper.Instance.OpenDB(ConsoleTableName.DB_NAME, (connect) =>
        {
            isNext = true;
        });

        yield return new WaitUntil(() => isNext == true);
        isNext = false;


        ConsoleTableUtils.DeleteTables();

        ConsoleTableUtils.CheckOrCreatTableSysSetting();
        ConsoleTableUtils.CheckOrCreatTableCoinInOutRecord();
        ConsoleTableUtils.CheckOrCreatTableJackpotRecord();
        ConsoleTableUtils.CheckOrCreatTableSlotGameRecord();
        ConsoleTableUtils.CheckOrCreatTablePusherGameRecord();
        ConsoleTableUtils.CheckOrCreatTableLogEventRecord();
        ConsoleTableUtils.CheckOrCreatTableLogErrorRecord();
        ConsoleTableUtils.CheckOrCreatTableBussinessDayRecord();

        // 获取历史总投退数据

        Task asyncTask = ConsoleTableUtils.CheckOrCreatTableBet();
        while (!asyncTask.IsCompleted)
        {
            yield return null;
        }

        LoadingPageInitSeting("get table sys settings");

        ConsoleTableUtils.GetTableBet();

        TableBusniessTotalRecordAsyncManager.Instance.GetTotalBusniess();

        ConsoleTableUtils.GetTableSysSetting((TableSysSettingItem res) =>
        {
            DebugUtils.Log($"获取表数据成功 ： SysSetting");

            //_music = res.music;
            //_sound = res.sound;
            //SoundManager.Instance.SetBGMVolumScale(_music);
            //SoundManager.Instance.SetEFFVolumScale(_sound);

            // GSManager.Instance.SetMute(GSManager.Instance.IsMute);
            // GSManager.Instance.SetTotalVolumEfft(GSManager.Instance.TotalVolumeEff);
            // GSManager.Instance.SetTotalVolumMusic(GSManager.Instance.TotalVolumeMusic);

            // 加载多语言文件
            //foreach (string fileName in ConfigUtils.i18nLoadFile)
            //{
            //    I18nMgr.Add(fileName);
            //}

            // 多语言选择
            //I18nLang curLanguage = (I18nLang)Enum.Parse(typeof(I18nLang), res.language_number);
            //I18nMgr.ChangeLanguage(curLanguage);  // 这里有bug
            //FguiI18nManager.Instance.ChangeLanguage(I18nLang.cn);

            isNext = true;
        });


        yield return new WaitUntil(() => isNext == true);
        isNext = false;


        // 加载多语言文件
        foreach (string fileName in ConfigUtils.i18nLoadFile)
        {
            I18nMgr.Add(fileName);
        }


        // 声音和语言
        LoadingPageInitSeting("check language");


        MachineDeviceCommonBiz.Instance.CheckLanguage(() =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;


        LoadingPageInitSeting("get SBOX_READ_CONF");
        MachineDataManager02.Instance.RequestReadConf((data) =>
        {
            SBoxConfData res = (SBoxConfData)data;
            SBoxModel.Instance.SboxConfData = res;

            DebugUtils.Log("【SBoxConfData】 sboxConfData: " + JsonConvert.SerializeObject(res));
            DebugUtils.Log("【SBoxConfData】 投币比例: " + res.CoinValue); // 投币比例
            DebugUtils.Log("【SBoxConfData】 1分对应几票: " + res.scoreTicket);  // 1分对应几票
            DebugUtils.Log("【SBoxConfData】 1票对应几分: " + res.TicketValue); // 1票对应几分（彩票比例）
            DebugUtils.Log("【SBoxConfData】 机台编号: " + res.MachineId); // 机台编号
            DebugUtils.Log("【SBoxConfData】 线号: " + res.LineId); // 线号
            DebugUtils.Log("【SBoxConfData】 上下分: " + res.ScoreUpUnit); // 上下分

            isNext = true;
        }, (BagelCodeError err) =>
        {
            DebugUtils.LogError(err.msg);
        });

        yield return new WaitUntil(() => isNext == true);
        isNext = false;



        /*
        LoadingPageInitSeting("get SBOX_GET_ACCOUNT");
        MachineDataManager02.Instance.RequestGetPlayerInfo((System.Action<object>)((res) =>
        {
            //DebugUtils.LogWarning($"@@##  SBoxAccount == {JsonConvert.SerializeObject(res)} ");
            SBoxAccount data = (SBoxAccount)res;

            int pid = SBoxModel.Instance.pid;

            bool isOK = false;
            List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
            for (int i = 0; i < playerAccountList.Count; i++)
            {
                if (playerAccountList[i].PlayerId == pid)
                {
                    SBoxModel.Instance.SboxPlayerAccount = playerAccountList[i];

                    MainBlackboardController.Instance.SyncMyTempCreditToReal(false); //同步玩家金币

                    isOK = true;
                    break;
                }
            }
            if (!isOK)
                DebugUtils.LogError($" SBoxPlayerAccount is null , Where PlayerId = {pid}");

            isNext = true;

        }), (BagelCodeError err) =>
        {
            DebugUtils.LogError(err.msg);
        });

        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        
        */







        /*
        LoadingPageInitSeting("get hardware version");
        MachineDataManager02.Instance.RequestGetHardwareVersion((System.Action<object>)((res) => {

            SBoxModel.Instance.HardwareVer = (string)res;
            isNext = true;
        }), (err) => {
            DebugUtils.LogError(err.msg);
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        */

        LoadingPageInitSeting("get algorithm version");
        MachineDataManager02.Instance.RequestGetAlgorithmVersion((System.Action<object>)((res) => {

            SBoxModel.Instance.AlgorithmVer = (string)res;
            isNext = true;
        }), (err) => {
            DebugUtils.LogError($"{SBoxEventHandle.SBOX_IDEA_VERSION} : {err.msg}");
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;


        /*
        LoadingPageInitSeting("clear all sqllite");
        MachineRestoreManager.Instance.ClearRecordWhenCoding(() =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        */

        LoadingPageInitSeting("get table finish");

    }




    void OnPropertyChange(EventData res = null)
    {
        string name = res.name;
        switch (name)
        {
            case "SBoxModel/tableSysSetting":
                OnPropertyChangeTableSysSetting(res);
                break;
            case "SBoxModel/betAllowList":
                OnPropertyChangeBetAllowList(res);
                break;
            case "SBoxModel/tableBet":
                OnPropertyChangeTableBet(res);
                break;
        }
    }

    void OnPropertyChangeTableSysSetting(EventData res = null)
    {
        TableSysSettingItem sysSettingItem = (TableSysSettingItem)res.value;

        /*
        if (sysSettingItem.music != _music)
        {
            _music = sysSettingItem.music;
            SoundManager.Instance.SetBGMVolumScale(_music);
        }

        if (sysSettingItem.sound != _sound)
        {
            _sound = sysSettingItem.sound;
            SoundManager.Instance.SetEFFVolumScale(_sound);
        }
        */

        /*
        if (sysSettingItem.language_number != I18nMgr.language.ToString())
        {
            I18nLang curLanguage = (I18nLang)Enum.Parse(typeof(I18nLang), sysSettingItem.language_number);
            I18nMgr.ChangeLanguage(curLanguage);
        }*/

        string sql = SQLiteAsyncHelper.SQLUpdateTableData<TableSysSettingItem>(ConsoleTableName.TABLE_SYS_SETTING, sysSettingItem);
        SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
    }

    void OnPropertyChangeTableBet(EventData receivedEvent)
    {
        string sql = SQLiteAsyncHelper.SQLUpdateTableData<TableBetItem>(ConsoleTableName.TABLE_BET, (TableBetItem)receivedEvent.value);
        SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
    }
    void OnPropertyChangeBetAllowList(EventData receivedEvent)
    {
        List<BetAllow> betAllowList = (List<BetAllow>)receivedEvent.value;

        SBoxModel.Instance.tableBet.bet_list = JsonConvert.SerializeObject(betAllowList);

        // 这个已经改变 ，在方法OnPropertyChangeTableBet 
        //string sql = SQLiteAsyncHelper.SQLUpdateTableData<TableBetItem>(ConsoleTableName.TABLE_BET, tableBet);
        //SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

        List<long> betList = new List<long>();
        foreach (BetAllow nd in betAllowList)
        {
            if (nd.allowed == 1)
                betList.Add(nd.value);
        }
        SBoxModel.Instance.betList = betList;

    }














    void LoadingPageAddSetingCount(int count) => EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_INIT_SETTINGS_EVENT,
        new EventData<int>(GlobalEvent.AddSettingsCount, count));
    void LoadingPageInitSeting(string msg) => EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_INIT_SETTINGS_EVENT,
            new EventData<string>(GlobalEvent.InitSettings, msg));




}
