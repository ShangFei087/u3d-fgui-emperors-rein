-- 机台 SQLite 路径（按运行环境）：
--   Unity Editor：通常与 Assets/StreamingAssets 下 ApplicationSettings.dbName 一致（如 Games.db）
--   Android：Application.persistentDataPath/<dbName>
-- 表名：slot_game_record（见 ConsoleDataType.cs / ConsoleTableName.TABLE_SLOT_GAME_RECORD）

-- 1) 看表结构（若 sqlite3 支持）
-- .schema slot_game_record

-- 2) 抽查几条 3997，对照导出 Excel 列含义（见 slot_game_record_export.py 文档）
SELECT
  id,
  game_id,
  total_bet,
  credit_before,
  credit_after,
  base_game_win_credit,
  jackpot_win_credit,
  open_type,
  result_type,
  free_curtime,
  free_totaltime,
  (credit_after - credit_before) AS delta_credit,
  (base_game_win_credit + jackpot_win_credit - total_bet) AS delta_from_parts,
  datetime(created_at / 1000, 'unixepoch', 'localtime') AS created_local
FROM slot_game_record
WHERE game_id = 3997
ORDER BY id ASC
LIMIT 10;

-- 4) 与 Tools/slot_export/slot_game_record_export.py、SlotGameRecordExport（Unity 测试菜单导出）列口径一致时，
--    核对 (credit_after - credit_before) 与 (base_game_win_credit + jackpot_win_credit - total_bet)
