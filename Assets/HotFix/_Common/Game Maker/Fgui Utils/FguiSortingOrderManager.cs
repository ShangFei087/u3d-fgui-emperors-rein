using FairyGUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


/*
public enum Layer
{
    Background,
    Base,
    Midground,
    Foreground,
    Foregroundm,
    EffectMidground,
    EffectForeground,
    PanelBase,
    PanelMidground,
    PanelForeground,
    Popup,
    Interaction,
    Overlay,
}
*/
public  class SortingOrderInfo{
    // 原来父级
    public GComponent fromeNode;
    // 新父级
    public GComponent toNode;
    public Vector2 fromLocalPos;
    public Func<GComponent,int> funcFromChildIndex;
    public Func<GComponent,int> funcToChildIndex;
    public string mark = "";
    //public int fromChildIndex;
    //Vector2 toLocalPos;
    //Vector2 nowToLocalPos;
}
public class FguiSortingOrderManager : Singleton<FguiSortingOrderManager>
{
    private Dictionary<GComponent, List<object[]>> requestIndexTasks = new Dictionary<GComponent, List<object[]>>();
    void RequestSetIndex(GObject goChild, int index)
    {
        //DebugUtils.LogError($"i am set index: {goChild.parent.parent.name}  index: {index}");
        if (!requestIndexTasks.ContainsKey(goChild.parent))
        {
            requestIndexTasks.Add(goChild.parent, new List<object[]>());
        }

        List < object[]> lst = requestIndexTasks[goChild.parent];
        bool isAdd = false;
        for (int i=0; i<lst.Count;i++)
        {
            object[] item = lst[i];

            int idxExpect = (int)item[0];
            if (index <= idxExpect)
            {
                try
                {
                    lst.Insert(i,new object[]{index,goChild});
                    isAdd = true;
                }
                catch (Exception e)
                {
                    DebugUtils.LogError($" idx: {idxExpect} lst.count: {lst.Count} index: {index} name: {goChild.parent.parent.name}");
                    throw e;
                }
                break;
            }
        }
        if(!isAdd)
            lst.Add(new object[]{index,goChild});
        
        Timers.inst.Remove(DoRequestSetIndex);
        Timers.inst.Add(0.02f,1,DoRequestSetIndex);
    }

    
    void DoRequestSetIndex(object param)
    {
        //DebugUtils.LogError("i am here DoRequestSetIndex !!!");
        while (requestIndexTasks.Count>0)
        {
            KeyValuePair<GComponent,List<object[]>> task = requestIndexTasks.ElementAt(requestIndexTasks.Count - 1);
            requestIndexTasks.Remove(task.Key);
            
            /* [note] : fgui的 “GObject A” 的 child index 和 “GObject A” 对应的 u3d  “GameObject A” 的 child index 没有一一对应！
            object[] item1 = task.Value[0];
            GObject goNode1 = (GObject)item1[1];
            string res = "";
            for (int i = 0; i < goNode1.parent.numChildren; i++)
            {
                res += goNode1.parent.GetChildAt(i).name;
            }
            string res1 = "";
            for (int i = 0; i < task.Value.Count; i++)
            {
                res1 += $"#{task.Value[i][0]}/{((GObject)(task.Value[i][1])).name}";
            }
            DebugUtils.LogError($" lst: {res1}  names:{res}");
            */
            for (int i = 0; i < task.Value.Count; i++)
            {
                object[] item = task.Value[i];
                int index = (int)item[0];
                GObject goNode = (GObject)item[1];
                goNode.parent.SetChildIndex(goNode,index);
            }
            /*
            res = "";
            for (int i = 0; i < goNode1.parent.numChildren; i++)
            {
                res += goNode1.parent.GetChildAt(i).name;
            }
            DebugUtils.LogError($"finish names:{res}");
            */
        }
    }
    

