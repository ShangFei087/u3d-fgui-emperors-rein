param(
    [string]$Source = "",
    [string]$BaselineVersionPath = "",
    [string]$Output = "",
    [string]$Filter = "",
    [switch]$DryRun,
    [switch]$IncludeTotalVersion,
    [switch]$NoPause
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..\..")).Path

if ([string]::IsNullOrWhiteSpace($Source)) {
    $Source = Join-Path $repoRoot "Assets\StreamingAssets\Hotfix"
}

function Get-LedgerKeyFromApplicationSettings([string]$RepoRoot) {
    $asset = Join-Path $RepoRoot "Assets\Resources\ApplicationSettings.asset"
    if (-not (Test-Path -LiteralPath $asset)) { return $null }

    $map = @{}
    Get-Content -LiteralPath $asset | ForEach-Object {
        if ($_ -match '^\s*(isMachine|isRelease|platformName|appVersion):\s*(.+)\s*$') {
            $map[$Matches[1]] = $Matches[2].Trim()
        }
    }
    if (-not $map.ContainsKey("platformName") -or -not $map.ContainsKey("appVersion")) { return $null }

    $platform = ($map["platformName"] -replace "[^A-Za-z0-9._-]", "")
    $appType = if ($map["isRelease"] -eq "1") { "release" } else { "debug" }
    $buildTarget = if ($map["isMachine"] -eq "1") { "machine" } else { "android" }
    $folder = ($map["appVersion"] -replace "\.", "_")
    return "{0}_{1}_{2}_{3}" -f $platform, $appType, $buildTarget, $folder
}

function Resolve-LedgerBaselinePath([string]$ScriptDir, [string]$RepoRoot) {
    $ledgerRoot = Join-Path $ScriptDir "ledger"
    $currentPath = Join-Path $ledgerRoot "current.json"
    if (Test-Path -LiteralPath $currentPath) {
        try {
            $cur = Get-Content -LiteralPath $currentPath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($cur.key) {
                $p = Join-Path $ledgerRoot (Join-Path $cur.key "version.json")
                if (Test-Path -LiteralPath $p) { return $p }
            }
        }
        catch { }
    }

    $settingsKey = Get-LedgerKeyFromApplicationSettings $RepoRoot
    if ($settingsKey) {
        $p = Join-Path $ledgerRoot (Join-Path $settingsKey "version.json")
        if (Test-Path -LiteralPath $p) { return $p }
    }

    return $null
}

if ([string]::IsNullOrWhiteSpace($BaselineVersionPath)) {
    $ledgerBaseline = Resolve-LedgerBaselinePath $scriptDir $repoRoot
    if ($ledgerBaseline) {
        $BaselineVersionPath = $ledgerBaseline
    }
    else {
        $BaselineVersionPath = Join-Path $scriptDir "baseline\version.json"
    }
}

$Source = (Resolve-Path -LiteralPath $Source).Path
$newVersionPath = Join-Path $Source "version.json"

if (-not (Test-Path -LiteralPath $newVersionPath)) {
    throw "未找到本次打包的 version.json: $newVersionPath`n请先执行 Unity: NewBuild/打包1001"
}

function Format-Bytes([long]$Bytes) {
    if ($Bytes -ge 1GB) { return "{0:N2} GB" -f ($Bytes / 1GB) }
    if ($Bytes -ge 1MB) { return "{0:N2} MB" -f ($Bytes / 1MB) }
    if ($Bytes -ge 1KB) { return "{0:N2} KB" -f ($Bytes / 1KB) }
    return "$Bytes B"
}

function Read-VersionJson([string]$Path) {
    $raw = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    return $raw | ConvertFrom-Json
}

function Get-HashMapFromVersion($versionObj, [string]$section, [string]$subSection) {
    $result = @{}
    if ($null -eq $versionObj) { return $result }

    $node = $versionObj
    if ($section) { $node = $node.$section }
    if ($subSection) { $node = $node.$subSection }
    if ($null -eq $node) { return $result }

    foreach ($prop in $node.PSObject.Properties) {
        $value = $prop.Value
        if ($value -is [string]) {
            $result[$prop.Name] = $value
        }
        elseif ($null -ne $value.hash) {
            $result[$prop.Name] = [string]$value.hash
        }
    }
    return $result
}

function Test-FilterMatch([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Filter)) { return $true }
    return $Path -like "*$Filter*"
}

