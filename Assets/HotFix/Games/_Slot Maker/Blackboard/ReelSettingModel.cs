using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PusherEmperorsRein;

namespace SlotMaker
{
    public class ReelSettingModel : MonoWeakSelectSingleton<ReelSettingModel>
    {

        [Serializable]
        public struct STReelSetting
        {
            public int reelIndex;

            public ReelSetting reelSetting;
        }


        public const string REEL_SETTING_REGULAR = "Reel Setting Regular";
        public const string REEL_SETTING_EXPECTATION_01 = "Reel Setting Expectation01";
        public const string REEL_SETTING_STOP = "Reel Setting Stop Immediately";
        public const string REEL_SETTING_AUTO = "Reel Setting Auto";

        /// <summary>
        /// 滚轮的默认参数设置
        /// </summary>
        [SerializeField]
        private ReelSetting defaultReelSetting = new ReelSetting()
        {
            timeTurnStartDelay = 0.12f,
            timeTurnOnce = 0.12f,
            timeReboundStart = 0.15f,
            timeReboundEnd = 0.08f,

            offsetYReboundStart = -200,  //反向
            offsetYReboundEnd = 20,  // 反向

            numReelTurn = 7,
            numReelTurnGap = 1,
        };

        /// <summary>
        /// 某列滚轮单独设置参数
        /// </summary>
        [SerializeField]
        private List<STReelSetting> eachReelSettings = new List<STReelSetting>();

        /// <summary>
        /// 代码动态设置滚轮参数（一般用不上）
        /// </summary>
        public Dictionary<int, ReelSetting> dynamicReelSettings = new Dictionary<int, ReelSetting>();




        /// <summary>
        /// 某列滚轮存在单独参数配置
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        bool ContainsEachReelSetting(int reelIndex)
        {
            for (int i = 0; i < eachReelSettings.Count; i++)
            {
                if (eachReelSettings[i].reelIndex == reelIndex)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取某列滚轮的配置
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        ReelSetting GetEachReelSettings(int reelIndex)
        {
            for (int i = 0; i < eachReelSettings.Count; i++)
            {
                if (eachReelSettings[i].reelIndex == reelIndex)
                    return eachReelSettings[i].reelSetting;
            }
            return null;
        }

        /// <summary>
        /// 滚轮转动一圈的时间
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public float GetTimeTurnOnce(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].timeTurnOnce != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].timeTurnOnce;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).timeTurnOnce != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).timeTurnOnce;
            }
            return defaultReelSetting.timeTurnOnce;
        }

        /// <summary>
        /// 获取起转回弹的时间
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public float GetTimeReboundStart(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].timeReboundStart != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].timeReboundStart;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).timeReboundStart != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).timeReboundStart;
            }
            return defaultReelSetting.timeReboundStart;
        }

        /// <summary>
        /// 获取结束回弹的时间
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public float GetTimeReboundEnd(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].timeReboundEnd != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].timeReboundEnd;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).timeReboundEnd != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).timeReboundEnd;
            }
            return defaultReelSetting.timeReboundEnd;
        }


        /// <summary>
        /// 单个滚轮起转的延时时间
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public float GetTimeTurnStartDelay(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].timeTurnStartDelay != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].timeTurnStartDelay;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).timeTurnStartDelay != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).timeTurnStartDelay;
            }

            return defaultReelSetting.timeTurnStartDelay;
        }


        /// <summary>
        /// 获取起转回弹的偏移量
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public float GetOffsetYReboundStart(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].offsetYReboundStart != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].offsetYReboundStart;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).offsetYReboundStart != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).offsetYReboundStart;
            }
            return defaultReelSetting.offsetYReboundStart;
        }

        /// <summary>
        /// 获取结束回弹的偏移量
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public float GetOffsetYReboundEnd(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].offsetYReboundEnd != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].offsetYReboundEnd;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).offsetYReboundEnd != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).offsetYReboundEnd;
            }
            return defaultReelSetting.offsetYReboundEnd;
        }

        /// <summary>
        /// 获取滚轮差量转动圈数
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public int GetNumReelTurnGap(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].numReelTurnGap != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].numReelTurnGap;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).numReelTurnGap != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).numReelTurnGap;
            }
            return defaultReelSetting.numReelTurnGap;

        }


        /// <summary>
        /// 单个滚轮常规要转动的圈数
        /// </summary>
        /// <param name="reelIndex"></param>
        /// <returns></returns>
        public int GetNumReelTurn(int reelIndex = -1)
        {
            if (dynamicReelSettings.ContainsKey(reelIndex) && dynamicReelSettings[reelIndex].numReelTurn != ReelSetting.NONE)
            {
                return dynamicReelSettings[reelIndex].numReelTurn;
            }
            if (ContainsEachReelSetting(reelIndex) && GetEachReelSettings(reelIndex).numReelTurn != ReelSetting.NONE)
            {
                return GetEachReelSettings(reelIndex).numReelTurn;
            }
            return defaultReelSetting.numReelTurn;
        }

    }
}