    Dictionary<GObject, SortingOrderInfo> nodes = new Dictionary<GObject, SortingOrderInfo>();
    public void ChangeSortingOrder(GObject goTarget, GComponent toNode, string mark = "", Func<GComponent,int> funcToChildIndex = null, Func<GComponent,int> funcFromChildIndex = null)
    {
        SortingOrderInfo info = new SortingOrderInfo()
        {
            mark = mark,
            fromeNode = goTarget.parent,
            toNode = toNode,
            fromLocalPos = new Vector2(goTarget.x, goTarget.y),
            funcFromChildIndex = funcFromChildIndex,
            funcToChildIndex = funcToChildIndex,
            //fromChildIndex = goTarget.parent.GetChildIndex(goTarget),
        };

        if(!nodes.ContainsKey(goTarget)) 
            nodes.Add(goTarget, info);
        else 
            nodes[goTarget] = info;

        Vector2 worldPos = LocalToGlobal(goTarget);
        Vector2 localPos = GlobalToLocal(toNode, worldPos);

        // 这里要加个延时！！
        goTarget.RemoveFromParent();
        toNode.AddChildAt(goTarget, funcToChildIndex != null ? funcToChildIndex(toNode) : toNode.numChildren);

        //goTarget.xy = localPos; // 适合父节点轴线在左上角(0,0)

        // 父节点fromeNode设置了轴心会影响到最终的位置（需要矫正位置！）
        goTarget.xy = new Vector2(localPos.x - info.fromeNode.pivotX * info.fromeNode.width, 
            localPos.y - info.fromeNode.pivotY * info.fromeNode.height); 

        //DebugUtils.Log($"{info.fromLocalPos.x},{info.fromLocalPos.y}  -- {worldPos.x},{worldPos.y} -- {localPos.x},{localPos.y}");
        
        // 延时设置索引！！
    }

    public void ReturnSortingOrder(GObject goTarget, bool isUseCurPos = false)
    {
        //if (!nodes.ContainsKey(goTarget)) return;

        //SortingOrderInfo info = nodes[goTarget];
        //nodes.Remove(goTarget);

        //Vector2 localPos = info.fromLocalPos;
        //if (isUseCurPos)
        //{
        //    Vector2 worldPos = LocalToGlobal(goTarget);
        //    localPos = GlobalToLocal(info.fromeNode, worldPos);
        //}
        //goTarget.RemoveFromParent();
        ////info.fromeNode.AddChildAt(goTarget, info.funcFromChildIndex != null ? info.funcFromChildIndex(info.fromeNode) : info.fromeNode.numChildren);
        //info.fromeNode.AddChildAt(goTarget, info.fromeNode.numChildren);
        //RequestSetIndex(goTarget,
        //    info.funcFromChildIndex != null ? info.funcFromChildIndex(info.fromeNode) : info.fromeNode.numChildren);

        ////goTarget.xy = localPos; // 适合父节点轴线在左上角(0,0)

        //// 矫正位置
        //if (info.toNode == null)
        //{
        //    DebugUtils.LogError("info.toNode is null");
        //}
        //else if (goTarget == null)
        //{
        //    DebugUtils.LogError("goTarget is null");
        //}
        //else 
        //    goTarget.xy = new Vector2(localPos.x - info.toNode.pivotX * info.toNode.width,
        //        localPos.y - info.toNode.pivotY * info.toNode.height);

        if (!nodes.ContainsKey(goTarget)) return;

        SortingOrderInfo info = nodes[goTarget];

        // ========== 详细日志：记录组件信息 ==========
        string targetName = goTarget != null ? goTarget.name : "NULL";
        string targetType = goTarget != null ? goTarget.GetType().Name : "NULL";
        bool targetDisposed = goTarget != null ? goTarget.isDisposed : true;
        bool displayObjectNull = goTarget != null && goTarget.displayObject == null;
        string fromNodeName = info.fromeNode != null ? info.fromeNode.name : "NULL";
        bool fromNodeDisposed = info.fromeNode != null ? info.fromeNode.isDisposed : true;
        string toNodeName = info.toNode != null ? info.toNode.name : "NULL";
        bool toNodeDisposed = info.toNode != null ? info.toNode.isDisposed : true;
        string markInfo = string.IsNullOrEmpty(info.mark) ? "(无标记)" : info.mark;

        // 检查是否有问题
        if (goTarget == null || goTarget.isDisposed || goTarget.displayObject == null ||
            info.fromeNode == null || info.fromeNode.isDisposed ||
            info.toNode == null || info.toNode.isDisposed)
        {
            DebugUtils.LogError($"[FguiSortingOrder] 组件已被清理，无法还原！\n" +
                $"  组件名称: {targetName}\n" +
                $"  组件类型: {targetType}\n" +
                $"  组件已销毁: {targetDisposed}\n" +
                $"  DisplayObject为null: {displayObjectNull}\n" +
                $"  原父节点: {fromNodeName} (已销毁: {fromNodeDisposed})\n" +
                $"  目标父节点: {toNodeName} (已销毁: {toNodeDisposed})\n" +
                $"  标记: {markInfo}\n" +
                $"  原始位置: ({info.fromLocalPos.x}, {info.fromLocalPos.y})\n" +
                $"  调用栈: {System.Environment.StackTrace}");

            // 从字典中移除，避免重复报错
            nodes.Remove(goTarget);
            return;
        }

        nodes.Remove(goTarget);

        Vector2 localPos = info.fromLocalPos;
        if (isUseCurPos)
        {
            Vector2 worldPos = LocalToGlobal(goTarget);
            localPos = GlobalToLocal(info.fromeNode, worldPos);
        }
        goTarget.RemoveFromParent();
        //info.fromeNode.AddChildAt(goTarget, info.funcFromChildIndex != null ? info.funcFromChildIndex(info.fromeNode) : info.fromeNode.numChildren);
        info.fromeNode.AddChildAt(goTarget, info.fromeNode.numChildren);
        RequestSetIndex(goTarget,
            info.funcFromChildIndex != null ? info.funcFromChildIndex(info.fromeNode) : info.fromeNode.numChildren);

        //goTarget.xy = localPos; // 适合父节点轴线在左上角(0,0)

        // 矫正位置
        if (info.toNode == null)
        {
            DebugUtils.LogError($"info.toNode is null, 组件: {targetName}, 标记: {markInfo}");
        }
        else if (goTarget == null)
        {
            DebugUtils.LogError($"goTarget is null, 标记: {markInfo}");
        }
        else
        {
            try
            {
                goTarget.xy = new Vector2(localPos.x - info.toNode.pivotX * info.toNode.width,
                    localPos.y - info.toNode.pivotY * info.toNode.height);
            }
            catch (System.NullReferenceException ex)
            {
                DebugUtils.LogError($"[FguiSortingOrder] 设置位置时发生NullReferenceException！\n" +
                    $"  组件名称: {targetName}\n" +
                    $"  组件类型: {targetType}\n" +
                    $"  组件已销毁: {goTarget.isDisposed}\n" +
                    $"  DisplayObject为null: {goTarget.displayObject == null}\n" +
                    $"  原父节点: {fromNodeName} (已销毁: {info.fromeNode.isDisposed})\n" +
                    $"  目标父节点: {toNodeName} (已销毁: {info.toNode.isDisposed})\n" +
                    $"  标记: {markInfo}\n" +
                    $"  异常信息: {ex.Message}\n" +
                    $"  调用栈: {ex.StackTrace}");
                throw;
            }
        }
    }

