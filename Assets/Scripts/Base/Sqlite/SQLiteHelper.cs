using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Data;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.Networking;

public partial class SQLiteHelper : MonoSingleton<SQLiteHelper>
{

    private SqliteConnection _connection;
    private SqliteCommand _command;
    private string _connectString;

    public string curDBName = null;




    /*public class Task
    {
        public string dbName;
        public string sql;
        public Action callback;
    }*/

    List<IEnumerator> taskLst = new List<IEnumerator>();

    Coroutine _co;
    void ClearCo()
    {
        if (_co != null)
            StopCoroutine(_co);
        _co = null;
    }


    // IEnumerator _OpenDB(string dbName = "Default.db",  Action<SqliteConnection> callBack = null)
    IEnumerator _OpenDB(string dbName, Action<SqliteConnection> callBack = null)
    {
        if (string.IsNullOrEmpty(dbName))
        {
            DebugUtils.LogError(" dbName is null ");
            yield break; 
        }

        if (_connection == null || _connection.State == ConnectionState.Closed || curDBName != dbName)
        {            
            Debug.LogWarning($"切换数据库{curDBName}为{dbName}");

            // 关闭数据库
            CloseDB();

/*
#if UNITY_EDITOR
            _connectString = "URI=file:" + Path.Combine(Application.streamingAssetsPath, dbName);
            DebugUtil.Log($"streamingAssets据库 {dbName}  url = {_connectString}");
#elif UNITY_ANDROID

            yield return copyDbFile(dbName);

#endif
*/

            if (Application.isEditor)
            {
                _connectString = "URI=file:" + Path.Combine(Application.streamingAssetsPath, dbName);
                DebugUtils.Log($"streamingAssets据库 {dbName}  url = {_connectString}");
            }
            else
            {

                //yield return copyDbFile(dbName);

                // 不拷贝，直接创建
                string targetDBPath = Path.Combine(Application.persistentDataPath, dbName);
                _connectString = "Data Source=" + targetDBPath;
            }


            _connection = new SqliteConnection(_connectString);
            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    curDBName = dbName;
                    DebugUtils.Log($"数据库 {dbName} 连接已建立并打开.");
                }
                else
                {
                    DebugUtils.LogError($"数据库 {dbName} 连接失败. State:{_connection.State} ");
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log(e);
            }
        }

        callBack?.Invoke(_connection);

        if(taskLst.Count > 0)
        {
            IEnumerator nextTask = taskLst[0];
            taskLst.RemoveAt(0);
            yield return nextTask;
        }

