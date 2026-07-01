# PAG Dragon → UFO 复测清单（纹理模式）

## 纹理模式（默认开启）

- `TurnTablePagUseFguiTexture = true`：PAG 经 **ExternalTexture** 显示在 `anchorTurnTable/pagEffect`（须在 FGUI 预置，层级由编辑器兄弟顺序决定）
- **已删除** CPU ARGB / `ConsumeFguiFrameArgb` / `LoadRawTextureData` 通路
- 回退浮层模式：`TurnTablePagUseFguiTexture = false`（可选 `TurnTablePagOverlayFallback = true` 走 ImageView 软件出帧）
- 离屏：`TurnTablePagFguiMaxDisplaySide = 0`（0 = 原画质，不限制长边）
- 显示：`pagEffect` 按 PAG **合成原尺寸**（如 720×1281）+ `ScaleFree` 铺满
- 帧率：Unity 侧刷新节流 **60fps**（`TurnTablePagFguiFps`）；Native progress 按 **`elapsed / durationUs` 时间轴**计算（与 Bitmap 回退一致）
- 播完：协程等待 `OnPagPlaybackFinished`，不再固定 8 秒

## FGUI 发布（改 pagEffect 后必做）

1. 用 FairyGUI 打开 `TheSourceFile/fgui-zhu-zai-jin-bi/SlotZhuZaiJinBi1700`
2. 确认 `AnchorTurnTable` 含 `pagEffect` GLoader（源文件：`components/gameMain/AnchorTurnTable.xml`）
3. Publish 到 `Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/FGUIs`

## 安装与构建

详见 [`EXPORT_ANDROID.md`](EXPORT_ANDROID.md)（含 Export 进度诊断、仅热更快捷路径）。  
脚本说明见 [`../README.md`](../README.md)。

```text
1. Unity: HybridCLR + NewBuild/打包1001
2. Tools\build_android_debug.bat hotfix nopause
APK: TheOutput\TargetProject\launcher\build\outputs\apk\debug\launcher-debug.apk
```

Unity Export 进度条不动时：`Tools\watch_unity_editor_log.bat`、`Tools\check_export_progress.bat watch`

## 抓 log（多设备）

```bat
Tools\watch_pag_logcat.bat nopause    REM 实时；日志 AndroidBuild\logs\watch_pag_logcat.log
Tools\dump_pag_logcat.bat nopause     REM 快照；logs\pag_logcat_dump.txt + logs\dump_pag_logcat.log
```

```powershell
cd Tools\AndroidBuild
.\capture_android_logcat.ps1 -ClearFirst
.\capture_android_logcat.ps1 -Serial <设备序列号>
```

关注 tag：`PagOverlayManager`、`PagUnityGlBridge`、`PagBridgeUnity`

## 脚本日志文件

| 脚本 | 日志 |
|------|------|
| `build_pag_unity_gl_bridge.bat` | `AndroidBuild\logs\build_pag_unity_gl_bridge.log` |
| `watch_pag_logcat.bat` | `AndroidBuild\logs\watch_pag_logcat.log` |
| `dump_pag_logcat.bat` | `AndroidBuild\logs\dump_pag_logcat.log`（含快照副本） |

## 通过标准

| 检查项 | 期望 |
|--------|------|
| `startFguiGpuPlayback: tex=<非0> WxH nativeFrames=XX`（勿出现 `tex=0 0x0`） | 应有 |
| `SetFguiFrameConfig ... fps=60` | 应有 |
| `requestGpuRenderFrame: progress=0.xxx` 单调递增至 1.0 | 应有 |
| `setupGpuSurfaceOnRenderThread: tex=<非0> WxH` | 应有（渲染线程 setup） |
| `requestGpuRenderFrame` / `OnPagGpuRenderFrame` | 应有 |
| `bindGpuTexture` | 应有 |
| 无 `Fatal signal` / `beginning of crash` | 不应出现 |
| `notifyPlaybackFinished` Dragon/UFO 各一次 | 应有 |
| 无 `publishFguiFrameToUnity` / `ConsumeFguiFrameArgb` | 不应出现 |
| `composition display size 720x1281` | 应有 |
| `resolveFguiTextureDimensions: composition=WxH render=WxH maxSideCap=0`（render 与 composition 一致） | 应有 |
| 画面铺满 pagEffect，层级符合 FGUI 中 pagEffect 与 holder 顺序 | 目视 |
| 删除 pagEffect 后播放 | `[PAG FGUI] 锚点 ... 缺少 GLoader「pagEffect」`，不崩溃 |
| 浮层模式回退 `UseFguiTexture=false` | WM 全屏仍可播 |
| 无 `FATAL EXCEPTION` | 不应出现 |
| `[1700 PAG] PlayTurnTableEnterSequence finished` | 应有 |

## ARMv7

- `pagBridge.androidlib` NDK 仅 `armeabi-v7a`，含 `libpag_unity_gl_bridge.so`
