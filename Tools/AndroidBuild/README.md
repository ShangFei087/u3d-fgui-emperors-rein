# AndroidBuild 工具说明

Unity Android 导出、Gradle 打 APK、PAG 原生桥接编译、Export 进度诊断、真机 logcat 调试等脚本均在本目录。

`Tools\` 根目录保留了同名**快捷入口**（转发到本目录），双击 `Tools\build_android_debug.bat` 与双击本目录内脚本效果相同。

**最终 APK：** `TheOutput\TargetProject\launcher\build\outputs\apk\debug\launcher-debug.apk`  
**运行日志：** `Tools\AndroidBuild\logs\`

---

## 目录结构

```
AndroidBuild/
├── README.md                          ← 本说明
├── build_android_debug.bat            ← 一键打包（主入口）
├── build_pag_unity_gl_bridge.bat      ← 编译 PAG JNI .so
├── copy_unity_export_to_target.bat    ← ExportProject → TargetProject（全量拷贝）
├── copy_hotfix_assets_to_target.bat   ← 仅拷贝热更 StreamingAssets
├── sync_pagbridge_to_target.bat       ← 同步 pagBridge.androidlib
├── clean_android_target.bat           ← 清理 Gradle build 缓存
├── check_export_progress.bat          ← Export 进度快照/轮询
├── check_export_progress.ps1          ← 统计导出目录大小（被 .bat 调用）
├── check_export_progress_editor_tail.ps1  ← 读取 Editor.log 末尾（被 .bat 调用）
├── watch_unity_editor_log.bat         ← 实时盯 Unity Editor.log
├── watch_pag_logcat.bat               ← 实时 PAG 相关 logcat
├── dump_pag_logcat.bat                ← 抓取 PAG logcat 快照
├── capture_android_logcat.ps1         ← 多设备 logcat 导出（闪退复现）
├── docs/
│   ├── EXPORT_ANDROID.md              ← Export 慢/卡住诊断、打包路径选择
│   ├── RETEST_PAG_UFO.md              ← PAG 转盘纹理模式复测清单
│   └── PAG_MAINTENANCE_PRIORITY.md    ← PAG 后续维护优先级
└── logs/                              ← 脚本运行日志（自动生成）
```

---

## 一键打包（推荐）

`build_android_debug.bat` 已串联完整流水线：

| 步骤 | 内容 |
|------|------|
| 0 | 拷贝（按模式：full / hotfix / skipcopy） |
| 1 | 编译 `libpag_unity_gl_bridge.so` |
| 2 | 同步 `pagBridge.androidlib` |
| 3 | 清理 TargetProject build 缓存 |
| 4 | `gradlew :launcher:assembleDebug` |
| 5 | 校验 APK |

### 用法

```bat
REM 全量 Export 后（默认，含 copy_unity_export）
Tools\build_android_debug.bat nopause

REM 仅热更（Unity: HybridCLR + NewBuild/打包1001）
Tools\build_android_debug.bat hotfix nopause