        _co = null;

    }


    IEnumerator copyDbFile(string dbName = "Default.db")
    {
        if (string.IsNullOrEmpty(dbName) )
        {
            DebugUtils.LogError(" dbName is null ");
        }
        //string targetDBPath1 = Application.persistentDataPath + $"/{dbName}";

        string targetDBPath = Path.Combine(Application.persistentDataPath, dbName);

       // DebugUtil.Log($"targetDBPath1 {targetDBPath1}  targetDBPath = {targetDBPath}");

        //connectString = "URI=file:" + targetDBPath;
        _connectString = "Data Source=" + targetDBPath;
        DebugUtils.Log($"本地数据库 {dbName}  url = {_connectString}");

        if (!Directory.Exists(Application.persistentDataPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath);
        }

        if (File.Exists(targetDBPath))
        {
            DebugUtils.Log($"数据库{dbName}存在 不做任何操作");
            yield break;
        }

        DebugUtils.Log($"本地没有数据库，开始拷贝包体数据库{dbName}到本地");


        string sourceDBPath = Path.Combine(Application.streamingAssetsPath, dbName);  // 【待解决】：这个有问题？？

        // 这块代码有问题
        if (File.Exists(sourceDBPath))
        {
            WWW loadWWW = new WWW(sourceDBPath);
            yield return loadWWW;
            File.WriteAllBytes(targetDBPath, loadWWW.bytes);
            DebugUtils.Log($"复制数据库{sourceDBPath}  到本地： {targetDBPath}");
        }
        else{
            DebugUtils.LogError($"包内数据库：{sourceDBPath} 不存在");
        }
    }


    IEnumerator copyDbFile02(string dbName = "Default.db")
    {
        DebugUtils.Log("copyDbFile02");

        string sourceDBPath = Path.Combine(Application.streamingAssetsPath, dbName);
        string targetDBPath = Path.Combine(Application.persistentDataPath, dbName);

        //connectString = "URI=file:" + targetDBPath;
        _connectString = "Data Source=" + targetDBPath;
        DebugUtils.Log($"本地数据库 {dbName}  url = {_connectString}");

        if (File.Exists(targetDBPath))
        {
            DebugUtils.Log($"数据库{ dbName }存在 不做任何操作");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(sourceDBPath);
        yield return request.SendWebRequest();
        if (request.isDone)
        {
            byte[] data = request.downloadHandler.data;
            File.WriteAllBytes(targetDBPath, data);
            DebugUtils.Log($"复制数据库{sourceDBPath}  到本地： {targetDBPath}");
        }

        request.Dispose();
    }


    public void OpenDB(Action<SqliteConnection> callBack)
    {
        if (string.IsNullOrEmpty(curDBName))
        {
            DebugUtils.LogError("curDBName is null");
            return;
        }
        OpenDB(curDBName, callBack);
    }

    //public void OpenDB(string dbName = "Default.db", Action<SqliteConnection> callBack = null)
    public void OpenDB(string dbName, Action<SqliteConnection> callBack = null)
    {
        IEnumerator task = _OpenDB(dbName, callBack);

        if (_co != null)
        {
            taskLst.Add(task);
            return;
        }

        _co = StartCoroutine(task);
    }


    protected override void OnDestroy()
    {

        ClearCo();
        CloseDB();
        taskLst.Clear();
        base.OnDestroy();
    }

    public void CloseDB()
    {
        if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
        {
            _connection.Close();
        }
        _connection = null;
    }




    public SqliteDataReader ExecuteQuery(string sql, string dbName = null)
    {
        sql = sql.Trim();
        if (_connection == null)
        {
            DebugUtils.LogError("connection is null");
            return null;
        }
        else if (dbName != null && dbName != curDBName)
        {
            DebugUtils.LogError($"dbName:{dbName} is not cur DB;  cur DB：{curDBName}");
            return null;
        }
        else if (sql.StartsWith("CREATE TABLE")
            || sql.StartsWith("INSERT INTO")
            || sql.StartsWith("UPDATE")
            || sql.StartsWith("DELETE")
            || sql.StartsWith("DROP"))
        {
            DebugUtils.LogError($"创表、插入、更新、删除 等业务，使用 api: ExecuteNonQuery");
            return null;
        }

        _command = _connection.CreateCommand();
        _command.CommandText = sql;
        SqliteDataReader dataReader = _command.ExecuteReader();
        return dataReader;
    }
    public void ExecuteQueryAfterOpenDB(string dbName, string sql, Action<SqliteDataReader> callBack)
    {
        OpenDB(dbName, (connection) =>
        {
            callBack?.Invoke(ExecuteQuery(sql, dbName));
        });
    }
    public void ExecuteQueryAfterOpenDB(string sql, Action<SqliteDataReader> callBack)=> ExecuteQueryAfterOpenDB(curDBName, sql, callBack);
    




    /// <summary>
    /// * 创建表、刷新表、删除表、插入等操作
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="dbName"></param>
    public void ExecuteNonQuery(string sql, string dbName = null)
    {
        sql = sql.Trim();
        if (_connection == null)
        {
            DebugUtils.LogError("connection is null");
            return;
        }
        else if (dbName != null && dbName != curDBName)
        {
            DebugUtils.LogError($"dbName:{dbName} is not cur DB;  cur DB：{curDBName}");
            return;
        }
        else if (sql.StartsWith("SELECT"))
        {
            DebugUtils.LogError($"查寻业务，使用 api: ExecuteQuery");
            return;
        }

        DebugUtils.Log($"sql = {sql}");

        using (SqliteCommand cmd = new SqliteCommand(sql, _connection))
        {
            cmd.ExecuteNonQuery();
            /*
            if (sql.Contains("CREATE TABLE"))
                DebugUtil.Log("创建表成功.");
            else if (sql.Contains("INSERT INTO"))
                DebugUtil.Log("数据插入成功.");
            else if (sql.Contains("UPDATE"))
                DebugUtil.Log("数据更新成功.");
            else if (sql.Contains("DELETE") || sql.Contains("DROP"))
                DebugUtil.Log("数据删除成功.");
            */
        }
    }
    public void ExecuteNonQueryAfterOpenDB(string dbName, string sql)
    {
        OpenDB(dbName, (connection) =>
        {
            ExecuteNonQuery(sql, dbName);
        });
    }
    public void ExecuteNonQueryAfterOpenDB(string sql) => ExecuteNonQueryAfterOpenDB(curDBName, sql);






    /// <summary>
    /// 数据库数据转json
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="dbName"></param>
    /// <returns></returns>
    public string ConvertSqliteToJson(string sql, string dbName = null)
    {
        if (_connection == null)
        {
            DebugUtils.LogError("connection is null");
            return null;
        }
        else if (dbName != null && dbName != curDBName)
        {
            DebugUtils.LogError($"dbName:{dbName} is not cur DB;  cur DB：{curDBName}");
            return null;
        }
        else if (!sql.Contains("SELECT"))
        {
            DebugUtils.LogError($"必须是 SELECT 业务");
            return null;
        }

        var dataTable = new DataTable();
        using (SqliteCommand cmd = new SqliteCommand(sql, _connection))
        {
            using (var reader = cmd.ExecuteReader())
            {
                dataTable.Load(reader);
            }
        }
        return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
    }

    public void ConvertSqliteToJsonAfterOpenDB(string dbName, string sql, Action<string> callBack)
    {
        OpenDB(dbName, (connection) =>
        {
            /*
            var dataTable = new DataTable();
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }
            callBack.Invoke( JsonConvert.SerializeObject(dataTable, Formatting.Indented));
            */
            callBack.Invoke(ConvertSqliteToJson(sql, dbName));
        });
    }
    public void ConvertSqliteToJsonAfterOpenDB(string sql, Action<string> callBack) => ConvertSqliteToJsonAfterOpenDB(curDBName, sql, callBack);

}




