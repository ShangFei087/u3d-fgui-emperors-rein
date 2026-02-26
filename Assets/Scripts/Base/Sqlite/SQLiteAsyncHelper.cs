using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Linq;
using Newtonsoft.Json;





public partial class SQLiteAsyncHelper : MonoSingleton<SQLiteAsyncHelper>
{
    public bool isInit = false;

    // string TableName = "PlayerPrefs";
    IEnumerator Start()
    {

        // bool isNext = false;

        Initialize();

        //yield return new WaitUntil(() => dbConnection != null);
        yield return new WaitUntil(() => dbConnection != null && dbConnection.State == ConnectionState.Open);

        isInit = true;
    }


    /*   
    Coroutine corInit = null;
    void Init()
    {
        if (corInit != null)
            StopCoroutine(corInit);
        corInit = StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        while (isEnableReconnect) { 

            isConnect = false;
            Initialize();

            float lastTimeS = Time.unscaledTime;
            yield return new WaitUntil(() => ( dbConnection != null && dbConnection.State == ConnectionState.Open) 
            || Time.unscaledTime - lastTimeS > 5f);

            if (Time.unscaledTime - lastTimeS <= 5f)
            {
                isConnect = true;

                while (isConnect)
                {
                    yield return null;
                }
            }
        }
    }

    bool isEnableReconnect = true;
    bool isConnect = false;
    */

}

public partial class SQLiteAsyncHelper : MonoSingleton<SQLiteAsyncHelper>
{

    public static string databaseName => ApplicationSettings.Instance.dbName; // "PssOn00152.db";

    private Thread dbThread;
    private Queue<Action> writeQueue = new Queue<Action>();
    private Queue<Action> readQueue = new Queue<Action>();
    private Queue<Action> mainThreadQueue = new Queue<Action>();


    /// <summary> isEanbleDB </summary>
    private bool isRunning = false;
    // private IDbConnection dbConnection = null;
    private SqliteConnection dbConnection = null;

    public bool isConnect => dbConnection != null && dbConnection.State == ConnectionState.Open;


    // 初始化数据库
    public void Initialize()
    {
        if (isRunning) return;

        isRunning = true;
        dbThread = new Thread(DatabaseThread);
        dbThread.IsBackground = true;
        dbThread.Start(databaseName);
    }


    // 数据库线程方法
    private void DatabaseThread(object dbName)
    {
        try
        {


#if UNITY_EDITOR
            string connectionString = "URI=file:" + Path.Combine(Application.streamingAssetsPath, databaseName) +
                                     ";Version=3;Pooling=true;Max Pool Size=100;";
#elif UNITY_ANDROID

        string dataSandBoxPath = Path.Combine(Application.persistentDataPath, databaseName);
        if (!Directory.Exists(Application.persistentDataPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath);
        }

        string connectionString = "URI=file:" + dataSandBoxPath +
                                     ";Version=3;Pooling=true;Max Pool Size=100;";
#endif


            dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();

            while (isRunning)
            {
                // 处理写操作
                if (writeQueue.Count > 0)
                {
                    Action writeAction;
                    lock (writeQueue)
                    {
                        writeAction = writeQueue.Dequeue();
                    }
                    writeAction.Invoke();

                    // 写操作后等待一小段时间，减少锁竞争
                    Thread.Sleep(10);
                }

                // 处理读操作
                if (readQueue.Count > 0 && writeQueue.Count == 0)
                {
                    Action readAction;
                    lock (readQueue)
                    {
                        readAction = readQueue.Dequeue();
                    }
                    readAction.Invoke();
                }

                Thread.Sleep(1);
            }
        }
        catch (Exception e)
        {
            if (isRunning || !string.IsNullOrEmpty(e.Message))
            {
                Debug.LogError("数据库线程错误: " + e.Message);
                EnqueueToMainThread(() => { Debug.LogError("数据库操作失败: " + e.Message); });
            }
        }
        finally
        {
            CloseDatabase();
            //如果不是退出游戏或主动关闭，则重连？
        }
    }


