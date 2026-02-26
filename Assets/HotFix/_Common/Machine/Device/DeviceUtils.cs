using UnityEngine;
using Newtonsoft.Json;
public static class DeviceUtils
{
    /// <summary>
    /// 当前选择的纸钞机
    /// </summary>
    /// <returns></returns>
    public static BillerSelect GetCurSelectBiller()
    {
        string curSelectBiller01 = SBoxModel.Instance.billerList[SBoxModel.Instance.selectBillerNumber];

        string curSelectBiller = curSelectBiller01.ToLower();
        //DebugUtils.LogError($"@ GetCurSelectBiller = {curSelectBiller01}  == {curSelectBiller} == {SBoxModel.Instance.selectBillerNumber} == {JsonConvert.SerializeObject(SBoxModel.Instance.sboxBillerList)}");

        if (curSelectBiller.Contains("mei"))
        {
            return BillerSelect.MEI__AE2831_D5;
        }
        if (curSelectBiller.Contains("pyramid") && curSelectBiller.Contains("5000"))
        {
            return BillerSelect.PYRAMID__APEX_5000_SERIES;
        }
        if (curSelectBiller.Contains("pyramid") && curSelectBiller.Contains("7000"))
        {
            return BillerSelect.PYRAMID__APEX_7000_SERIES;
        }
        if (curSelectBiller.Contains("ict"))
        {
            return BillerSelect.ICT__PA7_TAO;
        }
        return BillerSelect.None;
    }

    /// <summary>
    /// 当前是选择sas纸钞机？
    /// </summary>
    /// <returns></returns>
    public static bool IsCurSasBiller()
    {
        BillerSelect bSele = GetCurSelectBiller();
        return bSele == BillerSelect.MEI__AE2831_D5;
    }


    public static string GetCoinOutScaleStr()
    {
        long ticketPerCredit = SBoxModel.Instance.CoinOutScaleTicketPerCredit;//BlackboardUtils.GetValue<long>("@console/coinOutScalePerCredit2Ticket");
        long creditPerTicket = SBoxModel.Instance.CoinOutScaleCreditPerTicket;//BlackboardUtils.GetValue<long>("@console/coinOutScalePerTicket2Credit");
        string str = ticketPerCredit > 1 ? $"{ticketPerCredit}:1" : $"1:{creditPerTicket}";
        return str;
    }
    /// <summary>
    /// 玩家所有积分能出多少张票
    /// </summary>
    /// <returns></returns>
    public static int GetCoinOutNum()
    {

        //DebugUtils.LogError($"CfgData = { JsonConvert.SerializeObject(SBoxModel.Instance.CfgData)}");
        int coinOutNum = 0;
        // 计算能换多少个币
        if (SBoxModel.Instance.CoinOutScaleCreditPerTicket > 1)
        {
            coinOutNum = (int)SBoxModel.Instance.myCredit / SBoxModel.Instance.CoinOutScaleCreditPerTicket;
        }
        else if (SBoxModel.Instance.CoinOutScaleTicketPerCredit > 1)
        {
            coinOutNum = (int)SBoxModel.Instance.myCredit * SBoxModel.Instance.CoinOutScaleTicketPerCredit;
        }else if (SBoxModel.Instance.CoinOutScaleCreditPerTicket ==1 && SBoxModel.Instance.CoinOutScaleTicketPerCredit ==1)
        {
            coinOutNum = (int)SBoxModel.Instance.myCredit;
        }
        else
        {
            DebugUtils.LogError($"参数报错： creditTickeOutScale = {SBoxModel.Instance.CoinOutScaleTicketPerCredit}  ticketCreditOutScale = {SBoxModel.Instance.CoinOutScaleCreditPerTicket}");
        }
        return coinOutNum;
    }



    /// <summary>
    /// 玩家所有积分能出多少张票
    /// </summary>
    /// <returns></returns>
    public static int GetCoinOutNum(int credit)
    {
        int coinOutNum = 0;
        // 计算能换多少个币
        if (SBoxModel.Instance.CoinOutScaleCreditPerTicket > 1)
        {
            coinOutNum = credit / SBoxModel.Instance.CoinOutScaleCreditPerTicket;
        }
        else if (SBoxModel.Instance.CoinOutScaleTicketPerCredit > 1)
        {
            coinOutNum = credit * SBoxModel.Instance.CoinOutScaleTicketPerCredit;
        }
        else if (SBoxModel.Instance.CoinOutScaleCreditPerTicket == 1 && SBoxModel.Instance.CoinOutScaleTicketPerCredit == 1)
        {
            coinOutNum = credit;
        }
        else
        {
            DebugUtils.LogError($"参数报错： creditTickeOutScale = {SBoxModel.Instance.CoinOutScaleTicketPerCredit}  ticketCreditOutScale = {SBoxModel.Instance.CoinOutScaleCreditPerTicket}");
        }
        return coinOutNum;
    }



