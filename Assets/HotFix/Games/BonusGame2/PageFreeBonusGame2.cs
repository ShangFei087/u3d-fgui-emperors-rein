using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageFreeBonusGame2 : PageBase
{
    public const string pkgName = "BonusGame2";
    public const string resName = "PageFreeBonusGame2";
    // 在类中定义静态Random实例（全局唯一）
    private static readonly System.Random _random = new System.Random();
    int Count = 3;
    GTextField Spins;
    int score1, score2;
    List<GObject> table1, table2, table3 = new List<GObject> { };
    GObject targetObj2,targetObj1, targetObj3;
    float bombProbability = 0.25f;
    float goldProbability = 0.25f;
     float randomizeProbability = 0.7f; // 生成随机数的概率（0-1之间）
    bool isInTable1, isInTable2;
    protected override void OnInit()
    {
        
        base.OnInit();
    }


    public override void OnOpen(PageName name, EventData data)
    {
        base.OnOpen(name, data);
        InitParam();
    }

    // public override void OnTop() { DebugUtils..Log($"i am top {this.name}"); }

    public override void InitParam()
    {

        Spins = this.contentPane.GetChild("table3").asCom.GetChild("Spins").asTextField;
        table1 = GetChildrenOfType<GRichTextField>(this.contentPane.GetChild("table1").asCom.GetChild("slotMachine").asCom);
        table2 = GetChildrenOfType<GRichTextField>(this.contentPane.GetChild("table2").asCom.GetChild("slotMachine").asCom);
        table3 = GetChildrenOfType<GRichTextField>(this.contentPane.GetChild("table3").asCom.GetChild("slotMachine").asCom);
        targetObj2 = this.contentPane.GetChild("table2").asCom.GetChild("special");
        targetObj1 = this.contentPane.GetChild("table1").asCom.GetChild("special");

        ChangTextToNull();
        ExtractTable3_5x5();       // 提取5x5表格（每12个中取索引1-5）


        Timers.inst.Add(2, 1, (object obj) => tableBomb(2));

        Timers.inst.Add(3, 1, (object obj) => tableBomb(1));


        Timers.inst.Add(5, 1, (object obj) => tableGame(1));
        Timers.inst.Add(5, 1, (object obj) => tableGame(2));
        Timers.inst.Add(5, 1, (object obj) => tableGame(3));
        Timers.inst.Add(6, 1, StartTable3WinningProcess);


    }

    /// <summary>
    /// 将所有表类的text置“”
    /// </summary>
    void ChangTextToNull()
    {
        foreach (var item in table1)
        {
            item.text = "";
        }

        foreach (var item in table2)
        {
            item.text = "";
        }
        foreach (var item in table3)
        {
            item.text = "";
        }

    }


    /// <summary>
    /// fgui中寻找指定类型并返回列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    List<GObject> GetChildrenOfType<T>(GComponent parent) where T : GObject
    {
        List<GObject> result = new List<GObject>();
        foreach (GObject child in parent.GetChildren())
        {
            // 检查当前子对象是否为目标类型
            if (child is T)
            {
                result.Add(child);
            }
            // 如果子对象是容器，递归查找其内部
            if (child is GComponent)
            {
                result.AddRange(GetChildrenOfType<T>((GComponent)child));
            }
        }
        return result;
    }


    private List<GObject> GetTableFieldsByNumber(int n)
    {
        switch (n)
        {
            case 1:
                return table1;
            case 2:
                return table2;
            case 3:
                return table3;
            default:
                DebugUtils.LogError($"不支持的表编号: {n}");
                return null;
        }
    }


    /// <summary>
    /// 生成数字
    /// </summary>
    /// <param name="n"></param>
    void tableGame(int n)
    {
        if (n == 3)
        {
            Count -= 1;
        }
        this.contentPane.GetTransition("t"+n).Play();
        // 获取UI中的slotMachine组件
        GComponent slotMachine = this.contentPane.GetChild("table"+n).asCom.GetChild("slotMachine").asCom;


        // 保存当前炸弹概率
        float currentBombProbability = bombProbability;

        // 如果处于table1状态，强制炸弹概率为0
        if (isInTable1)
        {
            currentBombProbability = 0;
        }

        // 决定是否生成炸弹
        bool spawnBomb = _random.NextDouble() < currentBombProbability;
        bool spawnGold = _random.NextDouble() < goldProbability;
        int totalPositions = GetTableFieldsByNumber(n).Count; // 总位置数量
        int? bombPosition = null; // 存储炸弹位置索引，null表示不生成
        int? goldPosition = null; // 存储炸弹位置索引，null表示不生成
        // 如果需要生成炸弹，随机选择一个位置
        if (spawnBomb)
        {
            bombPosition = _random.Next(0, totalPositions); // 0到59之间的随机位置
        }

        // 如果需要生成金币，随机选择一个位置
        if (spawnGold)
        {
            goldPosition = _random.Next(0, totalPositions); // 0到59之间的随机位置
            if (goldPosition == bombPosition)
            {
                goldPosition = _random.Next(0, bombPosition.Value); // 0到59之间的随机位置
            }
        }

        // 遍历所有位置并更新UI
        for (int i = 0; i < totalPositions; i++)
        {
            // 检查是否超出文本组件数量
            if (i >= GetTableFieldsByNumber(n).Count) break;

            GRichTextField textField = GetTableFieldsByNumber(n)[i] as GRichTextField;
            if (textField == null) continue;

            // 处理炸弹位置
            if (bombPosition.HasValue && i == bombPosition.Value)
            {
                textField.text = "B"; 
                continue;
            }

            // 处理金币位置
            if (goldPosition.HasValue && i == goldPosition.Value)
            {
                textField.text = "G"; 
                continue;
            }

            // 非炸弹位置：根据概率生成随机数或保持原值
            if (_random.NextDouble() < randomizeProbability)
            {
                textField.text = _random.Next(0, 101).ToString(); // 生成0-100的随机数
            }
            // 不满足概率则保持原有文本内容不变
        }
    }

    /// <summary>
    /// 爆炸动效
    /// </summary>
    /// <param name="n"></param>
    void tableBomb(int n)
    {
        if (n != 1 && n != 2)
        {
            return;
        }
        this.contentPane.GetChild("table" + n).asCom.GetTransition("t0").Play();

    }


    #region 新增的table3中奖逻辑（取1-5位置，对应索引1-5）

    private List<List<GRichTextField>> _table3_5x5 = new List<List<GRichTextField>>();
    private List<(GRichTextField field, int value)> _winningElements = new List<(GRichTextField, int)>();

    /// <summary>
    /// 启动table3的中奖检测流程（取1-5位置，索引1-5）
    /// </summary>
    public void StartTable3WinningProcess(object obj)
    {

        DetectWinningLinesIn5x5(); // 检测连线（数字即可）
        MoveWinningElementsToTarget(); // 移动元素并计分
    }

    /// <summary>
    /// 从table3提取5x5表格（每12个元素中取1-5位置，即索引1-5的5个，共5行）
    /// </summary>
    private void ExtractTable3_5x5()
    {
        _table3_5x5.Clear();
        int totalElements = table3.Count;
        int groupSize = 12; // 每12个元素为一组

        // 提取5行，每行取对应组的索引1-5（1-5位置）
        for (int row = 0; row < 5; row++)
        {
            List<GRichTextField> currentRow = new List<GRichTextField>();
            // 计算当前组的起始索引（第row组的起始位置）
            int groupStartIndex = row * groupSize;

            // 取当前组中索引1-5的5个元素（对应1-5位置）
            for (int col = 1; col <= 5; col++) // 注意：从1开始，到5结束
            {
                int elementIndex = groupStartIndex + col; // 组内索引1-5
                if (elementIndex >= totalElements) break; // 防止越界

                // 转换为GRichTextField并添加到当前行
                if (table3[elementIndex] is GRichTextField field)
                    currentRow.Add(field);
            }
            _table3_5x5.Add(currentRow);
        }
    }

    /// <summary>
    /// 检测5x5表格中的中奖连线（连续5个数字即可，无需相同）
    /// </summary>
    private void DetectWinningLinesIn5x5()
    {
        _winningElements.Clear();
        if (_table3_5x5.Count != 5) return;


        // 1. 检查所有行（5行）
        for (int row = 0; row < 5; row++)
        {
            // 确保当前行有5个元素（避免索引越界）
            if (_table3_5x5[row].Count != 5) continue;

            bool isFullRow = true;
            for (int col = 0; col < 5; col++)
            {
                if (!IsValidNumberField(_table3_5x5[row][col]))
                {
                    isFullRow = false;
                    break;
                }
            }

            // 若整行都是有效数字，加入所有位置
            if (isFullRow)
            {
                for (int col = 0; col < 5; col++)
                {
                    _winningElements.Add((_table3_5x5[row][col], int.Parse(_table3_5x5[row][col].text)));
                }
            }
        }

        // 2. 检查所有列（5列）
        for (int col = 0; col < 5; col++)
        {
            bool isFullCol = true;
            for (int row = 0; row < 5; row++)
            {
                // 确保当前行列有元素
                if (col >= _table3_5x5[row].Count || !IsValidNumberField(_table3_5x5[row][col]))
                {
                    isFullCol = false;
                    break;
                }
            }

            // 若整列都是有效数字，加入所有位置
            if (isFullCol)
            {
                for (int row = 0; row < 5; row++)
                {
                    if(_winningElements.Contains((_table3_5x5[row][col], int.Parse(_table3_5x5[row][col].text))))
                    {
                        continue;
                    }
                    _winningElements.Add((_table3_5x5[row][col], int.Parse(_table3_5x5[row][col].text)));
                }
            }
        }

        // 3. 检查主对角线（左上角→右下角：row == col）
        bool isMainDiagonal = true;
        for (int i = 0; i < 5; i++)
        {
            if (i >= _table3_5x5[i].Count || !IsValidNumberField(_table3_5x5[i][i]))
            {
                isMainDiagonal = false;
                break;
            }
        }
        if (isMainDiagonal)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_winningElements.Contains((_table3_5x5[i][i], int.Parse(_table3_5x5[i][i].text))))
                {
                    continue;
                }
                _winningElements.Add((_table3_5x5[i][i], int.Parse(_table3_5x5[i][i].text)));
            }
        }

        // 4. 检查副对角线（右上角→左下角：row + col == 4）
        bool isSubDiagonal = true;
        for (int i = 0; i < 5; i++)
        {
            int col = 4 - i; // 列索引 = 4 - 行索引
            if (col >= _table3_5x5[i].Count || !IsValidNumberField(_table3_5x5[i][col]))
            {
                isSubDiagonal = false;
                break;
            }
        }
        if (isSubDiagonal)
        {
            for (int i = 0; i < 5; i++)
            {
                int col = 4 - i;
                if (_winningElements.Contains((_table3_5x5[i][col], int.Parse(_table3_5x5[i][col].text))))
                {
                    continue;
                }
                _winningElements.Add((_table3_5x5[i][col], int.Parse(_table3_5x5[i][col].text)));
            }
        }
        SortWinningElements(); // 按位置排序
    }

    /// <summary>
    /// 判断是否为有效数字字段（非G、非B、非空且可转换为数字）
    /// </summary>
    private bool IsValidNumberField(GRichTextField field)
    {
        string text = field.text;
        return text != "G" && text != "B" && !string.IsNullOrEmpty(text) && int.TryParse(text, out _);
    }

    /// <summary>
    /// 按左到右、上到下排序中奖元素
    /// </summary>
    private void SortWinningElements()
    {
        _winningElements.Sort((a, b) =>
        {
            (int rowA, int colA) = GetElementPositionIn5x5(a.field);
            (int rowB, int colB) = GetElementPositionIn5x5(b.field);
            if (rowA != rowB) return rowA.CompareTo(rowB);
            return colA.CompareTo(colB);
        });
    }

    /// <summary>
    /// 获取元素在5x5表格中的位置
    /// </summary>
    private (int row, int col) GetElementPositionIn5x5(GRichTextField field)
    {
        for (int row = 0; row < _table3_5x5.Count; row++)
        {
            for (int col = 0; col < _table3_5x5[row].Count; col++)
            {
                if (_table3_5x5[row][col] == field)
                    return (row, col);
            }
        }
        return (-1, -1);
    }

    /// <summary>
    /// 移动中奖元素到目标位置并累加分数
    /// </summary>
    private void MoveWinningElementsToTarget()
    {
        if (_winningElements.Count == 0 || targetObj2 == null) return;

        int currentIndex = 0;
        Vector2 targetGlobalPos = targetObj2.LocalToGlobal(Vector2.zero);
        const string loaderComponentUrl = "ui://BonusGame2/Symbol02";
        GRoot root = GRoot.inst;

        void MoveNext()
        {
            if (currentIndex >= _winningElements.Count)
                return;

            var (field, value) = _winningElements[currentIndex];
            currentIndex++;

            GComponent fieldParent = field.parent as GComponent;
            if (fieldParent == null)
            {
                // 直接启动下一个，无延迟
                MoveNext();
                return;
            }

            GLoader loader = new GLoader();
            loader.url = loaderComponentUrl;

            GRichTextField currentField = field;
            int currentValue = value;

            GComponent loadedComp = loader.component;
            if (loadedComp != null)
            {
                GRichTextField loaderText = loadedComp.GetChild("text") as GRichTextField;
                if (loaderText != null)
                    loaderText.text = currentField.text;
            }

            currentField.text = "";
            score2 += currentValue;

            // 位置同步
            Vector2 fieldGlobalPos = currentField.LocalToGlobal(Vector2.zero);
            Vector2 loaderLocalPos = root.GlobalToLocal(fieldGlobalPos);
            loader.SetXY(loaderLocalPos.x, loaderLocalPos.y);

            // 移动动画
            Vector2 targetLocalPos = root.GlobalToLocal(targetGlobalPos);
            GTweener tween = loader.TweenMove(targetLocalPos, 1f);
            tween.SetEase(EaseType.QuadOut);
            tween.OnComplete(() =>
            {
                loader.RemoveFromParent();
                loader.Dispose();
            });

            root.AddChild(loader);

            // 关键修改：延迟0.02秒启动下一个移动
            GTween.To(0, 1, 0.2f).OnComplete(MoveNext);
        }

        MoveNext();
    }
    #endregion

}
