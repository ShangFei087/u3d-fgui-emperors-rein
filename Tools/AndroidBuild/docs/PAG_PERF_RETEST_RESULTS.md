# PAG 性能复测执行记录

> 已同步至 [`PAG_PERF_RETEST.md`](PAG_PERF_RETEST.md)。

## 单路（2026-07）— 通过

| Scenario | FPS | 帧时间 | Pass |
|----------|-----|--------|------|
| PAG1 Fade | 30 | 29~30ms | 通过 |
| PAG2 BigWin_1024 | 30 | ~30ms | 通过 |
| PAG3 XingXing2 | 30 | 29~30ms | 通过 |
| PAG4 六段 | 30 | ~29ms | 通过 |

## 双路同屏（修复前基线）

| 组合 | FPS | 备注 | ≥25 |
|------|-----|------|-----|
| PAG1+PAG2 | 20（30→15 渐降） | Profiler: CPU 33→69ms，GPU 20→25ms | 否 |
| PAG1+PAG3 | 3 | 严重 stall 疑似 | 否 |
| PAG1+PAG5 | 28 | — | 是 |
| PAG5+PAG2 | 29 | — | 是 |
| PAG7+PAG2 | 26 | — | 是 |

## 代码修复（2026-07 hotfix）

| 文件 | 改动 |
|------|------|
| `PagGpuSyncGroup.cs` | 部分 batch flush；移除「partial pending」stall 判定 |
| `PageGameMain.cs` | 同屏时跳过 per-instance `TryAlignPagTestFpsAfterPlayStarted` |

## 双路复测（修复后 — 待填）

| 组合 | 修复后 FPS | T0→T30 CPU | RecoverFromStall 30s | Pass |
|------|-----------|-------------|----------------------|------|
| PAG1+PAG2 | _待填_ | _待填_ | _待填_ | ≥25，无渐降 |
| PAG1+PAG3 | _待填_ | _待填_ | _待填_ | ≥25 |

```powershell
cd Tools\AndroidBuild
.\capture_pag_dual_logcat.ps1 -Combo PAG1_PAG2 -DurationSec 30
.\capture_pag_dual_logcat.ps1 -Combo PAG1_PAG3 -DurationSec 30
```

## 决策

| 项 | 状态 |
|----|------|
| 单路 GL 链路 | 冻结 P0 |
| 双路 partial flush 修复 | **已提交 hotfix，待真机复测** |
| P1 glFenceSync | 跳过 |
