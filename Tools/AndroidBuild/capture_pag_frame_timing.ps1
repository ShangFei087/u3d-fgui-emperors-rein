param(
    [ValidateSet("Baseline", "PAG1", "PAG2", "PAG3", "PAG4")]
    [string]$Scenario = "PAG2",
    [int]$DurationSec = 30,
    [string]$Serial = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$logDir = Join-Path $scriptDir "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outFile = Join-Path $logDir "pag_frame_timing_${Scenario}_${stamp}.txt"

$adbBase = @()
if ($Serial) {
    $adbBase += "-s", $Serial
}

Write-Host "Scenario=$Scenario Duration=${DurationSec}s Output=$outFile"
Write-Host "Clear logcat, then play $Scenario on device..."

& adb @adbBase logcat -c | Out-Null

$header = @(
    "# PAG frame timing capture",
    "# Scenario: $Scenario",
    "# Duration: ${DurationSec}s",
    "# Started: $(Get-Date -Format o)",
    ""
)
$header | Set-Content -Encoding UTF8 $outFile

$proc = Start-Process -FilePath "adb" -ArgumentList ($adbBase + @(
    "logcat", "-v", "time",
    "PagOverlayManager:I", "PagUnityGlBridge:I", "PagBridgeUnity:I", "*:S"
)) -RedirectStandardOutput $outFile -NoNewWindow -PassThru

Start-Sleep -Seconds $DurationSec

if (-not $proc.HasExited) {
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
}

Add-Content -Encoding UTF8 $outFile "`n# Ended: $(Get-Date -Format o)"

$lines = Get-Content $outFile -ErrorAction SilentlyContinue
$requestCount = ($lines | Select-String "requestGpuRenderFrame").Count
$flushCount = ($lines | Select-String "flushGpuFrame|OnRenderEvent: flush").Count
$presentCount = ($lines | Select-String "deliverGpuFrame|onGpuFrameFlushed|notifyPlaybackFinished").Count

Write-Host "requestGpuRenderFrame lines: $requestCount"
Write-Host "flush lines: $flushCount"
Write-Host "present/finish lines: $presentCount"
Write-Host "Saved: $outFile"
