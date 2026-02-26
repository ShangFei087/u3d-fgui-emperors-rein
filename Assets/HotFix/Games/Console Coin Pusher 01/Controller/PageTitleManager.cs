using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PageTitleManager : Singleton<PageTitleManager>
{
    public  List<string> routePath = new List<string>();

    public void AddPageNode(string pageName)
    {
        if (pageName == "/")
        {
            routePath.Clear();
            routePath.Add("");
            return;
        }

        routePath.Add(pageName);
    }
    
    public void ClearPageNode() => routePath.Clear();

    public void RemoveLastPageNode()
    {
        routePath.RemoveAt(routePath.Count - 1);
    }

    public string GetPagePathName()
    {
        if (routePath.Count<=1)
            return "/ ";
        
        List<string> path = new List<string>();

        for (int i = 1; i < routePath.Count; i++)
        {
            path.Add(I18nMgr.T(routePath[i]));
        }
        string combinedPath = string.Join(" / ", path);
        return "/ " + combinedPath;
    }

}