    /*
    public int GetCoinOutNum(int Credit , float moneyPrtCoinOut)
    {
        // 计算币的数量
        double coinOutCount = Credit / pricePerCoin;
    } */

    /// <summary>
    /// count张票 能换多少积分
    /// </summary>
    /// <returns></returns>
    public static int GetCoinOutCredit(int count)
    {
        int coinOutCredit = 0;
        if (SBoxModel.Instance.CoinOutScaleCreditPerTicket > 1)
        {
            coinOutCredit = count * SBoxModel.Instance.CoinOutScaleCreditPerTicket;
        }
        else if (SBoxModel.Instance.CoinOutScaleTicketPerCredit > 1)
        {
            coinOutCredit = count / SBoxModel.Instance.CoinOutScaleTicketPerCredit;
        }else if (SBoxModel.Instance.CoinOutScaleCreditPerTicket ==1 && SBoxModel.Instance.CoinOutScaleTicketPerCredit==1)
        {
            coinOutCredit = count * 1;
        }
        else
        {
            DebugUtils.LogError($"参数报错： creditTickeOutScale = {SBoxModel.Instance.CoinOutScaleTicketPerCredit}  ticketCreditOutScale = {SBoxModel.Instance.CoinOutScaleCreditPerTicket}");
        }
        return coinOutCredit;
    }



    /*public static MoneyBoxMoneyOutInfo GetMoneyOutInfo(int credit)
    {
        int coinOutCount = GetCoinOutNum(credit);

        float moneyPerCoinOut = (float)GlobalsManager.Instance.GetAttribute(GlobalsManager.MoneyPerCoinOut);


        int targetMoneyOut = 0;
        int targetCoinOutCount = 0;
        int targetCoinOutCredit = 0;

        for (int i = coinOutCount; i > 0; i--)
        {
            float totalAmount = i * moneyPerCoinOut;
            if (totalAmount == (int)totalAmount)
            {
                targetCoinOutCount = i;
                targetMoneyOut = (int)totalAmount;
                targetCoinOutCredit = GetCoinOutCredit(targetCoinOutCount);
                break;
            }
        }

        return new MoneyBoxMoneyOutInfo()
        {
            coinOutCount = targetCoinOutCount,
            coinOutCredit = targetCoinOutCredit,
            moneyOut = targetMoneyOut,
        };

    }*/



    public static MoneyBoxMoneyInfo GetMoneyInInfo(int money , float moneyPerCoinIn)
    {

        int asCoinInCount = (int)(money / moneyPerCoinIn);

        int asCrefit = SBoxModel.Instance.CoinInScale * asCoinInCount;

        return new MoneyBoxMoneyInfo()
        {
            asCoinInCount = asCoinInCount,
            asCredit = asCrefit,
            money = money,
        };
    }

#if false
    public static MoneyBoxMoneyInfo GetMoneyOutInfo(int credit)
    {
        //int coinOutCount = GetCoinOutNum(credit);

        int coinInCount = credit / SBoxModel.Instance.CoinInScale;

        //float moneyPerCoinOut = (float)GlobalsManager.Instance.GetAttribute(GlobalsManager.MoneyPerCoinOut);
        float momeyPerCoinIn = MoneyBoxModel.Instance.moneyPerCoinIn;

        int targetMoneyOut = 0;
        int targetAsCoinInCount = 0;
        int targetAsCoinInCredit = 0;

        for (int i = coinInCount; i > 0; i--)
        {
            float totalAmount = i * momeyPerCoinIn;
            if (totalAmount == (int)totalAmount)
            {
                targetAsCoinInCount = i;
                targetMoneyOut = (int)totalAmount;
                targetAsCoinInCredit = targetAsCoinInCount * SBoxModel.Instance.CoinInScale;
                break;
            }
        }

        return new MoneyBoxMoneyInfo()
        {
            asCoinInCount = targetAsCoinInCount,
            asCredit = targetAsCoinInCredit,
            money = targetMoneyOut,
        };

    }




