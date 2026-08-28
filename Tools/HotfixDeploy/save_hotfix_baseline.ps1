param(
    [string]$Source = "",
    [string]$BaselineVersionPath = "",
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

function Resolve-LedgerKey([string]$ScriptDir, [string]$RepoRoot) {
    $currentPath = Join-Path $ScriptDir "ledger\current.json"
    if (Test-Path -LiteralPath $currentPath) {
        try {
            $cur = Get-Content -LiteralPath $currentPath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($cur.key) { return [string]$cur.key }
        }
        catch { }
    }
    return Get-LedgerKeyFromApplicationSettings $RepoRoot
}

function Save-VersionCopy([string]$Src, [string]$Dest) {
    $destDir = Split-Path -Parent $Dest
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    if (Test-Path -LiteralPath $Dest) {
        $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupPath = Join-Path $destDir ("version_{0}.json.bak" -f $stamp)
        Copy-Item -LiteralPath $Dest -Destination $backupPath -Force
        Write-Host "已备份旧基线: $backupPath"
    }
    Copy-Item -LiteralPath $Src -Destination $Dest -Force
}

$Source = (Resolve-Path -LiteralPath $Source).Path
$srcVersion = Join-Path $Source "version.json"

if (-not (Test-Path -LiteralPath $srcVersion)) {
    throw "未找到 version.json: $srcVersion"
}

$legacyBaseline = Join-Path $scriptDir "baseline\version.json"
if ([string]::IsNullOrWhiteSpace($BaselineVersionPath)) {
    $ledgerKey = Resolve-LedgerKey $scriptDir $repoRoot
    if ($ledgerKey) {
        $BaselineVersionPath = Join-Path $scriptDir (Join-Path "ledger" (Join-Path $ledgerKey "uploaded.json"))
        $currentObj = @{ key = $ledgerKey } | ConvertTo-Json
        $ledgerRoot = Join-Path $scriptDir "ledger"
        New-Item -ItemType Directory -Force -Path $ledgerRoot | Out-Null
        Set-Content -LiteralPath (Join-Path $ledgerRoot "current.json") -Value $currentObj -Encoding UTF8
    }
    else {
        $BaselineVersionPath = $legacyBaseline
    }
}

Save-VersionCopy $srcVersion $BaselineVersionPath

# 兼容旧路径：同时更新 baseline/version.json
if ($BaselineVersionPath -ne $legacyBaseline) {
    Save-VersionCopy $srcVersion $legacyBaseline
}

$versionObj = Get-Content -LiteralPath $BaselineVersionPath -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host ""
Write-Host "[完成] 已标记「上次成功上传」基线:" -ForegroundColor Green
Write-Host "  $BaselineVersionPath"
Write-Host ("  hotfix_version: {0}" -f $versionObj.hotfix_version)
Write-Host ""
Write-Host "下次 pack_hotfix_delta 将与此文件对比。不会覆盖 Unity 的 ledger/*/version.json（续号账本）。"
Write-Host ""

exit 0
