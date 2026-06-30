# PAG GPU 性能 Profiler 基线（Phase 0）

> 优化前采集；C1~C3 改动后对照 [`PAG_PERF_RETEST.md`](PAG_PERF_RETEST.md) 复测。

## 环境

| 项 | 值 |
|----|-----|
| 场景 | 1700 `PageGameMain`，PAG 测试按钮 |
| 用例 | **PAG2 单路**（`Transition.pag`，最小变量） |
| 时长 | 预热 5s + 采样 **30s** |
| Build | Development Build + **Autoconnect Profiler** |
| 帧率目标 | Native 30fps 节流（`TurnTablePagFguiFps` / `_fguiTargetFrameInterval`） |

## 优化前 FPS / 帧时间（已确认）

| 场景 | FPS | Δ帧时间 vs 基线 | 说明 |
|------|-----|-----------------|------|
| 基线（无 PAG） | 59 | — | — |
| PAG1 单路 | 36~37 | +10ms | 固定管线开销 |
| **PAG2 单路** | **36~37** | **+10ms** | **Phase 0 主采样用例** |
| PAG3 FeiZhou | 33 | +13.5ms | 固定 + 大资源 +3~4ms |
| PAG4 三实例 | 31 | +14.5ms | 固定 + 多实例 +4ms |

PAG2 单路帧时间约 **27.6ms**（1000/36）。

## Unity Android Profiler（Timeline）

1. Build & Run Development APK，Profiler 连接真机。
2. 进入 PageGameMain，点 **PAG2**，等 5s 预热。
3. Record **30s**，Hierarchy 筛选：

| 采样目标 | 预期位置 | 优化前占比（估） |
|----------|----------|------------------|
| `WaitForEndOfFrame` | 主线程，**每帧 ~3 次** | 高（~3~6ms/帧） |
| `PAG.WaitRenderIdle` / `PAG.GlFlushBatch` | Phase 0b 标记后可见 | 高 |
| `PAG.RequestNextFrame` | Present 后 RequestNext 协程 | 中（~2~3ms/帧） |
| `AndroidJavaObject.CallStatic` | `RequestNextGpuFrame` | 中（~0.5~2ms/帧） |
| `PlayerLoop` / `RenderPipelineManager` | IssuePluginEvent 前后 | 中 |
| 渲染线程 `libpag_unity_gl_bridge.so` / `glFinish` | Flush + FinishFrame | 高（~2~4ms/帧） |

**记录项：** 主线程 PAG 相关 ms/帧、渲染线程 glFinish 次数/帧、JNI CallStatic 次数/帧。

## logcat 帧耗时（零改动快速验证）

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

关键链路与时间戳差：

```text
requestGpuRenderFrame → (Unity flush) → flushGpuFrameOnRenderThread
  → deliverGpuFramePresented / onGpuFrameFlushed → RequestNextGpuFrame
```

| 阶段 | tag / 关键字 |
|------|----------------|
| 请求渲染 | `requestGpuRenderFrame: progress=` |
| 渲染线程 flush | `OnRenderEvent: flush batch` / `flushGpuFrameOnRenderThread` |
| Present | `deliverGpuFramePresented` / `onGpuFrameFlushed` |
| 下一帧 | `RequestNextGpuFrame` |

## Android Studio CPU Profiler（可选）

过滤：`PagUnityGlBridge`、`glFinish`、`libpag`、`flushGpuFrameOnRenderThread`

对比 **Render Thread / Unity Main / UI Thread** 等待时间。

## 自动化帧耗时脚本

```powershell
cd Tools\AndroidBuild
.\capture_pag_frame_timing.ps1 -Scenario PAG2 -DurationSec 30
```

输出：`AndroidBuild\logs\pag_frame_timing_PAG2_<timestamp>.txt`

## 固定开销结论（代码级，优化前）

每帧 PAG GPU 通路至少：**3× WaitForEndOfFrame + 2× glFinish**（Flush 内 1× + FinishFrame 1×），与 PAG1/PAG2 性能相同现象一致——框架固定成本，与 .pag 内容无关。
