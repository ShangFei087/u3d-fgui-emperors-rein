param(
    [int]$DurationSec = 30,
    [string]$Serial = "",
    [switch]$SkipPrompt
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$docsDir = Join-Path $scriptDir "docs"
$logDir = Join-Path $scriptDir "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$adbBase = @()
if ($Serial) {
    $adbBase += "-s", $Serial
}

function Test-AdbDevice {
    $out = & adb @adbBase devices 2>&1 | Out-String
    if ($out -notmatch "device\s*$") {
        throw "No adb device. Connect device/emulator and retry."
    }
}

function Show-ScenarioHelp {
    param([string]$Scenario)
    switch ($Scenario) {
        "PAG1" { return "Click PAG1 -> Fade.pag loop (repeat=-1). Keep playing 30s." }
        "PAG2" { return "Click PAG2 -> BigWin_1024.pag loop. Primary Profiler case." }
        "PAG3" { return "Click PAG3 -> XingXing2.pag loop." }
        "PAG4" { return "Click PAG4 -> BigWin 6-segment sequence. Re-click if sequence ends before 30s." }
        default { return "Follow PAG_PERF_RETEST.md" }
    }
}

Test-AdbDevice

$scenarios = @("PAG1", "PAG2", "PAG3", "PAG4")
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$summaryFile = Join-Path $logDir "pag_perf_retest_summary_${stamp}.md"

$summary = @(
    "# PAG Perf Retest Run $stamp",
    "",
    "| Scenario | Log file | requestGpuRenderFrame | flush lines | present/finish |",
    "|----------|----------|----------------------|-------------|----------------|"
)

Write-Host "PAG perf retest helper (logcat). FPS must be recorded separately in Unity Profiler."
Write-Host "See: docs\PAG_PERF_RETEST.md"
Write-Host ""

foreach ($scenario in $scenarios) {
    Write-Host "=== $scenario ==="
    Write-Host (Show-ScenarioHelp $scenario)
    if (-not $SkipPrompt) {
        Read-Host "Enter PageGameMain, start $scenario, then press Enter to capture ${DurationSec}s logcat"
    }

    & (Join-Path $scriptDir "capture_pag_frame_timing.ps1") -Scenario $scenario -DurationSec $DurationSec -Serial $Serial

    $latest = Get-ChildItem (Join-Path $logDir "pag_frame_timing_${scenario}_*.txt") |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latest) {
        $summary += "| $scenario | (missing) | - | - | - |"
        continue
    }

    $lines = Get-Content $latest.FullName -ErrorAction SilentlyContinue
    $req = ($lines | Select-String "requestGpuRenderFrame").Count
    $flush = ($lines | Select-String "flushGpuFrame|OnRenderEvent: flush").Count
    $present = ($lines | Select-String "onGpuFrameFlushed|notifyPlaybackFinished").Count
    $rel = $latest.Name
    $summary += "| $scenario | $rel | $req | $flush | $present |"
}

$summary += @(
    "",
    "## FPS (fill from Unity Profiler)",
    "",
    "| Scenario | FPS | Frame ms | Pass |",
    "|----------|-----|----------|------|",
    "| PAG1 | _待填_ | _待填_ | >=45 |",
    "| PAG2 | _待填_ | _待填_ | >=45 |",
    "| PAG3 | _待填_ | _待填_ | >=40 |",
    "| PAG4 | _待填_ | _待填_ | steady >=38 |",
    "",
    "Copy FPS rows into docs\PAG_PERF_RETEST.md when done."
)

$summary | Set-Content -Encoding UTF8 $summaryFile
Write-Host ""
Write-Host "Summary: $summaryFile"