    /// <summary>
    /// 当前打印机
    /// </summary>
    /// <returns></returns>
    public static PrinterSelect GetCurSelectPrinter()
    {
        string curSelectPrinter = SBoxModel.Instance.sboxPrinterList[SBoxModel.Instance.selectPrinterNumber];

        curSelectPrinter = curSelectPrinter.ToLower();

        if (curSelectPrinter.Contains("itc"))
        {
            return PrinterSelect.ITC__GP_58CR;
        }
        if (curSelectPrinter.Contains("pti"))
        {
            return PrinterSelect.PTI__Phoenix;
        }
        if (curSelectPrinter.Contains("950") && curSelectPrinter.Contains("epic"))
        {
            return PrinterSelect.ITHACA__Epic950;
        }
        if (curSelectPrinter.Contains("950") && curSelectPrinter.Contains("jcm"))
        {
            return PrinterSelect.JCM950;
        }

        return PrinterSelect.None;
    }

    /// <summary>
    /// 当前是选择sas打印机？
    /// </summary>
    /// <returns></returns>
    public static bool IsCurSasPrinter()
    {
        PrinterSelect pSele = GetCurSelectPrinter();
        return pSele == PrinterSelect.ITHACA__Epic950 || pSele == PrinterSelect.JCM950;
    }

    public static bool IsCurQRCodePrinter()
    {
        PrinterSelect pSele = GetCurSelectPrinter();
        return pSele == PrinterSelect.PTI__Phoenix;
    }



    /// <summary>
    /// 当前选择的纸钞机
    /// </summary>
    /// <returns></returns>
    public static BillerSelect GetCurSelectBiller()
    {
        string curSelectBiller01 = SBoxModel.Instance.sboxBillerList[SBoxModel.Instance.selectBillerNumber];

        string curSelectBiller = curSelectBiller01.ToLower();
        //DebugUtils.LogError($"@ GetCurSelectBiller = {curSelectBiller01}  == {curSelectBiller} == {SBoxModel.Instance.selectBillerNumber} == {JsonConvert.SerializeObject(SBoxModel.Instance.sboxBillerList)}");

        if (curSelectBiller.Contains("mei"))
        {
            return BillerSelect.MEI__AE2831_D5;
        }
        if (curSelectBiller.Contains("pyramid") && curSelectBiller.Contains("5000"))
        {
            return BillerSelect.PYRAMID__APEX_5000_SERIES;
        }
        if (curSelectBiller.Contains("pyramid") && curSelectBiller.Contains("7000"))
        {
            return BillerSelect.PYRAMID__APEX_7000_SERIES;
        }
        if (curSelectBiller.Contains("ict") )
        {
            return BillerSelect.ICT__PA7_TAO;
        }
        return BillerSelect.None;
    }


    /// <summary>
    /// 当前是选择sas纸钞机？
    /// </summary>
    /// <returns></returns>
    public static bool IsCurSasBiller()
    {
        BillerSelect bSele = GetCurSelectBiller();
        return bSele == BillerSelect.MEI__AE2831_D5;
    }

    
    #endif  
}


public enum PrinterSelect
{
    None,
    /// <summary> 普通打印机 </summary>
    ITC__GP_58CR,
    /// <summary> 二维码打印机 </summary>
    PTI__Phoenix,
    /// <summary> Sas 950打印机 </summary>
    ITHACA__Epic950,
    /// <summary> Sas 950打印机</summary>
    JCM950,
}



public enum BillerSelect
{
    None,
    /// <summary> Sas 纸钞机 </summary>
    MEI__AE2831_D5,
    /// <summary> PYRAMID纸钞机 </summary>
    PYRAMID__APEX_5000_SERIES,
    /// <summary> PYRAMID纸钞机 </summary>
    PYRAMID__APEX_7000_SERIES,
    /// <summary> 普通纸钞机</summary>
    ICT__PA7_TAO,
}



/// <summary>
/// 钱箱允许退钱的金额
/// </summary>
public class MoneyBoxMoneyInfo
{
    /// <summary> 当前的钞票类似投入多少个币 </summary>
    public int asCoinInCount;
    /// <summary> 当前的钞票类似多少玩家积分 </summary>
    public int asCredit;
    /// <summary> 允许退的真钱数值 </summary>
    public int money;
}