public partial class SQLiteHelper : MonoSingleton<SQLiteHelper>
{

    public const string TYPE_INT = "INTEGER";
    public const string TYPE_STRING = "TEXT";
    public const string TYPE_FLOAT = "REAL";
    //布尔值，存 0 或 1 （"INTEGER" 类型）


    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="columnType"></param>
    /// <returns></returns>
    public static string SQLCreateTable(string tableName, Dictionary<string, string> dicKeyType)
    {
        string[] columns = dicKeyType.Keys.ToArray();
        string[] columnType = dicKeyType.Values.ToArray();

        StringBuilder cmdSrt = new StringBuilder(20);

        //根据参数进行创建表格SQL命令字符串拼接
        //cmdSrt.Append($"CREATE TABLE {tableName}(");
        cmdSrt.Append($"CREATE TABLE {tableName}(id INTEGER PRIMARY KEY,");
        for (int i = 0; i < columns.Length; i++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            //cmdSrt.Append($"{columns[i]} {columnType[i]}");

            // 给默认值：
            if (columnType[i] == TYPE_INT)
            {
                cmdSrt.Append($"{columns[i]} {columnType[i]} DEFAULT 0");
            }else if (columnType[i] == TYPE_FLOAT)
            {
                cmdSrt.Append($"{columns[i]} {columnType[i]} DEFAULT 0.0");
            }
            else
            {
                cmdSrt.Append($"{columns[i]} {columnType[i]}");
            }

        }

        cmdSrt.Append(")");

        return cmdSrt.ToString();
    }




    public static string SQLCreateTable<T>(string tableName)
    {
        Type type = typeof(T);

        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        List<string> columns = new List<string>();
        List<string> columnType = new List<string>();

        foreach (FieldInfo finfo in fieldInfos)
        {
            //object resObj = finfo.GetValue(obj);

            if (finfo.Name == "id")
                continue;

            columns.Add(finfo.Name);
            if (finfo.FieldType == typeof(string))
            {
                columnType.Add(TYPE_STRING);
            }else if (finfo.FieldType == typeof(int) || finfo.FieldType == typeof(long))
            {
                columnType.Add(TYPE_INT);
            }
            else if (finfo.FieldType == typeof(float))
            {
                columnType.Add(TYPE_FLOAT);
            }
            else
            {
                DebugUtils.LogError($" type: {finfo.FieldType} is not allow");
            }
        }


        StringBuilder cmdSrt = new StringBuilder(20);

        //根据参数进行创建表格SQL命令字符串拼接
        //cmdSrt.Append($"CREATE TABLE {tableName}(");
        cmdSrt.Append($"CREATE TABLE {tableName}(id INTEGER PRIMARY KEY,");
        for (int i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            //cmdSrt.Append($"{columns[i]} {columnType[i]}");

            // 给默认值：
            if (columnType[i] == TYPE_INT)
            {
                cmdSrt.Append($"{columns[i]} {columnType[i]} DEFAULT 0");
            }
            else if (columnType[i] == TYPE_FLOAT)
            {
                cmdSrt.Append($"{columns[i]} {columnType[i]} DEFAULT 0.0");
            }
            else
            {
                cmdSrt.Append($"{columns[i]} {columnType[i]}");
            }
        }

        cmdSrt.Append(")");

        return cmdSrt.ToString();
    }




