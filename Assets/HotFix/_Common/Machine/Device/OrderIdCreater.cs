using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderIdCreater : MonoSingleton<OrderIdCreater>
{

    public const string CoinOut = "coin_out";
    public const string CoinIn = "coin_in";
    public const string ScoreDown = "score_down";
    public const string ScoreUp = "score_up";
    public const string MoneyBoxQrCodeOut = "money_box_qrcode_out";
    public const string MoneyBoxQrCodeIn = "money_box_qrcode_in";
    public const string IOTCoinOut = "iot_coin_out";
    public const string IOTCoinIn = "iot_coin_in";
    Dictionary<string, int> orderType = new Dictionary<string, int>()
    {
        [CoinOut] = 80000,
        [CoinIn] = 80001,
        [ScoreDown] = 80010,
        [ScoreUp] = 80011,
        [MoneyBoxQrCodeOut] = 80020,
        [MoneyBoxQrCodeIn] = 80021,
        [IOTCoinOut] = 80030,
        [IOTCoinIn] = 80031,
    };

    const string ORDER_ID_NUMBER = "ORDER_ID_NUMBER";
    JSONNode _orderIdNumber = null;
    JSONNode orderIdNumber
    {
        get
        {
            if (_orderIdNumber == null)
            {
                //JSONNode node = JSONNode.Parse("{}");
                //node.Add(CoinIn, 0);
                //node.Add(CoinOut, 0);
                //node.Add(ScoreUp, 0);
                //node.Add(ScoreDown, 0);
                //node.Add(MoneyBoxQrCodeIn, 0);
                //node.Add(MoneyBoxQrCodeOut, 0);
                //string target = SQLitePlayerPrefs02.Instance.GetString(ORDER_ID_NUMBER, node.ToString());
                string target = SQLitePlayerPrefs03.Instance.GetString(ORDER_ID_NUMBER, "{}");
                _orderIdNumber = JSONNode.Parse(target);
            }

            return _orderIdNumber;
        }
    }



    public string CreatOrderId(string type)
    {
        if (!orderType.ContainsKey(type))
        {
            DebugUtils.LogError($"can not find type:{type} in {ORDER_ID_NUMBER}");
            return Guid.NewGuid().ToString();
        }

        if (!orderIdNumber.HasKey(type))
        {
            orderIdNumber.Add(type, 0);
        }

        int number = orderIdNumber[type] + 1;
        if (number > 1000)
            number = 1;
        orderIdNumber[type] = number;
        SQLitePlayerPrefs03.Instance.SetString(ORDER_ID_NUMBER, orderIdNumber.ToString());

        long curTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DateTime localDateTime01 = DateTimeOffset.UtcNow.LocalDateTime;
        string timeStr = localDateTime01.ToString("yyyyMMddHHmmss");

        return $"{orderType[type]}-{UnityEngine.Random.Range(100, 1000)}-{timeStr}{curTimeMS}-{number}";
    }
}
