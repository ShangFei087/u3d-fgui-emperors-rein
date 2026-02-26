using GameMaker;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

public class TableJackpotRecordAsyncManager : MonoSingleton<TableJackpotRecordAsyncManager>
{

    [Button]
    public void AddJackpotRecord(int jpLevel, string jpName, long winCredit, long creditBefore, long creditAfter, string gameUID = "-1", long? createdAt = null)
    {
        if (createdAt == null)
        {
            createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        string insertQuery = @"
                INSERT INTO jackpot_record (
                    created_at,
                    user_id,
                    game_id,
                    game_uid,
                    jp_name,
                    jp_level,
                    win_credit,
                    credit_before,
                    credit_after
                ) VALUES (
                    :createdAt,
                    :userID,
                    :gameID,
                    :gameUID,
                    :jpName,
                    :jplevel,
                    :winCredit,
                    :creditBefore,
                    :creditAfter
                )";

        SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(insertQuery, new Dictionary<string, object>()
        {
            [":createdAt"] = createdAt,
            [":userID"] = 0,
            [":gameID"] = MainModel.Instance.gameID,
            [":gameUID"] = gameUID,
            [":jpName"] = jpName,
            [":jpLevel"] = jpLevel,
            [":winCredit"] = winCredit,
            [":creditBefore"] = creditBefore,
            [":creditAfter"] = creditAfter,
        });
    }
}

