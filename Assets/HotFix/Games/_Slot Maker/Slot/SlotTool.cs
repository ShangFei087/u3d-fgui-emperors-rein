using System.Collections.Generic;

namespace SlotMaker
{
    public class SlotTool
    {
        /// <summary>
        /// 将按列组织的二维牌面数据转换为字符串（列优先输出，列之间用#分隔）。
        /// </summary>
        /// <example>
        /// 若按矩阵视角为 [3][5]（3行5列）：
        /// [[1,2,3,4,5],[6,7,8,9,10],[11,12,13,14,15]]
        /// 对应输入列数据：[[1,6,11],[2,7,12],[3,8,13],[4,9,14],[5,10,15]]
        /// 输出字符串："1,6,11#2,7,12#3,8,13#4,9,14#5,10,15"
        /// </example>
        /// <param name="deckColRowList">按列存储的牌面数据集合。</param>
        /// <returns>形如 "1,2,3#4,5,6" 的列优先字符串。</returns>
        public static string GetDeckColRow(List<List<int>> deckColRowList)
        {
            string res = "";
            for (int col = 0; col < deckColRowList.Count; col++)
            {
                for (int row = 0; row < deckColRowList[col].Count; row++)
                {
                    res += $"{deckColRowList[col][row]},";
                }
                res += "#";
            }

            res = res.Replace(",#", "#").TrimEnd('#');

            return res;

        }

        /// <summary>
        /// 将按列组织的二维牌面数据转换为字符串（按行遍历输出，行之间用#分隔）。
        /// </summary>
        /// <example>
        /// 输入列数据：[[1,6,11],[2,7,12],[3,8,13],[4,9,14],[5,10,15]]
        /// 输出字符串："1,2,3,4,5#6,7,8,9,10#11,12,13,14,15"
        /// </example>
        /// <param name="deckColRowList">按列存储的牌面数据集合。</param>
        /// <returns>形如 "1,4,7#2,5,8" 的行优先字符串。</returns>
        public static string GetDeckRowCol(List<List<int>> deckColRowList)
        {
            string res = "";
            int rowCount = deckColRowList[0].Count;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < deckColRowList.Count; col++)
                {
                    res += $"{deckColRowList[col][row]},";

                }
                res += "#";
            }
            res = res.Replace(",#", "#").TrimEnd('#');

