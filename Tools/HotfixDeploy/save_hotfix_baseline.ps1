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
if ([string]::IsNullOrWhiteSpace($BaselineVersionPath)) {
    $BaselineVersionPath = Join-Path $scriptDir "baseline\version.json"
}

$Source = (Resolve-Path -LiteralPath $Source).Path
$srcVersion = Join-Path $Source "version.json"

if (-not (Test-Path -LiteralPath $srcVersion)) {
    throw "未找到 version.json: $srcVersion"
}

$baselineDir = Split-Path -Parent $BaselineVersionPath
New-Item -ItemType Directory -Force -Path $baselineDir | Out-Null

# 保留上一份基线备份
if (Test-Path -LiteralPath $BaselineVersionPath) {
    $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupPath = Join-Path $baselineDir ("version_{0}.json.bak" -f $stamp)
    Copy-Item -LiteralPath $BaselineVersionPath -Destination $backupPath -Force
    Write-Host "已备份旧基线: $backupPath"
}

Copy-Item -LiteralPath $srcVersion -Destination $BaselineVersionPath -Force

$versionObj = Get-Content -LiteralPath $BaselineVersionPath -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host ""
Write-Host "[完成] 基线已更新:" -ForegroundColor Green
Write-Host "  $BaselineVersionPath"
Write-Host ("  hotfix_version: {0}" -f $versionObj.hotfix_version)
Write-Host ""
Write-Host "下次执行 pack_hotfix_delta 将与此版本对比，只打包变化文件。"
Write-Host ""

exit 0
