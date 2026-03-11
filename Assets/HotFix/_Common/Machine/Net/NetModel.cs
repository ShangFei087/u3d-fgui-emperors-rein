using System.Collections.Generic;

public class NetModel : BaseManager<NetModel>
{
    public List<string> legalClientUSNList = new List<string>();

    public bool matchingClient;

    public List<int> playerOnlineState;

    public void Init()
    {
        playerOnlineState = new List<int> { 0, 0, 0, 0, 0, 0 };
    }

}