            return res;
        }


        /// <summary>
        /// 将行优先字符串解析为一维列优先数组。
        /// </summary>
        /// <example>
        /// 输入字符串："1,2,3,4,5#6,7,8,9,10#11,12,13,14,15"
        /// 输出列表：[1,6,11,2,7,12,3,8,13,4,9,14,5,10,15]
        /// </example>
        /// <param name="strDeckRowCol">行优先字符串，行之间用#分隔，列之间用,分隔。</param>
        /// <returns>列优先排列的一维整型列表。</returns>
        public static List<int> GetDeckColRow(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            string[] rows = strDeckRowCol.Split('#');
            int rowNum = rows.Length;
            int colNum = rows[0].Split(',').Length;

            List<List<int>> rowcolLst = new List<List<int>>();
            foreach (string row in rows)
            {
                List<int> _row = new List<int>();
                string[] cols = row.Split(',');
                foreach (string col in cols)
                {
                    _row.Add(int.Parse(col));
                }
                rowcolLst.Add(_row);
            }
            List<int> colrow = new List<int>();
            for (int idxCol = 0; idxCol < colNum; idxCol++)
            {
                for (int idxRow = 0; idxRow < rowNum; idxRow++)
                {
                    colrow.Add(rowcolLst[idxRow][idxCol]);
                }
            }
            return colrow;
        }

        /// <summary>
        /// 将行优先字符串解析为按列存储的二维列表（每列内部按从下到上顺序）。
        /// </summary>
        /// <param name="strDeckRowCol">行优先字符串，行之间用#分隔，列之间用,分隔。</param>
        /// <returns>按列组织的二维整型列表。</returns>
        public static List<List<int>> GetDeckColRow02(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            string[] rows = strDeckRowCol.Split('#');
            int rowNum = rows.Length;
            int colNum = rows[0].Split(',').Length;

            List<List<int>> colrowLst = new List<List<int>>();

            for (int i = 0; i < colNum; i++)
            {
                List<int> _col = new List<int>();
                for (int rowIndex = rowNum - 1; rowIndex >= 0; rowIndex--)
                {
                    string[] cols = rows[rowIndex].Split(',');
                    _col.Add(int.Parse(cols[i]));
                }
                colrowLst.Add(_col);
            }
            return colrowLst;
        }

        /// <summary>
        /// 将行优先字符串解析为按列存储的二维列表（每列内部按从上到下顺序）。
        /// </summary>
        /// <example>
        /// 输入字符串："1,2,3,4,5#6,7,8,9,10#11,12,13,14,15"
        /// 输出列数据：[[1,6,11],[2,7,12],[3,8,13],[4,9,14],[5,10,15]]
        /// </example>
        /// <param name="strDeckRowCol">行优先字符串，行之间用#分隔，列之间用,分隔。</param>
        /// <returns>按列组织的二维整型列表（列内顺序为上到下）。</returns>
        public static List<List<int>> GetDeckColRow03(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            string[] rows = strDeckRowCol.Split('#');
            int rowNum = rows.Length;
            int colNum = rows[0].Split(',').Length;

            List<List<int>> colrowLst = new List<List<int>>();
            for (int colIndex = 0; colIndex < colNum; colIndex++)
            {
                List<int> _col = new List<int>();
                for (int rowIndex = 0; rowIndex < rowNum; rowIndex++)
                {
                    string[] cols = rows[rowIndex].Split(',');
                    _col.Add(int.Parse(cols[colIndex]));
                }

                colrowLst.Add(_col);
            }

            return colrowLst;
        }


        /// <summary>
        /// 将行优先字符串解析为一维行优先数组。
        /// </summary>
        /// <example>
        /// 输入字符串："1,2,3,4,5#6,7,8,9,10#11,12,13,14,15"
        /// 输出列表：[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15]
        /// </example>
        /// <param name="strDeckRowCol">行优先字符串，行之间用#分隔，列之间用,分隔。</param>
        /// <returns>行优先排列的一维整型列表。</returns>
        public static List<int> GetDeckRowCol(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            string[] rows = strDeckRowCol.Split('#');
            //int rowNum = rows.Length;
            //int colNum = rows[0].Split(',').Length;

            List<string> rowcol = new List<string>();
            foreach (string row in rows)
            {
                rowcol.AddRange(row.Split(','));
            }

            List<int> rowcol01 = new List<int>();
            foreach (string item in rowcol)
            {
                rowcol01.Add(int.Parse(item));
            }

            return rowcol01;
        }


        /// <summary>
        /// 将列优先一维数组还原为按列组织的二维列表。
        /// </summary>
        /// <example>
        /// 输入数组：[1,6,11,2,7,12,3,8,13,4,9,14,5,10,15]，colCount=5，rowCount=3
        /// 输出列数据：[[1,6,11],[2,7,12],[3,8,13],[4,9,14],[5,10,15]]
        /// </example>
        /// <param name="deckColRow">列优先一维牌面数据。</param>
        /// <param name="colCount">列数。</param>
        /// <param name="rowCount">每列行数。</param>
        /// <returns>按列组织的二维整型列表。</returns>
        public static List<List<int>> GetDeckColRow(int[] deckColRow, int colCount, int rowCount)
        {
            List<List<int>> colrowLsts = new List<List<int>>();
            for (int col = 0; col < colCount; col++)
            {
                List<int> colLst = new List<int>();
                for (int row = 0; row < rowCount; row++)
                {
                    int syb = deckColRow[col * rowCount + row];
                    colLst.Add(syb);
                }
                colrowLsts.Add(colLst);
            }
            return colrowLsts;
        }

        /// <summary>
        /// 将行优先一维数组转换为按列组织的二维列表。
        /// </summary>
        /// <example>
        /// 输入数组：[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15]，colCount=5，rowCount=3
        /// 输出列数据：[[1,6,11],[2,7,12],[3,8,13],[4,9,14],[5,10,15]]
        /// </example>
        /// <param name="deckRowCol">行优先一维牌面数据。</param>
        /// <param name="colCount">列数。</param>
        /// <param name="rowCount">每列行数。</param>
        /// <returns>按列组织的二维整型列表。</returns>
        public static List<List<int>> GetDeckColRow01(int[] deckRowCol, int colCount, int rowCount)
        {
            List<List<int>> colrowLsts = new List<List<int>>();

            for (int col = 0; col < colCount; col++)
            {
                List<int> colLst = new List<int>();
                for (int row = 0; row < rowCount; row++)
                {
                    int syb = deckRowCol[row * colCount + col];
                    colLst.Add(syb);
                }
                colrowLsts.Add(colLst);
            }
            return colrowLsts;
        }

        /// <summary>
        /// 将行优先一维数组还原为按行组织的二维列表。
        /// </summary>
        /// <param name="deckRowCol">行优先一维牌面数据。</param>
        /// <param name="colCount">列数。</param>
        /// <param name="rowCount">行数。</param>
        /// <returns>按行组织的二维整型列表。</returns>
        public static List<List<int>> GetDeckRowCol01(int[] deckRowCol, int colCount, int rowCount)
        {
            List<List<int>> lst = new List<List<int>>();
            for (int row = 0; row < rowCount; row++)
            {
                lst.Add(new List<int>());
                for (int col = 0; col < colCount; col++)
                {
                    lst[row].Add(deckRowCol[row * 5 + col]);
                }
            }
            return lst;
        }





        /// <summary>
        /// 将行优先一维数组格式化为字符串（行之间用#分隔）。
        /// </summary>
        /// <example>
        /// 输入数组：[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15]，colCount=5，rowCount=3
        /// 输出字符串："1,2,3,4,5#6,7,8,9,10#11,12,13,14,15"
        /// </example>
        /// <param name="deckRowCol">行优先一维牌面数据。</param>
        /// <param name="colCount">列数。</param>
        /// <param name="rowCount">行数。</param>
        /// <returns>形如 "1,2,3#4,5,6" 的牌面字符串。</returns>
        public static string GetDeckRowCol(int[] deckRowCol, int colCount, int rowCount)
        {

            string res = "";

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    res += $"{deckRowCol[row * 5 + col]},";

                }
                res += "#";
            }
            res = res.Replace(",#", "#").TrimEnd('#');

            return res;

        }


    }
}
