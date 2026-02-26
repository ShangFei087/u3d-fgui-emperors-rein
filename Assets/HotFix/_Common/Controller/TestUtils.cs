
using UnityEngine;
public static class TestUtils  
{

   
    public static void CheckReporter()
    {
        /*
        GameObject goReporter = GOFind.FindObjectIncludeInactive("Reporter");
        if (ApplicationSettings.Instance.isRelease)
        {
            if (goReporter != null)
                GameObject.DestroyImmediate(goReporter);
        }
        else
        {
            if (SBoxModel.Instance.isUseReporterPage)
            {
                if (goReporter == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Common/Prefabs/Reporter");
                    // 实例化预制体到当前场景
                    GameObject go = GameObject.Instantiate(prefab);
                    // 设置实例化后的游戏对象名称
                    go.name = "Reporter";
                    if (go.GetComponent<GOFindTag>() == null)
                        go.AddComponent<GOFindTag>();

                    go.SetActive(true);
                }
            }
            else
            {
                if (goReporter != null)
                {
                    DebugUtils.LogError("删除 Reporter");
                    GameObject.DestroyImmediate(goReporter);
                }
            }
        }  
        */

        GameObject goReporter = GOFind.FindObjectIncludeInactive("Reporter");

        if (SBoxModel.Instance.isUseReporterPage)
        {
            if (goReporter == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Common/Prefabs/Reporter");
                // 实例化预制体到当前场景
                GameObject go = GameObject.Instantiate(prefab);
                // 设置实例化后的游戏对象名称
                go.name = "Reporter";
                if (go.GetComponent<GOFindTag>() == null)
                    go.AddComponent<GOFindTag>();

                go.SetActive(true);
            }
        }
        else
        {
            if (goReporter != null)
            {
                //DebugUtils.LogError("删除 Reporter");
                GameObject.DestroyImmediate(goReporter);
            }
        }    
    }



    public static void CheckTestManager()
    {
        /*
        if (ApplicationSettings.Instance.isRelease)
        {
            TestManager.Instance.SetToolActive(false);
        }
        else
        {
            TestManager.Instance.SetToolActive(SBoxModel.Instance.isUseTestTool);
        }*/

        TestManager.Instance.SetToolActive(SBoxModel.Instance.isUseTestTool);
    }
}