    public void ReturnAllSortingOrder(bool isUseCurPos = false)
    {
        //int i = nodes.Count;
        //while (--i >= 0)
        //{
        //    KeyValuePair<GObject, SortingOrderInfo> item = nodes.ElementAt(i);
        //    ReturnSortingOrder(item.Key, isUseCurPos);
        //}
        int totalCount = nodes.Count;
       // DebugUtils.Log($"[FguiSortingOrder] ReturnAllSortingOrder 开始，共 {totalCount} 个组件需要还原");

        int i = nodes.Count;
        int successCount = 0;
        int failedCount = 0;
        int j = 0;
        while (--i >= 0)
        {
            KeyValuePair<GObject, SortingOrderInfo> item = nodes.ElementAt(i);

            // 记录每个组件的信息
            string itemName = item.Key != null ? item.Key.name : "NULL";
            string itemMark = string.IsNullOrEmpty(item.Value.mark) ? "(无标记)" : item.Value.mark;

            try
            {
                ReturnSortingOrder(item.Key, isUseCurPos);
                successCount++;
            }
            catch (System.Exception ex)
            {
                failedCount++;
                DebugUtils.LogError($"[FguiSortingOrder] ReturnAllSortingOrder 处理组件失败: {itemName}, 标记: {itemMark}, 异常: {ex.Message}");
            }
        }

        //DebugUtils.Log($"[FguiSortingOrder] ReturnAllSortingOrder 完成，成功: {successCount}, 失败: {failedCount}, 总计: {totalCount}");
    }
    public void ReturnSortingOrder(string mark , bool isUseCurPos = false)
    {
        int i = nodes.Count;
        while (--i >= 0)
        {
            KeyValuePair<GObject, SortingOrderInfo> item = nodes.ElementAt(i);
            if (item.Value.mark == mark)
            {
                ReturnSortingOrder(item.Key, isUseCurPos);
            }
        }
    }
    public void Clear(string mark)
    {
        int i = nodes.Count;
        while (--i >=0)
        {
            KeyValuePair<GObject, SortingOrderInfo> item = nodes.ElementAt(i);
            if (item.Value.mark == mark)
            {
                nodes.Remove(item.Key);
            }
        }
    }
    //public void ClearAll() => nodes.Clear();

    public void ClearAll() {
        requestIndexTasks.Clear();
        nodes.Clear();
    }

    Vector2 LocalToGlobal(GObject go)
    {
        //go.parent.LocalToRoot
        Vector2 worldPos = go.parent.LocalToGlobal(go.xy);
        return worldPos;
    }

    Vector2 GlobalToLocal(GObject toParent, Vector2 worldPos )
    {
        Vector2 localPos = toParent.GlobalToLocal(worldPos);
        return localPos;
    }
  
}
