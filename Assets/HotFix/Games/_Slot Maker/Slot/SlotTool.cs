using System.Collections.Generic;

namespace SlotMaker
{
    public class SlotTool
    {
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
