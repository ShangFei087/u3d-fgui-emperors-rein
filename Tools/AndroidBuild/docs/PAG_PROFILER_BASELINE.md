# PAG 纹理模式性能 Profiler 基线

> C1~C3 优化后复测见 [`PAG_PERF_RETEST.md`](PAG_PERF_RETEST.md)。  
> 本文含 **Phase 0 历史数据**（旧测试资源）与 **当前代码对齐说明**。

## 当前环境（与 PageGameMain 对齐）

| 项 | 值 |
|----|-----|
| 场景 | 1700 `PageGameMain`，PAG1~4 测试按钮 |
| 主用例 | **PAG2 单路** → `PagTestName1` = **BigWin_1024.pag**，repeat=-1 |
| 时长 | 预热 5s + 采样 **30s** |
| Build | Development Build + **Autoconnect Profiler** |
| 帧率节流 | **`PagTestFguiFps = 30`** → `PagGpuSyncGroup` / `_fguiTargetFrameInterval` |
| 显示 | `PagTestUseFguiTexture = true`，`PagTestFguiMaxDisplaySide = 0` |
| 合组 | `PagConcurrentPlayback.Enabled = true`（单路 Play 也会 TryJoin） |

### 按钮 → 资源（代码常量）

| 按钮 | 文件常量 | .pag |
|------|----------|------|
| PAG1 | `PagTestName2` | Fade.pag |
| PAG2 | `PagTestName1` | BigWin_1024.pag |
| PAG3 | `PagTestName3` | XingXing2.pag |
| PAG4 | `PagTestBigWinSequence` | bigwin_start → … → megawin_idle（单槽六段） |

代码位置：[`PageGameMain.cs`](../../../Assets/HotFix/Games/Slot Zhu Zai Jin Bi 1700/PageGameMain.cs) L192-208、L231-235。

---

## Phase 0 历史 FPS（旧资源，仅供参考）

> **勿与现用例直接对比。** 当时 PAG2 为 Transition.pag，PAG4 为 CaiHongFeiDie×3 三实例。

| 场景 | FPS | Δ帧时间 vs 基线 | 说明 |
|------|-----|-----------------|------|
| 基线（无 PAG） | 59 | — | — |
| PAG1 单路（Neza） | 36~37 | +10ms | 固定管线开销 |
| **PAG2 单路（Transition）** | **36~37** | **+10ms** | Phase 0 主采样 |
| PAG3（FeiZhou） | 33 | +13.5ms | 固定 + 大资源 |
| PAG4 三实例（CaiHongFeiDie×3） | 31 | +14.5ms | 固定 + 多实例 |

现用例需在真机**重新采基线**，填入 [`PAG_PERF_RETEST.md`](PAG_PERF_RETEST.md)「现用例 FPS」列。

---

## Unity Android Profiler（Timeline）

1. Build & Run Development APK，Profiler 连接真机。
2. 进入 PageGameMain，点 **PAG2**（BigWin_1024），等 5s 预热。
3. Record **30s**，Hierarchy 筛选：

| 采样目标 | 位置 | C1~C3 前（估） | C1~C3 后（代码目标） |
|----------|------|----------------|---------------------|
| `WaitForEndOfFrame` | 主线程 | 每帧 ~3 次 | **~1 次/flush** |
| `PAG.WaitRenderIdle` | 主线程 | 含 FinishFrame | flush 路径无 FinishFrame |
| `PAG.GlFlushBatch` | 主线程 | 有 | 有 |
| `PAG.GlSetupBatch` | 首帧/setup | 有 | 有 |
| `PAG.GpuRenderFrame` | Java 要帧回调 | — | 有 |
| `PAG.GpuFrameReady` | present 后 | — | 有 |
| ~~`PAG.RequestNextFrame`~~ | — | 有（已删） | **不存在** |
| `AndroidJavaObject.CallStatic` | `RequestNextGpuFrame` | 中 | SyncGroup 节流后仍每帧 1 次 |
| 渲染线程 `glFinish` | Flush + FinishFrame | ~2/帧 | **~1/帧**（batch 末尾） |

**记录项：** 主线程 PAG 相关 ms/帧、渲染线程 glFinish 次数/帧、JNI CallStatic 次数/帧。

