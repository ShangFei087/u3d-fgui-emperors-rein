using System.Collections.Generic;

/// <summary>
/// UnitySendMessage 回调路由：InstanceKey -> PagController。
/// </summary>
public static class PagControllerRegistry
{
    private static readonly Dictionary<string, PagController> s_controllers =
        new Dictionary<string, PagController>();

    public static void Register(string instanceKey, PagController controller)
    {
        if (string.IsNullOrEmpty(instanceKey) || controller == null)
        {
            return;
        }

        s_controllers[instanceKey] = controller;
    }

    public static void Unregister(string instanceKey)
    {
        if (string.IsNullOrEmpty(instanceKey))
        {
            return;
        }

        s_controllers.Remove(instanceKey);
    }

    public static int ActiveCount => s_controllers.Count;

    public static PagController Resolve(string instanceKey)
    {
        if (string.IsNullOrEmpty(instanceKey))
        {
            return null;
        }

        s_controllers.TryGetValue(instanceKey, out PagController controller);
        return controller;
    }
}
