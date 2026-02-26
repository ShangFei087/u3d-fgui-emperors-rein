using FairyGUI;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System;
using GameMaker;
using System.Text;

namespace ConsoleCoinPusher01
{
    public class PageConsoleErrorRecord : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleErrorRecord";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();

            int count = 1;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };
            /*
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
            (GameObject clone) =>
            {
                callback();
            });
            */
            callback();



            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        OnClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        OnClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                        OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        OnClickNext();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        OnClickPrev();
                    },
                    [MachineButtonKey.BtnConsole] = (info) =>
                    {
                        CloseSelf(null);
                    }
                }
            };

        }


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("警告记录");
            base.OnOpen(name, data);
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }

        



        //GList glst;

        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu, goRecord, detailed, goBack;
        GRichTextField pageText, detailedInfo;
        GTextField txtDate;

        bool isRecord;
        Dictionary<int, string> menuMap;




        int curIndexMenuItem = 0;
        /// <summary>
        /// 记录的索引
        /// </summary>
        int curIndexRecordItem = 0;


        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());




            goMenu = this.contentPane.GetChild("items").asCom;
            goRecord = this.contentPane.GetChild("record").asCom;
            pageText = this.contentPane.GetChild("page").asRichTextField;
            pageText.text = "0/0";

            detailed = this.contentPane.GetChild("detailed").asCom;
            goBack = goRecord.GetChildAt(goRecord.numChildren - 1).asCom;
            GRichTextField grtBack = goBack.GetChild("value1").asRichTextField;
            grtBack.text = "返回";
            grtBack.onClick.Clear();
            grtBack.onClick.Add(() =>
            {
                if (!isRecord) return;
                BackToMenu();
            });





            txtDate = goMenu.GetChild("date").asCom.GetChild("value").asTextField;
            txtDate.text = "";

            detailedInfo = detailed.GetChild("info").asRichTextField;

            ClearPage();

            //ReadData();
            AddClickEvent();
            //#seaweed# RecordClearArrow(true);
            BackToMenu(); //#seaweed# 
            ClearArrow(false);



            InitLogInfo();

        }


        const string FORMAT_DATE_DAY = "yyyy-MM-dd";
        const int PER_PAGE_NUM = 11;
        int curPageIndex = 0;
        int totalPageCount = 0;
        int fromIdx = 0;
        int dateIndex = 0;
        List<string> dropdownDateLst;
        List<TableLogRecordItem> resLogEventRecord;


        void ClearPage()
        {
            for (int i = 0; i < goRecord.numChildren; i++)
            {
                GComponent item = goRecord.GetChildAt(i).asCom;
                item.visible = false;
            }
        }

        void InitLogInfo()
        {

            string sql = $"SELECT created_at FROM {ConsoleTableName.TABLE_LOG_ERROR_RECORD}";

            DebugUtils.Log(sql);
            List<long> date = new List<long>();
            dropdownDateLst = new List<string>();
            SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (SqliteDataReader sdr) =>
            {
                while (sdr.Read())
                {
                    long d = sdr.GetInt64(0);
                    date.Add(d);
                }
                foreach (long timestamp in date)
                {
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                    DateTime localDateTime = dateTimeOffset.LocalDateTime;
                    string time = localDateTime.ToString(FORMAT_DATE_DAY);

                    if (!dropdownDateLst.Contains(time))
                    {
                        dropdownDateLst.Insert(0, time);
                        //DebugUtils.Log($"时间搓：{timestamp} 时间 ：{time}");
                    }
                }


                if (dropdownDateLst.Count > 0)
                    SetDate(0);
            });
        }

        void OnDropdownChangedDate(int indexDate)
        {

            string sql2 = $"SELECT * FROM {ConsoleTableName.TABLE_LOG_ERROR_RECORD} WHERE DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{dropdownDateLst[indexDate]}'"; //可以用

            SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql2, null, (SqliteDataReader sdr) =>
            {
                resLogEventRecord = new List<TableLogRecordItem>();
                while (sdr.Read())
                {
                    resLogEventRecord.Insert(0,
                    new TableLogRecordItem()
                    {
                        log_type = sdr.GetString(sdr.GetOrdinal("log_type")),
                        log_content = sdr.GetString(sdr.GetOrdinal("log_content")),
                        log_stacktrace = sdr.GetString(sdr.GetOrdinal("log_stacktrace")),
                        log_tag = sdr.GetString(sdr.GetOrdinal("log_tag")),
                        created_at = sdr.GetInt64(sdr.GetOrdinal("created_at")),
                    });
                }

                curPageIndex = 0;
                totalPageCount = (resLogEventRecord.Count + (PER_PAGE_NUM - 1)) / PER_PAGE_NUM; //向上取整
                fromIdx = 0;
                SetUIEvent();
            });
        }


        void SetUIEvent()
        {
            int lastIdx = fromIdx + PER_PAGE_NUM - 1;
            if (lastIdx > resLogEventRecord.Count - 1)
            {
                lastIdx = resLogEventRecord.Count - 1;
            }




            goBack.parent.SetChildIndex(goBack, goBack.parent.numChildren);

            for (int i = 0; i < goRecord.numChildren; i++)
            {
                GComponent item = goRecord.GetChildAt(i).asCom;
                if (item != goBack)
                {
                    item.touchable = false;
                    item.visible = false;
                }
            }


            int endIndex = lastIdx - fromIdx;
            for (int i = 0; i <= endIndex; i++)
            {
                GComponent item = goRecord.GetChildAt(i).asCom;
                item.touchable = true;
                item.visible = true;

                int indexItem = i;
                int indexRecord = i + fromIdx;
                TableLogRecordItem res = resLogEventRecord[indexRecord];

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(res.created_at);
                DateTime localDateTime = dateTimeOffset.LocalDateTime;

                GRichTextField grtxtTime = item.GetChild("value1").asRichTextField;
                grtxtTime.text = localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                grtxtTime.onClick.Clear();
                grtxtTime.onClick.Add(() =>
                {
                    SetAllowToRecordItem(indexItem);
                });


                //item.Find("Tag/Text").GetComponent<TextMeshProUGUI>().text = $"--{res.log_tag}";
                string content = Encoding.UTF8.GetString(Convert.FromBase64String(res.log_content));
                string detail = GetDetail(indexRecord);

                GRichTextField grtxtContent = item.GetChild("value2").asRichTextField;
                grtxtContent.text = content;
                grtxtContent.data = $"{indexRecord}";
                grtxtContent.onClick.Clear();
                grtxtContent.onClick.Add(() =>
                {
                    if (!isRecord)
                        return;

                    SetAllowToRecordItem(indexItem);

                    PageManager.Instance.OpenPage(PageName.ConsolePusher01PopupConsoleRecord,
                      new EventData<Dictionary<string, object>>("",
                          new Dictionary<string, object>()
                          {
                              ["value"] = detail,
                          }
                      ));

                });
            }

            // 返回按钮放到最后
            goBack.parent.SetChildIndex(goBack, endIndex + 1);
            if (isRecord)
            {
                //goBack.touchable = true;
                goBack.visible = true;
            }
            else
            {
                //goBack.touchable = false;
                goBack.visible = false;
            }

            pageText.text = (curPageIndex + 1) + "/" + totalPageCount;

        }
        string GetDetail(int index)
        {
            TableLogRecordItem res = resLogEventRecord[index];
            string content = Encoding.UTF8.GetString(Convert.FromBase64String(res.log_content));

            return $"[content]:\n\n{content}\n\n[stacktrace]:\n\n" +
                     Encoding.UTF8.GetString(Convert.FromBase64String(res.log_stacktrace));
        }



        void ClearArrow(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            curIndexMenuItem = 0;
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                GComponent goItem = goMenu.GetChildAt(i).asCom;
                if (isClearAllArrow)
                    goItem.GetChild("icon").visible = false;
                else
                    goItem.GetChild("icon").visible = i == curIndexMenuItem;

                menuMap.Add(i, goItem.name);
            }

        }


        void RecordClearArrow(bool isClearAllArrow)
        {
            curIndexRecordItem = 0;
            for (int i = 0; i < goRecord.numChildren; i++)
            {
                GComponent goItem = goRecord.GetChildAt(i).asCom;
                if (isClearAllArrow)
                    goItem.GetChild("icon").visible = false;
                else
                    goItem.GetChild("icon").visible = i == curIndexRecordItem;

            }
        }


        void SetAllow()
        {
            if (isRecord)
            {
                for (int i = 0; i < goRecord.numChildren; i++)
                {
                    goRecord.GetChildAt(i).asCom.GetChild("icon").visible = i == curIndexRecordItem;
                }
            }
            else
            {
                for (int i = 0; i < goMenu.numChildren; i++)
                {
                    goMenu.GetChildAt(i).asCom.GetChild("icon").visible = i == curIndexMenuItem;
                }
            }

        }
        void OnClickNext()
        {
            if (isRecord)
            {
                if (++curIndexRecordItem >= goRecord.numChildren)
                    curIndexRecordItem = 0;

                if (goRecord.GetChildAt(curIndexRecordItem).visible == false)
                    curIndexRecordItem = 0;

                ShowDetailedRecord();
            }
            else
            {
                if (++curIndexMenuItem >= goMenu.numChildren)
                    curIndexMenuItem = 0;
            }
            SetAllow();
        }

        void OnClickPrev()
        {

            if (isRecord)
            {

                int endlessLoop = 12;
                do
                {
                    if (--curIndexRecordItem < 0)
                        curIndexRecordItem = goRecord.numChildren - 1;
                } while (goRecord.GetChildAt(curIndexRecordItem).visible == false && --endlessLoop >= 0);


                ShowDetailedRecord();
            }
            else
            {
                if (--curIndexMenuItem < 0)
                    curIndexMenuItem = goMenu.numChildren - 1;
            }
            SetAllow();
        }



        void BackToMenu()
        {
            RecordClearArrow(true);
            isRecord = false;
            goMenu.grayed = false;
            goMenu.touchable = true;
            goBack.visible = false;
            detailed.visible = false;
        }

        void OnClickConfirm()
        {
            if (isRecord)
            {
                if (curIndexRecordItem == goRecord.GetChildIndex(goBack))
                {
                    BackToMenu();
                    return;
                }

                int index = int.Parse((string)goRecord.GetChildAt(curIndexRecordItem).asCom.GetChild("value2").data);
                PageManager.Instance.OpenPage(PageName.ConsolePusher01PopupConsoleRecord,
                  new EventData<Dictionary<string, object>>("",
                      new Dictionary<string, object>()
                      {
                          ["value"] = GetDetail(index),
                      }
                  ));
                return;
            }


            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem])
                {
                    case "date":
                        {
                            ChangeDate();
                        }
                        return;
                    case "prev":
                        {
                            PervPage();
                        }
                        return;
                    case "next":
                        {
                            NextPage();
                        }
                        return;
                    case "check":
                        {
                            DetailedRecord();
                        }
                        return;
                    case "exit":
                        {
                            CloseSelf(null);
                        }
                        return;
                }
            }
        }

        void ChangeDate()
        {
            if (dropdownDateLst.Count <= 0) return;

            if (++dateIndex >= dropdownDateLst.Count)
                dateIndex = 0;

            SetDate(dateIndex);
        }

        void SetDate(int index)
        {
            dateIndex = index;
            OnDropdownChangedDate(index);
            txtDate.text = dropdownDateLst[index].ToString();
        }
        void AddClickEvent()
        {
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                int index = i;
                goMenu.GetChildAt(index).onClick.Clear();
                goMenu.GetChildAt(index).onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }

        /*
        void AddClickEventRecord()
        {
            for (int i = 0; i < goRecord.numChildren; i++)
            {
                int index = i;
                goRecord.GetChildAt(index).onClick.Clear();
                goRecord.GetChildAt(index).onClick.Add(() =>
                {
                    curIndexMenuItemRecord = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }*/



        /// <summary>
        /// 进入查看详细记录
        /// </summary>
        void DetailedRecord()
        {
            detailedInfo.text = "";

            if (dropdownDateLst.Count == 0)
                return;

            detailed.visible = true;
            isRecord = true;
            goBack.visible = true;
            goMenu.grayed = true;
            goMenu.touchable = false;
            RecordClearArrow(false);

            ShowDetailedRecord(); // 同步详情记录内容
   
        }


        /// <summary>  显示详细数据 </summary>
        void ShowDetailedRecord()
        {
            if (curIndexRecordItem == goRecord.GetChildIndex(goBack))
                return;
            
            int index = int.Parse((string)goRecord.GetChildAt(curIndexRecordItem).asCom.GetChild("value2").data);
            detailedInfo.text = GetDetail(index);
        }

        void SetAllowToRecordItem(int indexRecordItem)
        {
            if (!isRecord)
                return;

            curIndexRecordItem = indexRecordItem;
            ShowDetailedRecord();
            SetAllow();
        }


        void NextPage()
        {
            if (dropdownDateLst.Count == 0)
                return;

            if (fromIdx + PER_PAGE_NUM >= resLogEventRecord.Count)
                return;
            fromIdx += PER_PAGE_NUM;
            curPageIndex++;
            SetUIEvent();
        }



        void PervPage()
        {
            if (dropdownDateLst.Count == 0)
                return;

            if (fromIdx <= 0)
                return;
            fromIdx -= PER_PAGE_NUM;
            curPageIndex--;
            if (fromIdx < 0)
            {
                curPageIndex = 0;
                fromIdx = 0;
            }
            SetUIEvent();
        }



    }



}