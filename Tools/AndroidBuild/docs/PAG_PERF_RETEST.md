# PAG 性能复测清单（C1~C3 已落地）

> 框架基线见 [`PAG_PROFILER_BASELINE.md`](PAG_PROFILER_BASELINE.md)。  
> 转盘玩法验收（Dragon/UFO）见 [`RETEST_PAG_UFO.md`](RETEST_PAG_UFO.md)——与本文性能复测**分开执行**。  
> 填表记录见 [`PAG_PERF_RETEST_RESULTS.md`](PAG_PERF_RETEST_RESULTS.md)。

## 代码对齐说明（PageGameMain 测试区）

用例以 [`PageGameMain.cs`](../../../Assets/HotFix/Games/Slot Zhu Zai Jin Bi 1700/PageGameMain.cs) 中 `PagTestName*` / `PagTestBigWinSequence` 为准：

| 按钮 | 常量 / 资源 | 槽位 | 模式 |
|------|-------------|------|------|
| PAG1 | `PagTestName2` → **Fade.pag** | pagEffect1 | repeat=-1 循环 |
| PAG2 | `PagTestName1` → **BigWin_1024.pag** | pagEffect2 | repeat=-1 循环 |
| PAG3 | `PagTestName3` → **XingXing2.pag** | pagEffect3 | repeat=-1 循环 |
| PAG4 | `PagTestBigWinSequence` 六段 | pagEffect4 | Native 播放列表，各 repeat=1，播完结束 |

纹理模式配置（同文件）：

- `PagTestUseFguiTexture = true`
- `PagTestFguiMaxDisplaySide = 0`（原画）
- `PagTestFguiFps = 30`（SyncGroup 节流；非 TurnTable 文档中的 60fps）
- `PagConcurrentPlayback.Enabled` 随 FguiTexture 开启（单路也会 TryJoin 动态合组）

> **历史基线作废：** 旧文档中的 Neza / Transition / FeiZhou / CaiHongFeiDie×3 与当前按钮行为不一致；下表「旧框架基线」仅作 C1~C3 优化前**管线**参考，不可与现用例 FPS 直接对比。

## 构建

```text
1. Unity: HybridCLR + NewBuild/打包1001
2. Tools\build_pag_unity_gl_bridge.bat nopause   ← 改 PagUnityGlBridge.cpp 后必跑
3. Tools\build_android_debug.bat hotfix nopause
```

Development Build + Autoconnect Profiler 便于对照 Timeline 中 `PAG.*` 标记。

## 通过标准说明（2026-07 定稿）

测试区配置 `PagTestFguiFps = 30`，屏幕 FPS 稳定在 **30±2** 为**配置预期**，不要求 Phase 0 时代的 ≥45 FPS。

| 层级 | 标准 |
|------|------|
| **功能层（必过）** | 无花屏/闪屏；PAG4 段切无黑帧；无 crash；无 `GlQueueBacklogWarnThreshold` 告警 |
| **帧率层（单路 30fps）** | 屏幕 FPS ≈ `PagTestFguiFps`（**30±2**） |
| **帧率层（双路同屏）** | **≥25 FPS**；30s 内跌幅 **≤5**（无渐降）；CPU 主线程末段相对 T0 增幅 **≤10ms** |
| **可选进阶** | TurnTable `TurnTablePagFguiFps=60` 场景另测，见 [`RETEST_PAG_UFO.md`](RETEST_PAG_UFO.md) |

## FPS 指标（每项预热 5s + 采样 30s）

在 **PageGameMain** 点对应按钮，记录屏幕 FPS（或 Profiler `FPS` / `GCMonitorPro`）。

| 用例 | 资源 | 旧框架基线 FPS¹ | 现用例 FPS | 现用例帧时间 | 通过 |
|------|------|-----------------|-----------|-------------|------|
| PAG1 单路 | Fade.pag | 36~37 | **30** | **29~30ms** | **通过**（~30 FPS，无闪屏） |
| **PAG2 单路** | **BigWin_1024.pag** | **36~37** | **30** | **~30ms** | **通过**（~30 FPS，无闪屏） |
| PAG3 单路 | XingXing2.pag | 33² | **30** | **29~30ms** | **通过**（~30 FPS，无闪屏） |
| PAG4 六段连播 | BigWin 六段（单槽） | —³ | **30** | **~29ms** | **通过**（段切无黑帧；steady ~30 FPS） |

¹ 旧基线：Phase 0 在 **Transition.pag** 等旧资源 + 旧 PAG4 三实例下采集，见 [`PAG_PROFILER_BASELINE.md`](PAG_PROFILER_BASELINE.md) 历史表；**不可与现用例 FPS 直接对比**。  
² 旧 PAG3 为 FeiZhou.pag，资源更重。  
³ PAG4 语义：Phase4E 段切换 + chain，非三实例 batch flush。

**PAG4 采样说明：** 六段播完会结束；30s 采样请在播放进行中操作——段内 steady（如 bigwin_idle）记 FPS，或播完后再次点击 PAG4 重复。另记录段切换瞬间是否黑帧/闪屏。

### 双路同屏矩阵（2026-07，修复前基线）

操作：**先点左路 → 预热 5s → 再点右路 → 采样 30s**。

