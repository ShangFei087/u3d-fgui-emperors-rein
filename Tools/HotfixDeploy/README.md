# 热更增量部署

对比「上次上传后的 version.json（基线）」与「本次 Unity 打包产物」，只生成需要覆盖到资源服的小文件夹，避免每次远程桌面全量复制 `GameRes`。

## 快速使用

```text
1. Unity: HybridCLR 编译 + NewBuild/打包1001
2. Tools\pack_hotfix_delta.bat
3. 打开 TheOutput\HotfixDelta\最新时间戳文件夹
4. 远程桌面：将文件夹内容覆盖粘贴到资源服热更目录
5. Tools\save_hotfix_baseline.bat   （标记线上已是此版本）
```

## 脚本说明

| 脚本 | 作用 |
|------|------|
| `Tools\pack_hotfix_delta.bat` | 生成增量热更包到 `TheOutput/HotfixDelta/` |
| `Tools\save_hotfix_baseline.bat` | 保存当前 `version.json` 为基线，供下次对比 |

## 常用参数（PowerShell）

```powershell
# 只查看清单，不复制文件
Tools\HotfixDeploy\pack_hotfix_delta.ps1 -DryRun

# 只打包某个游戏相关 AB（路径模糊匹配）
Tools\HotfixDeploy\pack_hotfix_delta.ps1 -Filter "slot zhu zai jin bi 1700"

# 一并带上 total_version.json
Tools\HotfixDeploy\pack_hotfix_delta.ps1 -IncludeTotalVersion
```

## 首次使用

若 `baseline/version.json` 不存在，脚本会按**全量**处理（复制所有 DLL / AB / Backup + version.json）。

上传资源服成功后执行一次 `save_hotfix_baseline.bat`，之后即可走增量。

若你确认资源服当前版本就是包内版本、但还没传过，也可以先执行 `save_hotfix_baseline.bat` 建立基线，再开始增量打包。

## 基线文件

分两份，不要混用：

| 文件 | 谁写入 | 用途 |
|------|--------|------|
| `ledger/{key}/version.json` | Unity NewBuild / 切版本 | 这条线上次**打包**的号，用来 1.2 / 1.4 续号 |
| `ledger/{key}/uploaded.json` | `save_hotfix_baseline.bat` | 这条线上次**成功传到资源服**的清单，用来算增量 |
| `baseline/version.json` | `save_hotfix_baseline.bat`（兼容副本） | 没有 `uploaded.json` 时的回退 |

`key` 形如 `Treasury_debug_machine_1_2_0`。`current.json` 指向当前产品线。

`pack_hotfix_delta` **只对比 uploaded.json**（没有则用 `baseline/version.json`），不会拿 Unity 刚写入的 `version.json` 当基线，否则 NewBuild 后增量永远是空的。

Unity 侧：

- 切 `ApplicationSettings` 版本时自动把当前完整 `version.json` 存入对应账本，并只把该线的 `hotfix_version` / `hotfix_key` 写回包内（hash 仍由下次打包重算）
- 打包热更时若账本有记录，会从上次的号继续 +1（例如 1.2.22 → 1.2.23）
- 也可菜单 `NewBuild/保存当前热更账本` 手动存一次（只更新打包账本，不代表已上传）

## 对比规则（与客户端热更一致）

- `hotfix_dll`：按每个 DLL 的 hash 比较
- `asset_bundle.bundle_hash`：按每个 `.unity3d` 的 hash 比较；有变化时附带 `GameRes/GameRes` manifest
- `asset_backup`：按每个备份文件的 hash 比较
- `version.json`：每次必带

## 粘贴到资源服

增量包目录结构示例：

```text
HotfixDelta/20260702_153000/
├── version.json
├── GameDll/HotFix.dll.bytes
├── GameRes/GameRes
├── GameRes/games/.../fguis.unity3d
└── _delta_manifest.txt    （清单，不必上传）
```

将除 `_delta_manifest.txt` 外的内容**合并覆盖**到资源服对应热更目录即可（不要删掉服务器上未变化的 AB）。
