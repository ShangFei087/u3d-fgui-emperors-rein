using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FguiUtils : MonoBehaviour
{



    public static void TestScreen()
    {
        /*
        // 获取FairyGUI的StageCamera
        Camera stageCamera = GRoot.inst.GetCamera();
        // 获取视口分辨率
        float viewportWidth = stageCamera.pixelRect.width;
        float viewportHeight = stageCamera.pixelRect.height;
        // 打印分辨率
        DebugUtils.Log($"当前视口分辨率: stageCamera.pixelRect.width x height = {viewportWidth} x {viewportHeight}");
        */

        Stage stage = Stage.inst;
        // 获取视口大小
        float viewportWidth1 = stage.width;
        float viewportHeight1 = stage.height;
        // 打印分辨率
        DebugUtils.Log($"当前视口分辨率: stage.width x height = {viewportWidth1} x {viewportHeight1}");


        // 获取屏幕分辨率（可能受UI缩放影响）
        float screenWidth = GRoot.inst.width;
        float screenHeight = GRoot.inst.height;
        // 打印分辨率
        DebugUtils.Log($"当前UI视口分辨率: GRoot.inst.width x height = {screenWidth} x {screenHeight}");


        // 获取屏幕的宽度和高度（物理像素）
        int screenWidth1 = Screen.width;
        int screenHeight1 = Screen.height;
        // 打印分辨率
        DebugUtils.Log($"当前屏幕分辨率: Screen.width x height = {screenWidth1} x {screenHeight1}");

        /*
        // 获取当前屏幕设置的分辨率（物理像素）
        Resolution currentRes = Screen.currentResolution;
        // 打印分辨率和刷新率
        DebugUtils.Log($"当前屏幕设置分辨率: Screen.currentResolution.width x height = {currentRes.width} x {currentRes.height} @ {currentRes.refreshRate}Hz");
        */
    }





    /*
    // 递归查找指定ID的组件
    public GObject FindChildById(GComponent parent, string targetId)
    {
        foreach (GObject child in parent._children)
        {
            if (child.id == targetId)
                return child;

            if (child is GComponent)
            {
                GObject result = FindChildById(child as GComponent, targetId);
                if (result != null)
                    return result;
            }
        }
        return null;
    }

    // 使用示例
   // GObject target = FindChildById(this.contentPane, "xp0b9yqdglrj1i-n5_cwuo");

    */





    /*

    // 遍历组件及其所有后代节点，返回包含所有节点路径的列表
    public static List<string> GetAllNodePaths(GComponent rootComponent)
    {
        List<string> pathList = new List<string>();
        TraverseNode(rootComponent, "", pathList);
        return pathList;
    }

    // 递归遍历节点并生成路径，存入列表
    private static void TraverseNode(GObject node, string parentPath, List<string> pathList)
    {
        // 构建当前节点的完整路径
        string currentPath = string.IsNullOrEmpty(parentPath)
            ? $"{node.id}"
            : $"{parentPath}-{node.id}";

        // 将路径添加到列表
        pathList.Add(currentPath);  // new object[] {}

        // 如果当前节点是容器，递归处理其子节点
        if (node is GComponent component)
        {
            for (int i = 0; i < component.numChildren; i++)
            {
                GObject child = component.GetChildAt(i);
                TraverseNode(child, currentPath, pathList);
            }
        }
    }*/

    // 遍历组件及其所有后代节点，返回包含所有节点路径的列表
    public static List<object[]> GetAllNodePaths(GComponent rootComponent)
    {
        List<object[]> pathList = new List<object[]>();
        TraverseNode(rootComponent, "", pathList);
        return pathList;
    }

    // 递归遍历节点并生成路径，存入列表
    private static void TraverseNode(GObject node, string parentPath, List<object[]> pathList)
    {
        // 构建当前节点的完整路径
        string currentPath = string.IsNullOrEmpty(parentPath)
            ? $"{node.id}"
            : $"{parentPath}-{node.id}";

        // 将路径添加到列表
        pathList.Add(new object[] { currentPath, node });  // new object[] {}

        // 如果当前节点是容器，递归处理其子节点
        if (node is GComponent component)
        {
            for (int i = 0; i < component.numChildren; i++)
            {
                GObject child = component.GetChildAt(i);
                TraverseNode(child, currentPath, pathList);
            }
        }
    }



    public static List<T> GetAllNode<T>(GComponent rootComponent) where T : GObject
    {
        List<T> pathList = new List<T>();
        TraverseNode<T>(rootComponent, pathList);
        return pathList;
    }
    // 递归遍历节点并生成路径，存入列表
    private static void TraverseNode<T>(GObject node, List<T> pathList) where T : GObject
    {
        if (node is T matchingNode)
        {
            pathList.Add(matchingNode);
        }

        // 如果当前节点是容器，递归处理其子节点
        if (node is GComponent component)
        {
            for (int i = 0; i < component.numChildren; i++)
            {
                GObject child = component.GetChildAt(i);
                TraverseNode<T>(child, pathList);
            }
        }
    }


    public static List<GObject> GetAllNode(GComponent rootComponent, Func<GObject, bool> where)
    {
        List<GObject> pathList = new List<GObject>();
        TraverseNode(rootComponent, pathList, where);
        return pathList;
    }

    private static void TraverseNode(GObject node, List<GObject> pathList, Func<GObject, bool> where)
    {
        if (where(node))
        {
            pathList.Add(node);
        }

        // 如果当前节点是容器，递归处理其子节点
        if (node is GComponent component)
        {
            for (int i = 0; i < component.numChildren; i++)
            {
                GObject child = component.GetChildAt(i);
                TraverseNode(child, pathList, where);
            }
        }
    }
}
