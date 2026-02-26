using FairyGUI;
using GameMaker;
using GameUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleSlot01
{
    public class TipItemInfo
    {
        public string msg;
        public float timeS;
        public GComponent go;
    }
    public class PopupConsoleTip : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleTip";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();
        }


        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
            ShowTip(data);
        }

        GComponent cmpContent;
        GComponent item;

        float gap01, height01;


        List<TipItemInfo> tipItems = new List<TipItemInfo>();
        List<GComponent> scaleItems = new List<GComponent>();
        LoopTimer tCheckItemRemove = null, tScaleUpdate = null;

        float targetY = 0;

        bool isDirty = false;
        public override void InitParam()
        {
            cmpContent = this.contentPane.GetChild("content").asCom;
            item = cmpContent.GetChildAt(0).asCom;

            gap01 = 2;
            height01 = item.height;

            if (tScaleUpdate == null) tScaleUpdate = Timer.LoopAction(0.03f, (data) => { ScaleUpdate(); });
        }


        public void ShowTip(EventData data)
        {
            string msg = (string)data.value;
            AddItem(msg);
        }

        public bool Contains(string msg)
        {
            for (int i = 0; i < cmpContent.numChildren; i++)
            {
                GComponent itm = cmpContent.GetChildAt(i).asCom;
                if (itm.visible
                    && itm.GetChild("title").asRichTextField.text == msg
                    )
                    return true;
            }
            return false;
        }

        public void UpdatePos()
        {
            if (isDirty)
            {
                List<GComponent> items = new List<GComponent>();
                for (int i = cmpContent.numChildren - 1; i >= 0; i--)
                {
                    GComponent tfm = cmpContent.GetChildAt(i).asCom;
                    if (tfm.visible)
                        items.Add(tfm);
                }

                if (items.Count > 0)
                {
                    int i = 0;
                    while (++i < items.Count)
                    {
                        float targetY = (gap01 + height01) * i;

                        targetY = cmpContent.height / 2 - targetY;
                        items[i].SetXY(cmpContent.width / 2, targetY);
                    }
                }

                isDirty = false;
            }
        }

        public void AddItem(string msg)
        {

            GComponent tfmTarget = null;
            for (int i = 0; i < cmpContent.numChildren; i++)
            {
                GComponent tfm = cmpContent.GetChildAt(i).asCom;
                if (!tfm.visible)
                    tfmTarget = tfm;
            }
            if (tfmTarget == null)
            {
                tfmTarget = UIPackage.CreateObject("Console", "ItemPopupTip").asCom;
            }

            cmpContent.AddChildAt(tfmTarget, cmpContent.numChildren);
            tfmTarget.SetPivot(0.5f, 0.5f, true);
            tfmTarget.visible = true;
            tfmTarget.SetXY(cmpContent.width / 2, cmpContent.height / 2);
            tfmTarget.SetScale(0.2f, 0.2f);

            scaleItems.Add(tfmTarget);

            try
            {
                tfmTarget.GetChild("title").asRichTextField.text = msg;
            }
            catch
            {
                DebugUtils.LogError($"name: {tfmTarget.name}");
            }

            tipItems.Add(new TipItemInfo()
            {
                msg = msg,
                go = tfmTarget,
                timeS = UnityEngine.Time.unscaledTime    //DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), // UnityEngine.Time.time,
            });

            isDirty = true;
            UpdatePos();

            if (tCheckItemRemove == null)
                tCheckItemRemove = Timer.LoopAction(0.5f, (res) => { CheckItemRemove02(); });
        }


        void CheckItemRemove02()
        {
            if (tipItems.Count > 0)
            {
                while (tipItems.Count > 8)// 限制最多的个数
                {
                    TipItemInfo target = tipItems[0];
                    target.go.visible = false;
                    tipItems.Remove(target);
                }

                float nowTimeS = UnityEngine.Time.unscaledTime;

                TipItemInfo targetRemove = tipItems[0];
                if (targetRemove != null && nowTimeS - targetRemove.timeS > 2f)
                {
                    targetRemove.go.visible = false;
                    tipItems.Remove(targetRemove); //出现2秒后开始删除
                }

                if (tipItems.Count > 0)
                    return;
            }

            tCheckItemRemove.Cancel();
            tCheckItemRemove = null;
            PageManager.Instance.ClosePage(this);

        }

        public override void OnClose(EventData data = null)
        {
            base.OnClose(data);
            if (tCheckItemRemove != null)
            {
                tCheckItemRemove.Cancel();
                tCheckItemRemove = null;
            }
            if (tScaleUpdate != null)
            {
                tScaleUpdate.Cancel();
                tScaleUpdate = null;
            }
            for (int i = 0; i < cmpContent.numChildren; i++)
            {
                GComponent tfm = cmpContent.GetChildAt(i).asCom;
                tfm.visible = false;
            }
        }



        bool isDirtyScaleUpdate = true;
        void ScaleUpdate()
        {

            if (isDirtyScaleUpdate)
            {
                isDirtyScaleUpdate = false;

                int i = scaleItems.Count;
                while (--i >= 0)
                {
                    //DebugUtils.Log($"i am ScaleUpdate {scaleItems[i].scaleX} ");
                    float scale = scaleItems[i].scaleX + 30f * Time.unscaledDeltaTime;
                    if (scale >= 1)
                    {
                        scaleItems[i].scale = Vector2.one;
                        scaleItems.Remove(scaleItems[i]);
                    }
                    else
                    {
                        scaleItems[i].scale = new Vector2(scale, scale);
                    }
                }

                isDirtyScaleUpdate = true;
            }

        }

    }
}