| 组合 | 资源 | 修复前 FPS | Profiler 备注 | 通过（≥25） |
|------|------|-----------|---------------|-------------|
| PAG1+PAG2 | Fade + BigWin_1024 | **20**（30→15 渐降） | T0 CPU33 GPU20 → T30 CPU69 GPU25 | **待复测** |
| PAG1+PAG3 | Fade + XingXing2 | **3** | SyncGroup stall 疑似 | **待复测** |
| PAG1+PAG5 | Fade + glow_loop_720 | 28 | — | 通过 |
| PAG5+PAG2 | glow_loop_720 + BigWin_1024 | 29 | — | 通过 |
| PAG7+PAG2 | glow_half_1920 + BigWin_1024 | 26 | — | 通过 |

**修复（2026-07）：**

- [`PagGpuSyncGroup.cs`](../../../Assets/HotFix/Games/_Common/Anim UI/PagGpuSyncGroup.cs) — 部分 batch flush（有 pending 即 flush，不再等全员同帧 pending）
- [`PageGameMain.cs`](../../../Assets/HotFix/Games/Slot Zhu Zai Jin Bi 1700/PageGameMain.cs) — `PagConcurrentPlayback.Enabled` 时跳过 `TryAlignPagTestFpsAfterPlayStarted`  per-instance fps

修复后请复测上表「待复测」两行并填入「修复后 FPS」列。

### 可选：三单路并发

| 用例 | 操作 | FPS | 帧时间 | 通过 |
|------|------|-----|--------|------|
| 三单路并发（可选） | PAG1+PAG2+PAG3 同开 | _待填_ | _待填_ | ≥25，无 tearing |

```powershell
cd Tools\AndroidBuild
.\capture_pag_dual_logcat.ps1 -Combo PAG1_PAG2 -DurationSec 30
```

### C1+C2+C3 预期收益（框架层，与 .pag 内容无关）

| 优化项 | 预期 |
|--------|------|
| C1 减 WEOF（3→1 每 flush） | -3~6ms CPU |
| C2 flush 不再双 glFinish | -2~4ms GPU 阻塞 |
| C3 Present 改 Unity 本地 + `OnGpuFlushCompleted` | -1~3ms |

## Profiler 对比（PAG2 单路 30s）

| 标记 / 采样 | C1~C3 优化前 | C1~C3 优化后（代码目标） | 复测实测 |
|-------------|--------------|-------------------------|----------|
| `WaitForEndOfFrame` 次数/帧 | ~3 | **~1**（仅 `PAG.WaitRenderIdle`） | _待填_ |
| `PAG.WaitRenderIdle` | 2× WEOF + FinishFrame | **1× WEOF，flush 无 FinishFrame** | _待填_ |
| `PAG.GlFlushBatch` | 有 | 有（flush 主路径） | _待填_ |
| `PAG.GpuRenderFrame` / `PAG.GpuFrameReady` | — | 有（Java 要帧 / present） | _待填_ |
| ~~`PAG.RequestNextFrame`~~ | 有（已删除） | **不存在** | — |
| 渲染线程 `glFinish`/帧 | ~2 | **~1**（batch 末尾一次） | _待填_ |
| JNI `OnPagGpuFrameReady`/帧 | 1 | **0**（`OnGpuFlushCompleted`） | _待填_ |
| `AndroidJavaObject.CallStatic` `RequestNextGpuFrame` | 每帧 | SyncGroup 节流后每帧 1 次 | _待填_ |

## logcat 自动化

```powershell
cd Tools\AndroidBuild
# 四项依次采集（每项前在真机点击对应按钮）
.\run_pag_perf_retest.ps1 -DurationSec 30

# 或单项
.\capture_pag_frame_timing.ps1 -Scenario PAG2 -DurationSec 30
.\capture_pag_frame_timing.ps1 -Scenario PAG4 -DurationSec 30
```

汇总输出：`AndroidBuild\logs\pag_perf_retest_summary_<timestamp>.md`（含 logcat 计数 + FPS 待填表）。

脚本只抓 log；**播放对应用例资源**（见上表）。确认无 GL 队列积压：Unity log 无 `GlQueueBacklogWarnThreshold` 告警（≥32）。

## 性能验收（本文范围）

| 检查项 | 期望 |
|--------|------|
| 无花屏/闪屏（PAG1~3 循环、PAG4 段切） | 通过 |
| 无 `Fatal signal` / `FATAL EXCEPTION` | 通过 |
| PAG4 六段 `notifyPlaybackFinished` 一次 | 通过 |
| `OnGpuFlushCompleted` 链路正常（无 `OnPagGpuFrameReady`） | 通过 |
| 上表 FPS 达到通过线（30±2） | **通过**（2026-07 复测） |
| 单路 + 六段连播功能正常 | **通过**（用户确认） |

玩法验收（Dragon/UFO、TurnTable）不在本文范围，见 [`RETEST_PAG_UFO.md`](RETEST_PAG_UFO.md)。

## 回退

- C1/C3 C#：还原 `PagUnityGlBridge.cs`、`PagController.cs`、`PagGpuSyncGroup.cs`、`PagCallbackHub.cs`
- C2/C3 Native：还原 `PagUnityGlBridge.cpp`、`PagBridge.java`、`PagOverlayManager.java`，重编 `libpag_unity_gl_bridge.so`

## 决策（2026-07 复测结论）

| 结果 | 动作 |
|------|------|
| **单路已验收** — PAG1~4 ~30 FPS | **冻结 GL 链路**（Native .cpp 不动） |
| **双路** PAG1+P2 / P1+P3 修复后 | 待 hotfix 复测 ≥25 FPS |
| P1 glFenceSync | **跳过** |
| 将来 TurnTable 60fps 或 Unity 主循环 45+ 诉求 | 单独复测后再评估 P1 |