    /// <summary>
    /// "INSERT INTO test_student_info (name, numb, age, height) VALUES ('小韩', 1, 12, 15.9)";
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="keys"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static string SQLInsertTableData(string tableName, Dictionary<string,object> dicKeyValue)
    {

        StringBuilder cmdSrt = new StringBuilder(20);

        //根据参数进行创建表格SQL命令字符串拼接
        cmdSrt.Append($"INSERT INTO {tableName}(");

        string[] keys = dicKeyValue.Keys.ToArray();
        object[] values = dicKeyValue.Values.ToArray();
        for (int i = 0; i < keys.Length; i++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            cmdSrt.Append($"{keys[i]}");
        }
        
        cmdSrt.Append(") VALUES (");


        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            //cmdSrt.Append($" {values[i]}");

            if (values[i] is string)
                cmdSrt.Append(" '" + values[i] + "'");
            else
                cmdSrt.Append(" " + values[i]);
            
        }

        cmdSrt.Append(")");

        return cmdSrt.ToString();
    }




    public static string SQLInsertTableData<T>(string tableName, T obj)
    {
        Type type = typeof(T);

        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        List<string> keys = new List<string>();
        List<object> values = new List<object>();

        foreach (FieldInfo finfo in fieldInfos)
        {
            object resObj = finfo.GetValue(obj);

            if (finfo.Name == "id")
                continue;

            keys.Add(finfo.Name);
            values.Add(resObj);
        }


        StringBuilder cmdSrt = new StringBuilder(20);

        //根据参数进行创建表格SQL命令字符串拼接
        cmdSrt.Append($"INSERT INTO {tableName}(");


        for (int i = 0; i < keys.Count; i++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            cmdSrt.Append($"{keys[i]}");
        }

        cmdSrt.Append(") VALUES (");


        for (int i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            //cmdSrt.Append($" {values[i]}");

            if (values[i] is string)
                cmdSrt.Append(" '" + values[i] + "'");
            else
                cmdSrt.Append(" " + values[i]);

        }

        cmdSrt.Append(")");

        return cmdSrt.ToString();
    }


    public static string SQLUpdateTableData<T>(string tableName, T obj)
    {
        Type type = typeof(T);

        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        List<string> keys = new List<string>();
        List<object> values = new List<object>();
        long id = 1;
        foreach (FieldInfo finfo in fieldInfos)
        {
            object resObj = finfo.GetValue(obj);

            if (finfo.Name == "id") {
                id = (long)resObj;
                continue;
            }

            keys.Add(finfo.Name);
            values.Add(resObj);
        }

        StringBuilder cmdSrt = new StringBuilder(20);

        //根据参数进行创建表格SQL命令字符串拼接
        cmdSrt.Append($"UPDATE {tableName} SET ");

       // "UPDATE test_student_info SET numb = 4, height = 159 WHERE name = '小红33'";

        for (int i = 0; i < keys.Count; i ++)
        {
            if (i > 0)
            {
                cmdSrt.Append(",");
            }

            if (values[i] is string)
                cmdSrt.Append(keys[i] + " = '" + values[i] + "'");
            else
                cmdSrt.Append(keys[i] + " = " + values[i]);
            //cmdSrt.Append($"{keys[i]} = {values[i]}");
        }

        cmdSrt.Append($" WHERE  id = {id}");
        
        return cmdSrt.ToString();
    }




}



public partial class SQLiteHelper : MonoSingleton<SQLiteHelper>
{


