using MeiZhouHeiBao_3993;

/// <summary>
/// 20000 的游戏号和正在进入的游戏一致时，由 GameSwitch 取这一台的 lastAck。
/// 新接断电重连的机台在此加一个 case。
/// </summary>
public static class FreeSpinLastAckBootstrap
{
    public static int GetLastAck(int gameId, int playerId)
    {
        switch (gameId)
        {
            case FreeSpinSessionStoreG3993.GameId:
                return FreeSpinSessionStoreG3993.GetLastAckOrUnconfirmed(playerId);
            default:
                return -1;
        }
    }
}
 