REM 仅改 pagBridge，跳过拷贝
Tools\build_android_debug.bat skipcopy nopause
```

### 按改动类型选模式

| 改了什么 | Unity 操作 | 命令 |
|----------|------------|------|
| 仅 Hotfix C# / AB | HybridCLR + 打包1001 | `build_android_debug.bat hotfix` |
| 主工程 / 大资源 / Il2Cpp | Export Project | `build_android_debug.bat`（默认 full） |
| 仅 pagBridge Java/C++ | 无 | `build_android_debug.bat skipcopy` |

---

## 各文件说明

### 打包与拷贝

| 文件 | 作用 |
|------|------|
| **build_android_debug.bat** | **主入口**。按模式拷贝 → 编译 PAG so → 同步 pagBridge → 清理 → Gradle 打 debug APK → 校验。 |
| **copy_unity_export_to_target.bat** | Unity Export 后，将 `ExportProject\unityLibrary` 下的 `assets`、`Il2CppOutputProject`、`jniLibs`、`jniStaticLibs`、`pagBridge.androidlib` 镜像到 `TargetProject`。已被主入口 full 模式调用，也可单独运行。 |
| **copy_hotfix_assets_to_target.bat** | 仅将热更 StreamingAssets 同步到 TargetProject（优先 ExportProject/assets，否则 `Assets/StreamingAssets`）。已被主入口 hotfix 模式调用。 |
| **sync_pagbridge_to_target.bat** | 将 `Assets/Plugins/Android/pagBridge.androidlib` 整目录复制到 TargetProject。主入口 Step 2 会再次同步，确保 Assets 侧改动生效。 |
| **clean_android_target.bat** | 停止 Gradle Daemon，删除 `launcher/build`、`unityLibrary/build`、`pagBridge/build`，执行 `gradlew clean`。防止 EOCD/损坏 APK。 |
| **build_pag_unity_gl_bridge.bat** | 用 Unity NDK（`E:\UnityNDK`）编译 `libpag_unity_gl_bridge.so`（armeabi-v7a），含符号校验。主入口 Step 1 自动调用。 |

### Export 进度诊断

| 文件 | 作用 |
|------|------|
| **watch_unity_editor_log.bat** | 实时 tail `%LOCALAPPDATA%\Unity\Editor\Editor.log`，高亮 Il2CPP/Export/Copying 等关键字。Export 进度条不动时并行开启。 |
| **check_export_progress.bat** | 快照 Editor.log 大小/末尾 + ExportProject 各目录 MB 与最新修改时间。加 `watch` 每 30 秒刷新。 |
| **check_export_progress.ps1** | 统计 `assets`、`Il2CppOutputProject`、`jniLibs` 目录大小（供 check_export_progress.bat 调用）。 |
| **check_export_progress_editor_tail.ps1** | 读取 Editor.log 最后 12 行写入日志（供 check_export_progress.bat 调用）。 |

### 真机 logcat 调试

| 文件 | 作用 |
|------|------|
| **watch_pag_logcat.bat** | 清空 buffer 后实时流式输出 PAG 相关 tag（PagBridge、PagOverlayManager、PagUnityGlBridge 等），写入 `logs/watch_pag_logcat.log`。 |
| **dump_pag_logcat.bat** | 一次性 `adb logcat -d`，筛选 PAG 行保存到 `logs/pag_logcat_dump.txt`。 |
| **capture_android_logcat.ps1** | 多设备场景导出完整 logcat；`-ClearFirst` 先清 buffer 再复现闪退；默认输出到工程根 `crash_after_repro.txt`。 |

### 文档（docs/）

| 文件 | 作用 |
|------|------|
| **EXPORT_ANDROID.md** | 解释 Export 进度条长时间不动的原因、真卡住 vs 只是慢的判别、按改动类型选打包路径。 |
| **RETEST_PAG_UFO.md** | PAG 转盘 Dragon→UFO 纹理模式复测步骤、关注 log tag、通过标准。 |
| **PAG_MAINTENANCE_PRIORITY.md** | PAG 已落地基线、渲染流程、后续维护优先级与风险点。 |

**美术向**（`Tools/docs/`）：**[PAG_ARTIST_GUIDE.md](../docs/PAG_ARTIST_GUIDE.md)**（完整版）、**[PAG_ARTIST_GUIDE_ONE_PAGER.md](../docs/PAG_ARTIST_GUIDE_ONE_PAGER.md)**（一页纸摘要，适合飞书/Word）。

### 日志（logs/）

脚本运行时自动写入，例如 `build_android_debug.log`、`copy_unity_export.log`、`watch_pag_logcat.log` 等。打包失败时优先查看对应日志。

---

## 典型流程

**全量：**
```bat
REM Unity: Export Project → TheOutput\ExportProject
Tools\build_android_debug.bat nopause
```

**日常热更：**
```bat
REM Unity: HybridCLR + NewBuild/打包1001
Tools\build_android_debug.bat hotfix nopause
```

**Export 期间诊断：**
```bat
Tools\watch_unity_editor_log.bat nopause
Tools\check_export_progress.bat watch
```

**PAG 真机复测：**
```bat
Tools\build_android_debug.bat hotfix nopause
Tools\watch_pag_logcat.bat nopause
```

---

## 其他 Tools 子目录

| 目录 | 说明 |
|------|------|
| `Tools/PagPreview/` | Unity Editor 内 PAG 预览（Node.js + libpag Web SDK） |
| `Tools/slot_export/` | 老虎机游戏记录导出 SQL/Python 工具 |