    public bool CheckTableExists(string tableName, string dbName = null)
    {
        if (_connection == null)
        {
            DebugUtils.LogError("connection is null");
            return false;
        }
        else if (dbName != null && dbName != curDBName)
        {
            DebugUtils.LogError($"dbName:{dbName} is not cur DB;  cur DB：{curDBName}");
            return false;
        }

        string sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type ='table' and name='{tableName}';";

        //创建命令
        _command = _connection.CreateCommand();
        _command.CommandText = sql;
        int count = Convert.ToInt32(_command.ExecuteScalar());

        //如果结果为1则表示存在该表格
        bool isExists = count == 1;

        return isExists;
    }

    /// <summary>
    /// 获取表的行数
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public long GetTableRowCount(string tableName)
    {
        long rowCount = 0;

        string sql = $"SELECT COUNT(*) FROM {tableName}";

        using (var command = new SqliteCommand(sql, _connection))
        {
            object res = command.ExecuteScalar();
            DebugUtils.Log($"res = {res}");
            rowCount = (long)res;
            DebugUtils.Log($"Table '{tableName}' has {rowCount} rows.");
        }
        return rowCount;
    }

    /// <summary>
    /// 创建表格
    /// </summary>
    [Button]
    void TestCreatTable( )
    {
        SQLiteHelper.Instance.OpenDB(ApplicationSettings.Instance.dbName,  (connect) =>
        {
            if (!SQLiteHelper.Instance.CheckTableExists("test_student_info"))
            {
                string sql = "CREATE TABLE test_student_info(id INTEGER PRIMARY KEY, numb INTEGER, name TEXT, age INTEGER, height REAL, created_at INTEGER)";
                SQLiteHelper.Instance.ExecuteNonQuery(sql);
            }
        });
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    [Button]
    void TestUpdateTable()
    {
        string sql = "UPDATE test_student_info SET numb = 4, height = 159 WHERE name = '小红33'";
        SQLiteHelper.Instance.ExecuteNonQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql);
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    [Button]
    void TestInsertTable(string name="小红33", int numb = 2, int age = 10 ,float height = 158.5f)
    {
        //string sql = "INSERT INTO test_student_info (name, numb, age, height) VALUES ('小红2', 2, 10, 158.5)";
        string sql = "INSERT INTO test_student_info (name, numb, age, height, created_at) VALUES ('@name', @numb, @age, @height, @created_at)";

        long tim = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        sql = sql.Replace("@name", name)
             .Replace("@numb", numb.ToString())
             .Replace("@age", age.ToString())
             .Replace("@height", height.ToString())
             .Replace("@created_at", tim.ToString());


        DebugUtils.Log($"sql = {sql}");
        SQLiteHelper.Instance.ExecuteNonQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql);

    }
    [Button]
    void TestInsertTable02(string name = "小红33", int numb = 2, int age = 10, float height = 158.5f)
    {
        long tim = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string sql = @"INSERT INTO test_student_info 
                    (name, numb, age, height, created_at)
                    VALUES (:name, :numb, :age, :height, :created_at)";

        using (var command = new SqliteCommand(sql, _connection))
        {

            // 添加命名参数并设置值
            command.Parameters.AddWithValue(":name", name);
            command.Parameters.AddWithValue(":numb", numb);
            command.Parameters.AddWithValue(":age", age);
            command.Parameters.AddWithValue(":height", height);
            command.Parameters.AddWithValue(":created_at", tim);

            // 执行命令
            command.ExecuteNonQuery();




            // 读取数据示例
            sql = "SELECT * FROM test_student_info WHERE name = :name";
            using (var command1 = new SqliteCommand(sql, _connection))
            {
                command1.Parameters.AddWithValue(":name", name);

                using (var reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DebugUtils.Log($"ID: {reader.GetInt32(0)}, Numb: {reader.GetInt32(1)}, Name: {reader.GetString(2)}");
                    }
                }
            }



        }





    }

    /// <summary>
    /// 查寻数据
    /// </summary>
    [Button]
    void TestSELECTName(string name = "小红33")
    {
        /*string sql = "SELECT* FROM test_student_info WHERE name = '小红33' AND age = 10";*/
        string sql = "SELECT* FROM test_student_info WHERE name = '@name'";
        sql = sql.Replace("@name", name);
        SQLiteHelper.Instance.ExecuteQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql, (reader) =>
        {
            while (reader.Read())
            {
                //string textData = reader.GetString(reader.GetOrdinal("height"));
                float height = reader.GetFloat(reader.GetOrdinal("height"));
                DebugUtils.Log($"{name}的身高 = {height}");
            }
        });

    }


    /// <summary>
    /// 【可以用】清空表中的数据但保留表结构
    /// </summary>
    [Button]
    void TestClearTable()
    {
        /* TRUNCATE TABLE 语句在 SQLite 中不支持
        string sql = "TRUNCATE TABLE test_student_info";*/

        string sql = "DELETE FROM test_student_info;";
        DebugUtils.Log($"sql = {sql}");
        SQLiteHelper.Instance.ExecuteNonQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql);

    }


    /// <summary>
    /// 【可以用】删除表
    /// </summary>
    [Button]
    void TestDeleteTable()
    {
        string sql = "DROP TABLE test_student_info;";
        DebugUtils.Log($"sql = {sql}");
        SQLiteHelper.Instance.ExecuteNonQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql);
    }


    /// <summary>
    /// 删除表数据
    /// </summary>
    [Button]
    void TestDeleteData(string name = "小红33")
    {
        string sql = "DELETE FROM test_student_info WHERE name = '@name'";
        sql = sql.Replace("@name", name);
        DebugUtils.Log($"sql = {sql}");
        SQLiteHelper.Instance.ExecuteNonQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql);
    }



    /// <summary>
    /// 是否存在表
    /// </summary>
    /// <param name="tableName"></param>
    [Button]
    void TestIsTable(string tableName = "test_student_info")
    {
        SQLiteHelper.Instance.OpenDB(ApplicationSettings.Instance.dbName, (connect) =>
        {
            if (!SQLiteHelper.Instance.CheckTableExists(tableName))
            {
                DebugUtils.Log($"不存在表 {tableName} ");
            }
            else
            {
                DebugUtils.Log($"存在表 {tableName} ");
            }
        });
    }


    /// <summary>
    /// 查看表的数据条数
    /// </summary>
    /// <param name="tableName"></param>
    [Button]
    void TestGetTableRowCount(string tableName = "slot_game_record")
    {
        SQLiteHelper.Instance.OpenDB(ApplicationSettings.Instance.dbName, (connection) =>
        {
            if (SQLiteHelper.Instance.CheckTableExists(tableName))
            {
                string sql = $"SELECT COUNT(*) FROM {tableName}";
                DebugUtils.Log(sql);
                using (var command = new SqliteCommand(sql, connection))
                {
                    object res = command.ExecuteScalar();
                    DebugUtils.Log($"res = {res}");
                    long  rowCount = (long)res;
                    DebugUtils.Log($"Table '{tableName}' has {rowCount} rows.");
                }
            }
            else
            {
                DebugUtils.Log($"不存在表 {tableName} ");
            }
        });
    }


    /// <summary>
    /// 行数据转json字符串
    /// </summary>
    void TestTable2Json()
    {
        OpenDB((connection) =>
        {
            string sql = "SELECT* FROM test_student_info WHERE name = '小红'";
            var dataTable = new DataTable();
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }

            DebugUtils.Log($" data to json :{JsonConvert.SerializeObject(dataTable, Formatting.Indented)}");
        });
    }


    /// <summary>
    /// 保留最新的n条数据
    /// </summary>
    /// <param name="remainRow"></param>
    [Button]
    void TestDeleteTableRemainRow(int remainRow = 3)
    {

        //"DELETE FROM your_table WHERE created_at NOT IN (  SELECT created_at FROM (  SELECT created_at FROM your_table ORDER BY created_at DESC LIMIT 10 ))";

        string tableName = "test_student_info";
        string rowName = "created_at";
        string sql = $"DELETE FROM {tableName} WHERE {rowName} NOT IN (  SELECT {rowName} FROM (  SELECT {rowName} FROM {tableName} ORDER BY {rowName} DESC LIMIT {remainRow} ))";

        DebugUtils.Log($"sql = {sql}");
        SQLiteHelper.Instance.ExecuteNonQueryAfterOpenDB(ApplicationSettings.Instance.dbName, sql);

    }
}


