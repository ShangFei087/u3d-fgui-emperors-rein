using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameCommon
{
    public static class FguiUtils
    {

        #region "Spine、例子特效、3D模式" 装载器


        /// <summary>
        /// 添加Spine、例子特效、3D模式
        /// </summary>
        /// <param name="anchorRoot"></param>
        /// <param name="go"></param>
        public static void AddWrapper(GComponent anchorRoot, GameObject go) 
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            GGraph holder = anchorRoot.GetChild("holder").asGraph;  
            GoWrapper wrapper = new GoWrapper(go);
            holder.SetNativeObject(wrapper); 

            GLoader goDesign = anchorRoot.GetChild("example").asLoader;  

            holder.SetPivot(0.5f, 0.5f, true);
            holder.size = new Vector2(goDesign.size.x, goDesign.size.y);
            holder.xy = Vector2.zero;
            holder.visible = true;

            holder.scale = new Vector2(goDesign.scale.x, goDesign.scale.y);

        }


        /// <summary>
        /// 改变"Spine/粒子/模型"对象的父节点，不销毁原物体
        /// </summary>
        /// <param name="oldAnchorRoot">原父节点容器</param>
        /// <param name="newAnchorRoot">新父节点容器</param>
        /// <param name="cloneMaterial">是否克隆材质</param>
        /// <returns>是否成功改变父节点</returns>
        public static bool ChangeWrapperParent(GComponent oldAnchorRoot, GComponent newAnchorRoot, bool cloneMaterial = true)
        {
            // 验证输入参数
            if (oldAnchorRoot == null || newAnchorRoot == null || oldAnchorRoot == newAnchorRoot)
                return false;

            try
            {
                // 获取原容器中的包装器和目标对象
                GGraph oldHolder = oldAnchorRoot.GetChild("holder").asGraph;
                GoWrapper oldWrapper = oldHolder.displayObject as GoWrapper;
                if (oldWrapper == null || oldWrapper.wrapTarget == null)
                    return false;

                GameObject targetObj = oldWrapper.wrapTarget;

                // 从原容器中移除包装器
                oldHolder.SetNativeObject(null);

                // 将对象添加到新容器
                AddWrapper(newAnchorRoot, targetObj);

                // 更新新容器中的包装器状态
                GoWrapper newWrapper = GetWrapper(newAnchorRoot);
                if (newWrapper != null)
                {
                    newWrapper.SetWrapTarget(targetObj, cloneMaterial);
                    RefreshWrapper(newAnchorRoot);
                }

                return true;
            }
            catch (Exception ex)
            {
                // 可以在这里添加日志记录
                // DebugUtils.LogError($"改变父节点失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除“Spine/粒子/模型”的“GameObject”对象
        /// </summary>
        /// <param name="anchorRoot"></param>
        /// <returns></returns>
        public static void DeleteWrapperTarget(GComponent anchorRoot)
        {
            try
            {
                GGraph holder = anchorRoot.GetChild("holder").asGraph;                 
                GoWrapper wrapper = holder.displayObject as GoWrapper;
                GameObject.Destroy(wrapper.wrapTarget);
            }
            catch (Exception ex)
            {

            }
        }

        
        public static void DeleteWrapper(GComponent anchorRoot)
        {
            try
            {
                if (anchorRoot == null)
                    return;  // 要报错？

                GGraph holder = anchorRoot.GetChild("holder").asGraph;                
                GoWrapper wrapper = holder.displayObject as GoWrapper;
                wrapper.Dispose();  // 会同时删除 WrapperTarget
            }
            catch (Exception ex)
            {

            }
        }
        
     
        /// <summary>
        /// 获取“Spine/粒子/模型”的“GoWrapper”包装器
        /// </summary>
        /// <param name="anchorRoot"></param>
        /// <returns></returns>
        public static GoWrapper GetWrapper(GComponent anchorRoot)
        {
            try
            {
                GGraph holder = anchorRoot.GetChild("holder").asGraph;                  
                GoWrapper wrapper = holder.displayObject as GoWrapper;
                return wrapper;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 获取“Spine/粒子/模型”的“GameObject”对象
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static GameObject GetWrapperTarget(GComponent anchorRoot)
        {
            try
            {
                GGraph holder = anchorRoot.GetChild("holder").asGraph;                  
                // GoWrapper : DisplayObject
                GoWrapper wrapper = holder.displayObject as GoWrapper;
                return wrapper.wrapTarget;
            }
            catch (Exception ex)
            {

            }
            return null;
        }



        /// <summary>
        /// 更换“Spine/粒子/模型”对象
        /// </summary>
        /// <param name="anchor"></param>
        /// <param name="goTarget"></param>
        /// <param name="cloneMaterial"></param>
        public static void  ChangeWrapperTarget(GComponent anchorRoot, GameObject goTarget, bool cloneMaterial = true)
        {
            try
            {
                GGraph holder = anchorRoot.GetChild("holder").asGraph;                  
                GoWrapper wrapper = holder.displayObject as GoWrapper;
                wrapper.SetWrapTarget(goTarget, cloneMaterial);
            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 刷新“Spine/粒子/模型”展示的状态（当“Spine/粒子/模型”发生变化时）
        /// </summary>
        /// <param name="anchorRoot"></param>
        public static void RefreshWrapper(GComponent anchorRoot)
        {
            try
            {
                GGraph holder = anchorRoot.GetChild("holder").asGraph;                  
                GoWrapper wrapper = holder.displayObject as GoWrapper;
                wrapper.CacheRenderers();
            }
            catch (Exception ex)
            {

            }
        }
        
        #endregion


        

        #region 坐标转化
        
        static public Vector2 GetToNodeLocalPos(Vector2 formLocalPos ,GComponent fromNode, GComponent toNode)
        {
            Vector2 worldPos = fromNode.LocalToGlobal(formLocalPos);
        
            Vector2  localPos = toNode.GlobalToLocal(worldPos);
        
            return new Vector2(localPos.x - fromNode.pivotX * fromNode.width,
                localPos.y - fromNode.pivotY * fromNode.height);
        }
        
        #endregion



#region 画线
        static public object[] DrawLine(List<Vector2> points, float lineSize=1)
        {
            // 每个线段的法线向量的斜率
            List<float> lineNormalSlope = new List<float>();
            // 每个线段头尾端点的向上法向量
            List<Vector2> lineNormaUnitUpPoints = new List<Vector2>();
            // 每个线段头尾端点的向下法向量
            List<Vector2> lineNormaUnitDownPoints = new List<Vector2>();    
            
            // 获取每个线段左右两个端点的法向量
            for (int i = 1; i < points.Count; i++)
            {
                Vector2  prevPoint = points[i - 1];
                Vector2 currentPoint = points[i];
                // 计算线段斜率
                float dx = currentPoint.x - prevPoint.x;
                float dy = currentPoint.y - prevPoint.y;
                float segmentSlope = dy / dx;
                // 计算法线斜率（法线斜率是线段斜率的负倒数）
                float normalSlope = -1 / segmentSlope;
                lineNormalSlope.Add(normalSlope);
                // 计算法线单位向量
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                float unitDx = dx / length;
                float unitDy = dy / length;
                // 法线方向向量（旋转90度）
                float normalDx = -unitDy;
                float normalDy = unitDx;    
                
               
                Vector2 downPoint1 = new Vector2(
                    prevPoint.x + normalDx * lineSize,
                    prevPoint.y + normalDy * lineSize);
                lineNormaUnitDownPoints.Add(downPoint1);       
                
                Vector2 downPoint2 = new Vector2(
                    currentPoint.x + normalDx * lineSize,
                    currentPoint.y + normalDy * lineSize);
                lineNormaUnitDownPoints.Add(downPoint2);
                
                
                Vector2 upPoint1 = new Vector2(
                    prevPoint.x - normalDx * lineSize,
                    prevPoint.y - normalDy * lineSize);
                lineNormaUnitUpPoints.Add(upPoint1);
                
                // 在当前点上，沿法线方向找到距离为1的两个点
                Vector2 upPoint2 = new Vector2(
                    currentPoint.x - normalDx * lineSize,
                    currentPoint.y - normalDy * lineSize);
                lineNormaUnitUpPoints.Add(upPoint2);
                
            }
            
            
            // 获取每个端点的法向量
            List<Vector2> endpointsNormaUnitUpPoints = new List<Vector2>();
            List<Vector2> endpointsNormaUnitDownPoints = new List<Vector2>();       
            
            endpointsNormaUnitUpPoints.Add(lineNormaUnitUpPoints[0]);
            for(int i=1; i<points.Count-1;i++)
            {
                int j = (i - 1) * 2 + 1;
                Vector2 D = GetPointD(lineNormaUnitUpPoints[j], points[i], lineNormaUnitUpPoints[j + 1]);
                endpointsNormaUnitUpPoints.Add(D);
            }
            endpointsNormaUnitUpPoints.Add(lineNormaUnitUpPoints[lineNormaUnitUpPoints.Count-1]); 
            
            
            endpointsNormaUnitDownPoints.Add(lineNormaUnitDownPoints[0]); 
            for(int i=1; i<points.Count-1;i++)
            {
                int j = (i - 1) * 2 + 1;
                Vector2 D = GetPointD(lineNormaUnitDownPoints[j], points[i], lineNormaUnitDownPoints[j + 1]);
                endpointsNormaUnitDownPoints.Add(D);
            }
            endpointsNormaUnitDownPoints.Add(lineNormaUnitDownPoints[lineNormaUnitDownPoints.Count-1]);
            
            return new object[]{ endpointsNormaUnitUpPoints,endpointsNormaUnitDownPoints};
        }

        static Vector2 GetPointD( Vector2 A, Vector2 B, Vector2 C)
        {
            // BD = BA + BC
            // D = A + BC
            Vector2 BC = C - B;
            Vector2 D = A + BC;
            return D;
        }
        
        
        public static Color HexToColor(string hex)
        {
            // 去除 #（如果有）
            hex = hex.Replace("#", "");
        
            // 解析 RGB 分量
            float r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        
            // 返回 Color 对象（Alpha 默认为 1）
            return new Color(r, g, b);
        }

        static public Color GetColor(int R,int G,int B)
        {
            return  new Color(R / 255f, G / 255f, B / 255f);
        }
        static public Color32 GetColor32(int R,int G,int B)
        {
            return  (Color32)(new Color(R / 255f, G / 255f, B / 255f));
        }
        static public Color32 GetColor32(int R,int G,int B,int A)
        {
            return  new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
 
        
        #endregion
    }
}