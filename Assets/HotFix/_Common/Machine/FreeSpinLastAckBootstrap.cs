using MeiZhouHeiBao_3993;
using SBoxApi;

/// <summary>
/// 各机台把 lastAck 注册到 20200。新增断电重连机台时在此加一行 EnsureRegistered。
/// </summary>
public static class FreeSpinLastAckBootstrap
{
    public static void RegisterAll()
    {
        FreeSpinSessionStoreG3993.EnsureRegistered();
    }
}