function Add-CopyItem(
    [System.Collections.Generic.List[object]]$CopyList,
    [string]$RelativePath,
    [string]$Category,
    [string]$Reason
) {
    $srcFile = Join-Path $Source $RelativePath
    if (-not (Test-Path -LiteralPath $srcFile)) {
        Write-Warning "源文件不存在，跳过: $RelativePath"
        return
    }

    $size = (Get-Item -LiteralPath $srcFile).Length
    $CopyList.Add([pscustomobject]@{
        RelativePath = $RelativePath.Replace("\", "/")
        Category     = $Category
        Reason       = $Reason
        SizeBytes    = $size
        SourcePath   = $srcFile
    }) | Out-Null
}

$newVersion = Read-VersionJson $newVersionPath
$hasBaseline = Test-Path -LiteralPath $BaselineVersionPath
$oldVersion = $null
if ($hasBaseline) {
    $oldVersion = Read-VersionJson $BaselineVersionPath
}

$copyList = [System.Collections.Generic.List[object]]::new()

# version.json 始终需要
Add-CopyItem $copyList "version.json" "Meta" "热更清单（每次必更新）"

if ($IncludeTotalVersion) {
    $totalVersionSrc = Join-Path $Source "total_version.json"
    if (Test-Path -LiteralPath $totalVersionSrc) {
        Add-CopyItem $copyList "total_version.json" "Meta" "总版本路由表"
    }
}

# GameDll
$newDllMap = Get-HashMapFromVersion $newVersion "hotfix_dll"
$oldDllMap = Get-HashMapFromVersion $oldVersion "hotfix_dll"
foreach ($dllName in $newDllMap.Keys) {
    $newHash = $newDllMap[$dllName]
    $oldHash = $oldDllMap[$dllName]
    if (-not $hasBaseline -or $oldHash -ne $newHash) {
        $reason = if (-not $hasBaseline) { "无基线，全量" } elseif (-not $oldHash) { "新增 DLL" } else { "hash 变化" }
        Add-CopyItem $copyList ("GameDll\{0}.dll.bytes" -f $dllName) "GameDll" $reason
    }
}

# GameRes AB
$newBundleMap = Get-HashMapFromVersion $newVersion "asset_bundle" "bundle_hash"
$oldBundleMap = Get-HashMapFromVersion $oldVersion "asset_bundle" "bundle_hash"
$changedBundles = [System.Collections.Generic.List[string]]::new()

foreach ($bundleName in $newBundleMap.Keys) {
    if (-not (Test-FilterMatch $bundleName)) { continue }

    $newHash = $newBundleMap[$bundleName]
    $oldHash = $oldBundleMap[$bundleName]
    if (-not $hasBaseline -or $oldHash -ne $newHash) {
        $changedBundles.Add($bundleName) | Out-Null
        $reason = if (-not $hasBaseline) { "无基线，全量" } elseif (-not $oldHash) { "新增 AB" } else { "hash 变化" }
        Add-CopyItem $copyList ("GameRes\{0}" -f $bundleName) "GameRes" $reason
    }
}

# Manifest：有 AB 变化或 manifest hash 变化时必须带上
$oldManifestHash = $null
if ($null -ne $oldVersion -and $null -ne $oldVersion.asset_bundle.manifest) {
    $oldManifestHash = [string]$oldVersion.asset_bundle.manifest.hash
}
$newManifestHash = [string]$newVersion.asset_bundle.manifest.hash
$manifestChanged = (-not $hasBaseline) -or ($oldManifestHash -ne $newManifestHash) -or ($changedBundles.Count -gt 0)

if ($manifestChanged) {
    $reason = if (-not $hasBaseline) { "无基线，全量" } elseif ($oldManifestHash -ne $newManifestHash) { "manifest hash 变化" } else { "有 AB 变化，需更新 manifest" }
    Add-CopyItem $copyList "GameRes\GameRes" "GameRes" $reason
}

# GameBackup
$newBackupMap = Get-HashMapFromVersion $newVersion "asset_backup"
$oldBackupMap = Get-HashMapFromVersion $oldVersion "asset_backup"
foreach ($backupPath in $newBackupMap.Keys) {
    if (-not (Test-FilterMatch $backupPath)) { continue }

    $newHash = $newBackupMap[$backupPath]
    $oldHash = $oldBackupMap[$backupPath]
    if (-not $hasBaseline -or $oldHash -ne $newHash) {
        $reason = if (-not $hasBaseline) { "无基线，全量" } elseif (-not $oldHash) { "新增备份资源" } else { "hash 变化" }
        Add-CopyItem $copyList ("GameBackup\{0}" -f $backupPath) "GameBackup" $reason
    }
}

# 与基线完全一致时无需上传
if ($hasBaseline -and $copyList.Count -eq 1 -and $copyList[0].RelativePath -eq "version.json") {
    $oldJson = Get-Content -LiteralPath $BaselineVersionPath -Raw -Encoding UTF8
    $newJson = Get-Content -LiteralPath $newVersionPath -Raw -Encoding UTF8
    if ($oldJson -eq $newJson) {
        $copyList.Clear()
    }
}

# 统计
$byCategory = $copyList | Group-Object Category
$deltaBytes = ($copyList | Measure-Object -Property SizeBytes -Sum).Sum
if ($null -eq $deltaBytes) { $deltaBytes = 0 }

$gameResDir = Join-Path $Source "GameRes"
$fullGameResBytes = 0
if (Test-Path -LiteralPath $gameResDir) {
    $fullGameResBytes = (Get-ChildItem -LiteralPath $gameResDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
    if ($null -eq $fullGameResBytes) { $fullGameResBytes = 0 }
}

$gameResDeltaBytes = ($copyList | Where-Object { $_.Category -eq "GameRes" } | Measure-Object -Property SizeBytes -Sum).Sum
if ($null -eq $gameResDeltaBytes) { $gameResDeltaBytes = 0 }

$oldVer = if ($oldVersion) { [string]$oldVersion.hotfix_version } else { "(无基线)" }
$newVer = [string]$newVersion.hotfix_version

Write-Host ""
Write-Host "========================================"
Write-Host "  热更增量打包"
Write-Host "========================================"
Write-Host "源目录:     $Source"
Write-Host "基线:       $(if ($hasBaseline) { $BaselineVersionPath } else { '(未找到，按全量处理)' })"
Write-Host "热更版本:   $oldVer -> $newVer"
if ($Filter) { Write-Host "路径过滤:   *$Filter*" }
Write-Host ""

if (-not $hasBaseline) {
    Write-Host "[提示] 未找到基线 version.json。本次将复制全部资源。" -ForegroundColor Yellow
    Write-Host "       上传资源服后请执行: Tools\save_hotfix_baseline.bat" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "变化摘要:"
foreach ($group in $byCategory) {
    $catBytes = ($group.Group | Measure-Object -Property SizeBytes -Sum).Sum
  Write-Host ("  {0}: {1} 个文件, {2}" -f $group.Name, $group.Count, (Format-Bytes $catBytes))
}
Write-Host ("  合计: {0} 个文件, {1}" -f $copyList.Count, (Format-Bytes $deltaBytes))

if ($fullGameResBytes -gt 0 -and $gameResDeltaBytes -ge 0) {
    $saved = $fullGameResBytes - $gameResDeltaBytes
    if ($saved -gt 0) {
        $pct = [math]::Round(100.0 * $saved / $fullGameResBytes, 1)
        Write-Host ("  GameRes 全量约 {0}, 本次 GameRes {1} (约节省 {2}%)" -f (Format-Bytes $fullGameResBytes), (Format-Bytes $gameResDeltaBytes), $pct)
    }
}

Write-Host ""
Write-Host "文件清单:"
foreach ($item in ($copyList | Sort-Object Category, RelativePath)) {
    Write-Host ("  [{0}] {1} ({2}) - {3}" -f $item.Category, $item.RelativePath, (Format-Bytes $item.SizeBytes), $item.Reason)
}

if ($DryRun) {
    Write-Host ""
    Write-Host "[DryRun] 未复制任何文件。" -ForegroundColor Cyan
    exit 0
}

if ($copyList.Count -eq 0) {
    Write-Host ""
    Write-Host "[完成] 没有需要上传的文件（与基线一致）。" -ForegroundColor Green
    exit 0
}

if ([string]::IsNullOrWhiteSpace($Output)) {
    $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $Output = Join-Path $repoRoot "TheOutput\HotfixDelta\$stamp"
}
else {
    $Output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Output)
}

New-Item -ItemType Directory -Force -Path $Output | Out-Null

foreach ($item in $copyList) {
    $destFile = Join-Path $Output $item.RelativePath
    $destDir = Split-Path -Parent $destFile
    if (-not (Test-Path -LiteralPath $destDir)) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    }
    Copy-Item -LiteralPath $item.SourcePath -Destination $destFile -Force
}

# 写一份清单方便查阅
$manifestPath = Join-Path $Output "_delta_manifest.txt"
$lines = @(
    "generated_at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "source: $Source",
    "baseline: $(if ($hasBaseline) { $BaselineVersionPath } else { 'none' })",
    "hotfix_version: $oldVer -> $newVer",
    "total_files: $($copyList.Count)",
    "total_bytes: $deltaBytes",
    "",
    "files:"
)
foreach ($item in ($copyList | Sort-Object Category, RelativePath)) {
    $lines += "  [$($item.Category)] $($item.RelativePath) | $(Format-Bytes $item.SizeBytes) | $($item.Reason)"
}
$lines | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Write-Host ""
Write-Host "[完成] 增量包已生成:" -ForegroundColor Green
Write-Host "  $Output"
Write-Host ""
Write-Host "下一步:"
Write-Host "  1. 远程桌面打开上述文件夹"
Write-Host "  2. 将其中的内容覆盖粘贴到资源服热更目录"
Write-Host "  3. 上传成功后执行: Tools\save_hotfix_baseline.bat"
Write-Host ""

exit 0