> **Profiler 标记说明：** Timeline 中 `PAG.GlFlushBatch`、`PAG.GpuRenderFrame` 等标记名保留历史命名，均指 **纹理模式** 通路（见本文「术语」[`PAG_MAINTENANCE_PRIORITY.md`](PAG_MAINTENANCE_PRIORITY.md)）。

---

## 每帧管线（当前代码）

```text
Java requestGpuRenderFrame (progress)
  → Unity PagGpuSyncGroup / PagController HandleGpuRenderFrame
  → PagUnityGlBridge IssueFlushPagGpuBatch
  → [渲染线程] flushGpuFrameOnRenderThread × N → glFinish ×1
  → Unity NotifyGpuFrameReadyAfterFlush / OnGpuFramePresented
  → PagFguiGpuPresenter.OnGpuFrameReady
  → OnGpuFlushCompleted (JNI) → Java onGpuFrameFlushed
  → PagGpuSyncGroup AdvanceGroupFrame (WaitForSecondsRealtime 30fps)
  → RequestNextGpuFrame
```

---

## logcat 帧耗时

```bat
Tools\watch_pag_logcat.bat nopause
```

或：

```powershell
cd Tools\AndroidBuild
.\capture_android_logcat.ps1 -ClearFirst
# 播放 PAG2 30s 后 Ctrl+C
```

关注 tag：`PagOverlayManager`、`PagUnityGlBridge`、`PagBridgeUnity`

| 阶段 | tag / 关键字 |
|------|----------------|
| 请求渲染 | `requestGpuRenderFrame: progress=` |
| 渲染线程 flush | `OnRenderEvent: flush batch` / `flushGpuFrameOnRenderThread` |
| Present / 续播 | `onGpuFrameFlushed` → Unity `OnGpuFlushCompleted` |
| 下一帧（SyncGroup） | `RequestNextGpuFrame` |
| 段切 / 播完 | `fguiGpuTickPhase` / `notifyPlaybackFinished` |

> 旧关键字 `deliverGpuFramePresented`、`OnPagGpuFrameReady` 已废弃。

---

## 自动化帧耗时脚本

```powershell
cd Tools\AndroidBuild
.\capture_pag_frame_timing.ps1 -Scenario PAG2 -DurationSec 30
```

输出：`AndroidBuild\logs\pag_frame_timing_PAG2_<timestamp>.txt`

---

## 固定开销结论

### C1~C3 优化前（Phase 0，代码级）

每帧纹理模式通路至少：**3× WaitForEndOfFrame + 2× glFinish**（Flush 内 1× + FinishFrame 1×）。PAG1/PAG2 FPS 相近 → 成本主要在框架管线，与 .pag 内容关系不大。

### C1~C3 优化后（当前代码目标）

每 flush：**1× WaitForEndOfFrame + 1× glFinish**；Present 不经 Java `OnPagGpuFrameReady`；SyncGroup 用 `WaitForSecondsRealtime` 做 30fps 节流（非 WEOF）。

真机是否达标 → **2026-07 已验收**（见 [`PAG_PERF_RETEST.md`](PAG_PERF_RETEST.md)）。

---

## 2026-07 现用例实测（PagTest 测试区）

| 用例 | 资源 | FPS | 说明 |
|------|------|-----|------|
| PAG2 单路 | BigWin_1024.pag | **30** | 与 `PagTestFguiFps=30` 一致；功能正常 |
| PAG1 / PAG3 / PAG4 | 见 PAG_PERF_RETEST | **~30** | 单路 + 六段连播无闪屏/段切黑帧 |

现行通过标准（30fps 配置）→ [`PAG_PERF_RETEST.md`](PAG_PERF_RETEST.md)「通过标准说明」。

---

## 配置微调参考（复测未达标时）

| 参数 | 默认 | 说明 |
|------|------|------|
| `PagTestFguiFps` | 30 | 降至 24 可减 glow 循环（PAG5~9）开销 |
| `PagTestFguiMaxDisplaySide` | 0 | 大资源掉帧时可试 720 / 512 |
| `PagConcurrentPlayback.Enabled` | true | 仅多实例同屏必需；单路 A/B 可评估延迟 TryJoin |

TurnTable 正式接入时 fps 按场景单独配置（转盘 vs 背景），勿与测试区 30fps 混用。
