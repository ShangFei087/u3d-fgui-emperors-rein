# Unity Android 导出：进度条不动 & 打包路径选择

> 脚本目录：`Tools/AndroidBuild/`（`Tools\` 根目录有同名快捷入口）  
> 完整说明见 [`../README.md`](../README.md)

## 为何进度条长时间不动

本工程 **Il2CPP + HybridCLR + StreamingAssets ~500MB+**，Export 时进度条停在 `0%`/`50%` **5～20 分钟** 很常见，尤其在：

- Il2CPP 生成（CPU 高、进度条常不刷新）
- 拷贝 StreamingAssets 到 `ExportProject`
- 写 Gradle 工程

**合计 often 15～30 分钟**。超过 30 分钟且下面诊断无活动 → 按「真卡住」处理。

## 导出时并行开两个窗口

```bat
REM 窗口1：Unity 里点 Export Project

REM 窗口2：盯 Editor.log
Tools\watch_unity_editor_log.bat nopause

REM 或每 30 秒看导出目录是否在长大
Tools\check_export_progress.bat watch
```

日志文件：

| 脚本 | 日志 |
|------|------|
| `watch_unity_editor_log.bat` | `Tools\AndroidBuild\logs\watch_unity_editor_log.log` |
| `check_export_progress.bat` | `Tools\AndroidBuild\logs\check_export_progress.log` |

### 真卡住 vs 只是慢

| 信号 | 只是慢 | 真卡住 |
|------|--------|--------|
| Editor.log | 持续有新行（Il2CPP/Copying/Export） | **10～15 分钟** 完全无新行 |
| 任务管理器 | Unity CPU 高或磁盘写入 | CPU≈0 且磁盘无写入 |
| ExportProject | `assets`/`Il2CppOutputProject` 修改时间在变 | 长时间不变 |

### 真卡住时

1. 关 Android Studio、关掉对 `TheOutput\ExportProject` 的预览
2. 工程目录加入杀毒排除项
3. 确认 `F:` 剩余空间 >5GB
4. 重启 Unity，只勾 **Export Project**（不要 Build And Run），重试

---

## 减少全量 Export（按改动类型）

| 改了什么 | 是否需要 Unity Export | 推荐命令 |
|----------|----------------------|----------|
| 仅 Hotfix C# | **不必** 每次全量 Export | Unity：`HybridCLR` + `NewBuild/打包1001` → `build_android_debug.bat hotfix` |
| Hotfix + 要更新 Il2Cpp/AOT | **需要** Export | Export → `build_android_debug.bat` |
| 仅 pagBridge Java/so | **不需要** Export | `build_android_debug.bat skipcopy` |
| 主工程/大资源 | **需要** Export | Export → `build_android_debug.bat` |

`build_android_debug.bat` 已内置拷贝步骤（full / hotfix / skipcopy），无需再单独跑 copy 脚本。

### 仅热更快捷路径

```bat
REM Unity 内：HybridCLR 编译 + NewBuild/打包1001

Tools\build_android_debug.bat hotfix nopause
```

hotfix 模式优先从 `ExportProject/.../assets` 拷贝，否则从 `Assets/StreamingAssets`。

---

## 全量打包（标准）

```bat
REM Unity：Export Project -> TheOutput\ExportProject

Tools\build_android_debug.bat nopause
```

APK：`TheOutput\TargetProject\launcher\build\outputs\apk\debug\launcher-debug.apk`
