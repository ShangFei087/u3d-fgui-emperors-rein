# PAG 性能复测清单（C1~C3 优化后）

> 基线数据见 [`PAG_PROFILER_BASELINE.md`](PAG_PROFILER_BASELINE.md)。  
> 玩法验收见 [`RETEST_PAG_UFO.md`](RETEST_PAG_UFO.md)。

## 构建

```text
1. Unity: HybridCLR + NewBuild/打包1001
2. Tools\build_pag_unity_gl_bridge.bat nopause   ← C2 改 .cpp 后必跑
3. Tools\build_android_debug.bat hotfix nopause
```

Development Build + Autoconnect Profiler 便于对照 Timeline 中 `PAG.*` 标记。

## 四项 FPS 指标（每项预热 5s + 采样 30s）

在 **PageGameMain** 使用 PAG1~PAG4 按钮，记录屏幕 FPS（或 Profiler `FPS` / `GCMonitorPro` 显示值）。

| 用例 | 资源 | 优化前 FPS | 优化前帧时间 | 优化后 FPS | 优化后帧时间 | 通过 |
|------|------|-----------|-------------|-----------|-------------|------|
| PAG1 单路 | Neza.pag | 36~37 | ~27.6ms | _待填_ | _待填_ | ≥45 FPS |
| **PAG2 单路** | Transition.pag | **36~37** | **~27.6ms** | _待填_ | _待填_ | **≥45 FPS** |
| PAG3 单路 | FeiZhou.pag | 33 | ~30.3ms | _待填_ | _待填_ | ≥40 FPS |
| PAG4 三实例 | CaiHongFeiDie.pag ×3 | 31 | ~32.3ms | _待填_ | _待填_ | ≥38 FPS |

### 预期收益（C1+C2+C3）

| 优化项 | 预期 |
|--------|------|
| C1 减 WEOF（3→1 每帧） | -3~6ms CPU |
| C2 flush 不再双 glFinish | -2~4ms GPU 阻塞 |
| C3 合并 Present JNI + Handler 直 post | -1~3ms |

## Profiler 对比（PAG2 30s）

| 标记 / 采样 | 优化前 | 优化后 |
|-------------|--------|--------|
| `WaitForEndOfFrame` 次数/帧 | ~3 | **~1** |
| `PAG.WaitRenderIdle` | 含 2× WEOF + FinishFrame | **1× WEOF，flush 无 FinishFrame** |
| `PAG.RequestNextFrame` | 含开头 WEOF | **无 WEOF** |
| 渲染线程 `glFinish`/帧 | ~2 | **~1** |
| JNI `OnPagGpuFrameReady`/帧 | 1 | **0**（改 Unity 本地 + `OnGpuFlushCompleted`） |

## logcat 自动化

```powershell
cd Tools\AndroidBuild
.\capture_pag_frame_timing.ps1 -Scenario PAG2 -DurationSec 30
.\capture_pag_frame_timing.ps1 -Scenario PAG4 -DurationSec 30
```

确认无 GL 队列积压：Unity log 无 `GlQueueBacklogWarnThreshold` 告警（≥32）。

## RETEST_PAG_UFO 验收

按 [`RETEST_PAG_UFO.md`](RETEST_PAG_UFO.md) 完整跑一遍，重点：

| 检查项 | 期望 |
|--------|------|
| 无花屏/闪屏 | 通过 |
| Dragon / UFO 各一次 `notifyPlaybackFinished` | 通过 |
| 无 `Fatal signal` / `FATAL EXCEPTION` | 通过 |
| PAG4 三实例同步播放 | 无撕裂、帧率可接受 |

## 回退

- C1/C3 C#：还原 `PagUnityGlBridge.cs`、`PagController.cs`、`PagGpuSyncGroup.cs`、`PagCallbackHub.cs`
- C2/C3 Native：还原 `PagUnityGlBridge.cpp`、`PagBridge.java`、`PagOverlayManager.java`，重编 `libpag_unity_gl_bridge.so`
