using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using GameMaker;

namespace ConsoleSlot01
{
    public class PageConsoleDrawLine : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleDrawLine";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();
        }


        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        public override void OnClose(EventData data = null)
        {
            Stage.inst.onTouchMove.Remove(OnTouchMove);
            Stage.inst.onTouchEnd.Remove(OnTouchEnd);
            ClearLine();
            base.OnClose(data);
        }

        GButton btnClose;

        GComponent comFG;

        public override void InitParam()
        {

            btnClose = this.contentPane.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() => { CloseSelf(null); });
            comFG = this.contentPane.GetChild("fg").asCom;

            ClearLine();
            Stage.inst.onTouchMove.Remove(OnTouchMove);
            Stage.inst.onTouchEnd.Remove(OnTouchEnd);
            Stage.inst.onTouchMove.Add(OnTouchMove);
            Stage.inst.onTouchEnd.Add(OnTouchEnd);
        }


        private bool isDrawLineing = false;

        List<Vector2> dots = new List<Vector2>();

        void DrawLine(object data)
        {

            // 设置颜色
            List<Color32[]> defalutColors = GetColor32LSt();

            object[] targetDots = GameCommon.FguiUtils.DrawLine(dots, 10);


            for (int i = 0; i < comFG.numChildren; i++)
            {
                GGraph hodler = comFG.GetChildAt(i).asGraph;
                hodler.shape.parent.RemoveChild(hodler.shape); // 从父容器移除
                hodler.shape.Dispose(); // 显式释放资源
            }
            comFG.RemoveChildren();

            List<Vector2> endpointsNormaUnitUpPoints = targetDots[0] as List<Vector2>;
            List<Vector2> endpointsNormaUnitDownPoints = targetDots[1] as List<Vector2>;
            for (int i = 0; i < endpointsNormaUnitUpPoints.Count - 1; i++)
            {
                List<Vector2> lst = new List<Vector2>();
                lst.Add(endpointsNormaUnitUpPoints[i]);
                lst.Add(endpointsNormaUnitUpPoints[i + 1]);
                lst.Add(endpointsNormaUnitDownPoints[i + 1]);
                lst.Add(endpointsNormaUnitDownPoints[i]);

                GGraph holder = new GGraph();
                holder.shape.DrawPolygon(lst, defalutColors[i]);
                comFG.AddChild(holder);

            }

            isDrawLineing = false;
        }


        List<Color32[]> GetColor32LSt()
        {
            int dif = 20;

            int R = 255;
            int G = 0;
            int B = 0;
            bool isFirst = true;
            bool isSecond = false;

            List<Color32[]> colsRes = new List<Color32[]>();
            for (int i = 0; i < dots.Count; i++)
            {
                List<Color32> cols = new List<Color32>();

                while (true)
                {
                    if (isFirst)
                    {
                        if (G + dif <= 255)
                        {
                            R = 255;
                            B = 0;
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G + dif, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G + dif, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            G += dif;
                            break;
                        }
                        else if (R - dif >= 0)
                        {
                            G = 255;
                            B = 0;
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R - dif, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R - dif, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            R -= dif;
                            break;
                        }
                        else if (B + dif <= 255)
                        {
                            G = 255;
                            R = 0;
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B + dif));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B + dif));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            B += dif;
                            break;
                        }
                        else
                        {
                            isSecond = true;
                            isFirst = false;
                        }
                    }

                    if (isSecond)
                    {
                        if (G - dif >= 0)
                        {
                            R = 0;
                            B = 255;
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G - dif, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G - dif, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            G -= dif;
                            break;
                        }
                        else if (R + dif <= 255)
                        {
                            G = 0;
                            B = 255;
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R + dif, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R + dif, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            R += dif;
                            break;
                        }
                        else if (B - dif >= 0)
                        {
                            G = 0;
                            R = 255;
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B - dif));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B - dif));
                            cols.Add(GameCommon.FguiUtils.GetColor32(R, G, B));
                            B -= dif;
                            break;
                        }
                        else
                        {
                            isSecond = false;
                            isFirst = true;
                        }
                    }

                }

                colsRes.Add(cols.ToArray());
            }

            return colsRes;
        }


        private void OnTouchMove(EventContext context)
        {
            InputEvent inputEvent = (InputEvent)context.data;
            Vector2 pos = inputEvent.position;

            if (dots.Count == 0)
            {
                dots.Add(pos);
            }
            else
            {
                Vector2 lastPos = dots[dots.Count - 1];
                if (Mathf.Abs(pos.x - lastPos.x) > 5 && Mathf.Abs(pos.y - lastPos.y) > 5)
                {
                    dots.Add(pos);
                    if (isDrawLineing == false)
                    {
                        isDrawLineing = true;
                        Timers.inst.Add(0.1f, 1, DrawLine);
                    }
                }
            }
            //DebugUtils.Log($"触摸移动 - 坐标: {pos}");
        }

        private void OnTouchEnd(EventContext context)
        {
            InputEvent inputEvent = (InputEvent)context.data;
            Vector2 pos = inputEvent.position;


            ClearLine();
        }

        void ClearLine()
        {
            isDrawLineing = false;
            Timers.inst.Remove(DrawLine);
            dots = new List<Vector2>();
            for (int i = 0; i < comFG.numChildren; i++)
            {
                GGraph hodler = comFG.GetChildAt(i).asGraph;
                hodler.shape.parent.RemoveChild(hodler.shape); // 从父容器移除
                hodler.shape.Dispose(); // 显式释放资源
            }
            comFG.RemoveChildren();
        }
    }
}