    /// <summary>
    /// 执行非查询操作（写操作）
    /// </summary>
    /// <param name="query"></param>
    /// <param name="callback"></param>
    public void ExecuteNonQueryAsync(string query, Action<bool> callback = null)
    {
        lock (writeQueue)
        {
            writeQueue.Enqueue(() =>
            {
                bool success = false;
                try
                {
                    using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"执行非查询失败, error: {e.Message}  sql: {query}");
                }

                if (callback != null)
                    EnqueueToMainThread(() => callback(success));
            });
        }
    }


    /// <summary>
    /// 执行非查询操作（写操作）
    /// </summary>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    public void ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters, Action<bool> callback = null)
    {
        lock (writeQueue)
        {
            writeQueue.Enqueue(() =>
            {
                bool success = false;
                try
                {
                    /*using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                        success = true;
                    }*/


                    using (var command = new SqliteCommand(query, dbConnection))
                    {

                        foreach (KeyValuePair<string, object> kv in parameters)
                        {
                            command.Parameters.AddWithValue(kv.Key, kv.Value);
                        }
                        /*
                        command.Parameters.AddWithValue("@startTimestamp", startTimestamp);
                        command.Parameters.AddWithValue("@endTimestamp", endTimestamp);
                        command.Parameters.AddWithValue("@createdAt", timestamp);
                        command.Parameters.AddWithValue("@credit", credit);
                        command.Parameters.AddWithValue("@creditBefore", creditBefore);
                        command.Parameters.AddWithValue("@creditAfter", creditAfter);
                        */

                        /*
                        1. rowsAffected > 0
                        * 表示 SQL 语句至少影响了一行数据。此时进一步判断：
                        * rowsAffected == 1 说明执行了INSERT操作（插入了一条新记录），日志输出： "New data inserted"
                        * 否则 说明执行了UPDATE操作（更新了现有记录），日志输出： "Data updated"
                        2. rowsAffected <= 0
                        * 表示 SQL 语句没有影响任何数据。通常发生在：
                        * 条件不匹配（如WHERE子句未找到记录）
                        * 插入重复数据但被约束阻止
                        * SQL 逻辑错误
                        */
                        int rowsAffected = command.ExecuteNonQuery();

                        DebugUtils.Log(rowsAffected > 0 ?
                            (rowsAffected == 1 ? "【sqlite async helper】New data inserted" : "【sqlite async helper】Data updated") :
                            "【sqlite async helper】No operation performed");

                        success = rowsAffected > 0;

                    }

                }
                catch (Exception e)
                {
                    Debug.LogError($"执行非查询失败, error: {e.Message}  sql: {query}");
                }

                if (callback != null)
                    EnqueueToMainThread(() => callback(success));

            });
        }
    }


    /// <summary>
    /// 执行查询操作（读操作）
    /// </summary>
    /// <param name="query"></param>
    /// <param name="callback"></param>
    public void ExecuteQueryAsync(string query, Action<DataTable> callback)
    {
        if (callback == null) return;

        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {
                DataTable dataTable = new DataTable();
                try
                {
                    using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        using (IDataReader reader = cmd.ExecuteReader())
                        {
                            dataTable.Load(reader);
                            reader.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行查询失败: " + e.Message);
                }

                EnqueueToMainThread(() => callback(dataTable));
            });
        }
    }




    /// <summary>
    /// 执行查询操作（读操作）
    /// </summary>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    public void ExecuteQueryAsync(string query, Dictionary<string, object> parameters, Action<SqliteDataReader> callback)
    {
        if (callback == null) return;

        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {
                try
                {
                    using (SqliteCommand command = new SqliteCommand(query, dbConnection))
                    {
                        if (parameters != null)
                            foreach (KeyValuePair<string, object> kv in parameters)
                            {
                                command.Parameters.AddWithValue(kv.Key, kv.Value);
                            }

                        SqliteDataReader reader = command.ExecuteReader();

                        EnqueueToMainThread(() => callback(reader));
                        /*
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            EnqueueToMainThread(() => callback(reader));
                        }*/
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行查询失败: " + e.Message);
                }

            });
        }
    }



    /// <summary>
    /// 【这个写法用不来，有bug】存在数据则刷新，不存在则插入数据
    /// </summary>
    /// <param name="selectQuery"></param>
    /// <param name="updateQuery"></param>
    /// <param name="insertQuery"></param>
    /// <param name="callback"></param>
    /// <remarks>
    /// * 使用transaction，导致数据写不了
    /// </remarks>
    private void ExecuteUpdateOrInsertBUG(string selectQuery, string updateQuery, string insertQuery, Action<bool> callback)
    {
        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {

                Debug.Log($"【sqlite async helper】 selectQuery = {selectQuery}");
                Debug.Log($"【sqlite async helper】 updateQuery = {updateQuery}");
                Debug.Log($"【sqlite async helper】 insertQuery = {insertQuery}");

                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqliteCommand selectCommand = new SqliteCommand(selectQuery, dbConnection, transaction))
                        {

                            SqliteDataReader reader = selectCommand.ExecuteReader();

                            if (reader.Read()) //reader.HasRows
                            {
                                // 数据存在，执行更新操作
                                reader.Close(); // 关闭读取器才能执行下一个命令

                                using (SqliteCommand updateCommand = new SqliteCommand(updateQuery, dbConnection, transaction))
                                {
                                    int rowsAffected = updateCommand.ExecuteNonQuery();

                                    DebugUtils.Log(rowsAffected > 0 ?
                                        (rowsAffected == 1 ? "【sqlite async helper】New data inserted" : "【sqlite async helper】Data updated") :
                                        "【sqlite async helper】No operation performed");


                                    EnqueueToMainThread(() => callback?.Invoke(rowsAffected > 0));
                                }
                            }
                            else
                            {
                                // 数据不存在，执行插入操作
                                reader.Close();

                                using (SqliteCommand insertCommand = new SqliteCommand(insertQuery, dbConnection, transaction))
                                {
                                    int rowsAffected = insertCommand.ExecuteNonQuery();

                                    DebugUtils.Log(rowsAffected > 0 ?
                                        (rowsAffected == 1 ? "【sqlite async helper】New data inserted" : "【sqlite async helper】Data updated") :
                                        "【sqlite async helper】No operation performed");


                                    EnqueueToMainThread(() => callback?.Invoke(rowsAffected > 0));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("执行查询失败: " + e.Message);
                        if (transaction != null) transaction.Rollback(); // 回滚事务
                        EnqueueToMainThread(() => callback?.Invoke(false));
                        throw;
                    }
                    finally
                    {
                        // if (transaction != null) transaction.Dispose();

                    }
                }

            });
        }
    }




    public void ExecuteUpdateOrInsertAsync(string selectQuery, string updateQuery, string insertQuery, Action<bool> callback)
    {
        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {

                //Debug.Log($"【sqlite async helper】 selectQuery = {selectQuery}");
                //Debug.Log($"【sqlite async helper】 updateQuery = {updateQuery}");
                //Debug.Log($"【sqlite async helper】 insertQuery = {insertQuery}");

                try
                {
                    using (SqliteCommand selectCommand = new SqliteCommand(selectQuery, dbConnection))
                    {

                        SqliteDataReader reader = selectCommand.ExecuteReader();

                        if (reader.HasRows) //  或   if (reader.Read())
                        {
                            // 数据存在，执行更新操作
                            reader.Close(); // 关闭读取器才能执行下一个命令

                            using (SqliteCommand updateCommand = new SqliteCommand(updateQuery, dbConnection))
                            {
                                int rowsAffected = updateCommand.ExecuteNonQuery();
                                DebugUtils.Log($"【sqlite async helper】 updateQuery = {updateQuery}");
                                DebugUtils.Log(rowsAffected > 0 ?
                                    (rowsAffected == 1 ? "【sqlite async helper】New data inserted" : "【sqlite async helper】Data updated") :
                                    "【sqlite async helper】No operation performed");

                                EnqueueToMainThread(() => callback?.Invoke(rowsAffected > 0));
                            }
                        }
                        else
                        {
                            // 数据不存在，执行插入操作
                            reader.Close();

                            using (SqliteCommand insertCommand = new SqliteCommand(insertQuery, dbConnection))
                            {
                                int rowsAffected = insertCommand.ExecuteNonQuery();
                                DebugUtils.Log($"【sqlite async helper】 insertQuery = {insertQuery}");
                                DebugUtils.Log(rowsAffected > 0 ?
                                    (rowsAffected == 1 ? "【sqlite async helper】New data inserted" : "【sqlite async helper】Data updated") :
                                    "【sqlite async helper】No operation performed");

                                EnqueueToMainThread(() => callback?.Invoke(rowsAffected > 0));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行查询失败: " + e.Message);
                    Debug.LogError($"【selectQuery】: {selectQuery}  【updateQuery】: {updateQuery}  【insertQuery】: {insertQuery}");
                    EnqueueToMainThread(() => callback?.Invoke(false));
                    throw;
                }
                finally
                {

                }
            });
        }
    }




    /// <summary>
    /// 删除超出的数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="maxRecord"></param>
    /// <param name="rowName"></param>
    /// <param name="callback"></param>
    public void ExecuteDeleteOverflowAsync(string tableName, int maxRecord, string rowName, Action<object[]> callback)
    {
        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {

                object[] res = new object[] { 0 };
                try
                {
                    // 检查数据库连接状态
                    // if (dbConnection.State != System.Data.ConnectionState.Open) dbConnection.Open();

                    string existsQuery = $"SELECT COUNT(*) FROM sqlite_master WHERE type ='table' and name='{tableName}';";

                    using (SqliteCommand existsCommand = new SqliteCommand(existsQuery, dbConnection))
                    {

                        int count = Convert.ToInt32(existsCommand.ExecuteScalar());
                        //如果结果为1则表示存在该表格
                        bool isExists = count == 1;
                        if (isExists) // 
                        {

                            string countQuery = $"SELECT COUNT(*) FROM {tableName}";

                            using (SqliteCommand countCommand = new SqliteCommand(countQuery, dbConnection))
                            {
                                object result = countCommand.ExecuteScalar();
                                long rowCount = (long)result;
                                if (maxRecord < rowCount)
                                {
                                    string deleteSQL = $"DELETE FROM {tableName} WHERE {rowName} NOT IN (  SELECT {rowName} FROM (  SELECT {rowName} FROM {tableName} ORDER BY {rowName} DESC LIMIT {maxRecord} ))";

                                    using (SqliteCommand deleteCommand = new SqliteCommand(deleteSQL, dbConnection))
                                    {
                                        int rowsAffected = deleteCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        else
                        {
                            res = new object[] { 1, $"表 {tableName} 不存在" };
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}  maxRecord: {maxRecord}  rowName: {rowName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }




    /// <summary>
    /// 删除表数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="callback"></param>
    public void ExecuteDeleteAsync(string tableName, Action<object[]> callback)
    {
        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {

                object[] res = new object[] { 0 };
                try
                {
                    // 检查数据库连接状态
                    // if (dbConnection.State != System.Data.ConnectionState.Open) dbConnection.Open();

                    string existsQuery = $"SELECT COUNT(*) FROM sqlite_master WHERE type ='table' and name='{tableName}';";

                    using (SqliteCommand existsCommand = new SqliteCommand(existsQuery, dbConnection))
                    {

                        int count = Convert.ToInt32(existsCommand.ExecuteScalar());
                        //如果结果为1则表示存在该表格
                        bool isExists = count == 1;
                        if (isExists) // 
                        {
                            string deleteSQL = $"DELETE FROM {tableName};";

                            using (SqliteCommand deleteCommand = new SqliteCommand(deleteSQL, dbConnection))
                            {
                                int rowsAffected = deleteCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            res = new object[] { 1, $"表 {tableName} 不存在" };
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }





    public void CheckOrCreatTableAsync<T>(string tableName, T[] defaultValue, Action<object[]> callback)
    {

        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {

                object[] res = new object[] { 0 };
                try
                {
                    // 检查数据库连接状态
                    // if (dbConnection.State != System.Data.ConnectionState.Open) dbConnection.Open();

                    string existsQuery = $"SELECT COUNT(*) FROM sqlite_master WHERE type ='table' and name='{tableName}';";

                    using (SqliteCommand existsCommand = new SqliteCommand(existsQuery, dbConnection))
                    {
                        int count = Convert.ToInt32(existsCommand.ExecuteScalar());
                        //如果结果为1则表示存在该表格
                        bool isExists = count == 1;
                        if (!isExists) // 
                        {

                            // 创建表
                            string creatSQL = SQLCreateTable<T>(tableName);
                            using (SqliteCommand creatCommand = new SqliteCommand(creatSQL, dbConnection))
                            {
                                creatCommand.ExecuteNonQuery();
                            }

                            // 插入默认数据
                            if (defaultValue != null && defaultValue.Length > 0)
                            {
                                foreach (T item in defaultValue)
                                {
                                    string insertSQL = SQLInsertTableData<T>(tableName, item);

                                    using (SqliteCommand insertCommand = new SqliteCommand(insertSQL, dbConnection))
                                    {
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }



    public void CheckOrCreatTableAsync02<T>(string tableName, T[] defaultValue, Action<object[]> callback)
    {
        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {
                object[] res = new object[] { 0 };
                try
                {
                    // 创建表
                    string creatSQL = SQLCreateTableIfNotExists<T>(tableName);
                    using (SqliteCommand creatCommand = new SqliteCommand(creatSQL, dbConnection))
                    {
                        creatCommand.ExecuteNonQuery();


                        // 插入默认数据
                        if (defaultValue != null && defaultValue.Length > 0)
                        {
                            foreach (T item in defaultValue)
                            {
                                string insertSQL = SQLInsertTableData<T>(tableName, item);

                                using (SqliteCommand insertCommand = new SqliteCommand(insertSQL, dbConnection))
                                {
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }




    public void GetDataAsync<T>(string tableName, string selectSQL, T[] defaultValue, Action<object[]> callback)
    {
        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {
                object[] res = new object[] { 0, "[]" };
                try
                {
                    // 创建表
                    string creatSQL = SQLCreateTableIfNotExists<T>(tableName);
                    using (SqliteCommand creatCommand = new SqliteCommand(creatSQL, dbConnection))
                    {
                        creatCommand.ExecuteNonQuery();
                    }

                    var dataTable = new DataTable();
                    using (SqliteCommand cmd = new SqliteCommand(selectSQL, dbConnection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            /*  if (reader.Read()) // 这样会导致读不到数据！！！！！
                                 dataTable.Load(reader);*/
                            dataTable.Load(reader);
                        }
                    }
                    string result = JsonConvert.SerializeObject(dataTable, Formatting.Indented);

                    if (!string.IsNullOrEmpty(result) && result != "[]")
                    {
                        res = new object[] { 0, result };
                    }
                    else
                    {
                        // 插入默认数据
                        if (defaultValue != null && defaultValue.Length > 0)
                        {
                            foreach (T item in defaultValue)
                            {
                                string insertSQL = SQLInsertTableData<T>(tableName, item);

                                using (SqliteCommand insertCommand = new SqliteCommand(insertSQL, dbConnection))
                                {
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                            // 再次获取数据
                            var dataTable02 = new DataTable();
                            using (SqliteCommand cmd = new SqliteCommand(selectSQL, dbConnection))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    /* if (reader.Read()) // 这样会导致读不到数据！！！！！
                                         dataTable02.Load(reader);*/
                                    dataTable02.Load(reader);
                                }
                            }
                            string result02 = JsonConvert.SerializeObject(dataTable02, Formatting.Indented);
                            res = new object[] { 0, result02 };
                        }
                    }


                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }


    /*
        // 检查是否存在 id=0 的记录
        var checkQuery = "SELECT * FROM TableBussinessTotalRecordItem WHERE id = 0";
                using var checkCommand = new SQLiteCommand(checkQuery, connection, transaction);
    using var reader = checkCommand.ExecuteReader();

    if (reader.Read())
    {
        // 存在记录，转换为对象并序列化为 JSON
        var record = ReadRecordFromReader(reader);
        return JsonSerializer.Serialize(record);
    }
    else
    {
        // 不存在记录，插入默认值
        var defaultRecord = CreateDefaultRecord();
        InsertRecord(defaultRecord, connection, transaction);
        return JsonSerializer.Serialize(defaultRecord);
    }

    */





    /// <summary>
    /// 删除表格
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="callback"></param>
    /// <remarks>
    /// * 可以使用
    /// </remarks>
    public void ExecuteDropTableAsync(string tableName, Action<object[]> callback)
    {

        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {

                object[] res = new object[] { 0 };
                try
                {
                    // 检查数据库连接状态
                    // if (dbConnection.State != System.Data.ConnectionState.Open) dbConnection.Open();
                    string existsQuery = $"SELECT COUNT(*) FROM sqlite_master WHERE type ='table' and name='{tableName}';";

                    using (SqliteCommand existsCommand = new SqliteCommand(existsQuery, dbConnection))
                    {
                        int count = Convert.ToInt32(existsCommand.ExecuteScalar());
                        //如果结果为1则表示存在该表格
                        bool isExists = count == 1;
                        Debug.Log($"is {tableName} Exists: {isExists}");
                        if (isExists) // 
                        {
                            string dropSQL = $"DROP TABLE {tableName};";
                            Debug.Log(dropSQL);
                            using (SqliteCommand dropCommand = new SqliteCommand(dropSQL, dbConnection))
                            {
                                dropCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }

    // DROP TABLE IF EXISTS users;


    /// <summary>
    /// 删除表格
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="callback"></param>
    /// <remarks>
    /// * 可以使用
    /// </remarks>
    public void ExecuteDropTableAsync02(string tableName, Action<object[]> callback)
    {

        lock (readQueue)
        {
            writeQueue.Enqueue(() =>
            {

                object[] res = new object[] { 0 };
                try
                {
                    string dropSQL = $"DROP TABLE IF EXISTS {tableName};";
                    using (SqliteCommand dropCommand = new SqliteCommand(dropSQL, dbConnection))
                    {
                        dropCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行删除失败: " + e.Message + $" tableName: {tableName}");
                    res = new object[] { 1, e.Message };
                    // throw e;
                }
                finally
                {
                    EnqueueToMainThread(() => callback?.Invoke(res));
                }
            });
        }
    }


    /// <summary>
    /// 执行查询操作（读操作）
    /// </summary>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    public void ExecuteTotalCount(string query, Dictionary<string, object> parameters, Action<long> callback)
    {
        if (callback == null) return;

        lock (readQueue)
        {
            readQueue.Enqueue(() =>
            {
                try
                {
                    using (SqliteCommand command = new SqliteCommand(query, dbConnection))
                    {
                        if (parameters != null)
                            foreach (KeyValuePair<string, object> kv in parameters)
                            {
                                command.Parameters.AddWithValue(kv.Key, kv.Value);
                            }

                        long totalCount = 0;
                        // 执行查询并获取结果
                        object result = command.ExecuteScalar();
                        // 处理结果
                        if (result != null && result != DBNull.Value)
                        {
                            totalCount = Convert.ToInt64(result);
                        }

                        EnqueueToMainThread(() => callback(totalCount));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("执行查询失败: " + e.Message);
                }

            });
        }
    }












    // 将操作加入主线程队列
    private void EnqueueToMainThread(Action action)
    {
        lock (mainThreadQueue)
        {
            mainThreadQueue.Enqueue(action);
        }
    }

    // 在主线程更新
    void Update()
    {
        if (mainThreadQueue.Count > 0)
        {
            Action action;
            lock (mainThreadQueue)
            {
                action = mainThreadQueue.Dequeue();
            }
            action.Invoke();
        }
    }

    // 关闭数据库
    public void CloseDatabase()
    {
        try
        {
            isRunning = false;
            if (dbThread != null && dbThread.IsAlive)
            {
                dbThread.Abort();
                dbThread = null;
            }

            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection = null;
            }


            lock (writeQueue)
            {
                if (writeQueue.Count > 0)
                    Debug.LogError($"writeQueue.Count= {writeQueue.Count}");
            }
            lock (readQueue)
            {
                if (readQueue.Count > 0)
                    Debug.LogError($"readQueue.Count= {readQueue.Count}");
            }

            Debug.LogWarning("已关闭数据库 ");
        }
        catch (Exception e)
        {
            Debug.LogError("关闭数据库失败: " + e.Message);
        }
    }

    protected override void OnDestroy()
    {
        Debug.LogWarning("关闭数据库 OnDestroy");
        CloseDatabase();
        base.OnDestroy();
    }

}




public partial class SQLiteAsyncHelper : MonoSingleton<SQLiteAsyncHelper>
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
            }
            else if (finfo.FieldType == typeof(int) || finfo.FieldType == typeof(long))
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





    public static string SQLCreateTableIfNotExists<T>(string tableName)
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
            }
            else if (finfo.FieldType == typeof(int) || finfo.FieldType == typeof(long))
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
        cmdSrt.Append($"CREATE TABLE IF NOT EXISTS {tableName}(id INTEGER PRIMARY KEY,");
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
    public static string SQLInsertTableData(string tableName, Dictionary<string, object> dicKeyValue)
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

            if (finfo.Name == "id")
            {
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

        for (int i = 0; i < keys.Count; i++)
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



public partial class SQLiteAsyncHelper : MonoSingleton<SQLiteAsyncHelper>
{
    public void CheckTableExistsAsync(string tableName, Action<bool> callback)
    {
        lock (writeQueue)
        {
            writeQueue.Enqueue(() =>
            {
                string sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type ='table' and name='{tableName}';";
                try
                {
                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = sql;
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        bool isExists = count == 1;
                        EnqueueToMainThread(() => callback?.Invoke(isExists));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"执行非查询失败, error: {e.Message}  sql: {sql}");
                    EnqueueToMainThread(() => callback?.Invoke(false));
                }


            });
        }
    }




    /// <summary>
    /// 数据库数据转json
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="dbName"></param>
    /// <returns></returns>
    public void ConvertSqliteToJsonAsync(string sql, Action<string> callback)
    {
        if (!sql.Contains("SELECT"))
        {
            DebugUtils.LogError($"必须是 SELECT 业务");
            return;
        }


        lock (writeQueue)
        {
            writeQueue.Enqueue(() =>
            {

                var dataTable = new DataTable();
                using (SqliteCommand cmd = new SqliteCommand(sql, dbConnection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
                string res = JsonConvert.SerializeObject(dataTable, Formatting.Indented);

                EnqueueToMainThread(() => callback?.Invoke(res));
            });

        }
    }
}