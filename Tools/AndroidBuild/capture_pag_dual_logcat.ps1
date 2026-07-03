param(
    [ValidateSet("PAG1_PAG2", "PAG1_PAG3", "PAG1_PAG5", "PAG5_PAG2", "PAG7_PAG2", "NPC_BIGWIN_NORMAL", "NPC_FREE_NORMAL")]
    [string]$Combo = "PAG1_PAG2",
    [int]$DurationSec = 30,
    [string]$Serial = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$logDir = Join-Path $scriptDir "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$help = @{
    PAG1_PAG2 = "1) Click PAG1 (Fade), wait 5s. 2) Click PAG2 (BigWin_1024). Capture 30s."
    PAG1_PAG3 = "1) Click PAG1, wait 5s. 2) Click PAG3 (XingXing2). Capture 30s."
    PAG1_PAG5 = "1) Click PAG1, wait 5s. 2) Click PAG5 (glow 720). Capture 30s."
    PAG5_PAG2 = "1) Click PAG5, wait 5s. 2) Click PAG2. Capture 30s."
    PAG7_PAG2 = "1) Click PAG7, wait 5s. 2) Click PAG2. Capture 30s."
    NPC_BIGWIN_NORMAL = "PageTest: 1) btnBigwinNpc (PT5), wait 5s. 2) btnNormalNpc (PT7). Capture 30s. Pass: FPS>=28, RecoverFromStall~0."
    NPC_FREE_NORMAL = "PageTest B0: 1) btnFreeNpc (PT6). 2) btnNormalNpc (PT7). Wait Free finish; Normal must keep playing. Capture 30s."
}

Write-Host "Combo=$Combo Duration=${DurationSec}s"
Write-Host $help[$Combo]
Read-Host "Press Enter when both PAG buttons are playing"

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outFile = Join-Path $logDir "pag_dual_${Combo}_${stamp}.txt"

$adbBase = @()
if ($Serial) { $adbBase += "-s", $Serial }

& adb @adbBase logcat -c | Out-Null
$proc = Start-Process -FilePath "adb" -ArgumentList ($adbBase + @(
    "logcat", "-v", "time",
    "PagOverlayManager:I", "PagUnityGlBridge:I", "PagBridgeUnity:I", "Unity:I", "*:S"
)) -RedirectStandardOutput $outFile -NoNewWindow -PassThru

Start-Sleep -Seconds $DurationSec
if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }

$lines = Get-Content $outFile -ErrorAction SilentlyContinue
$stall = ($lines | Select-String "RecoverFromStall").Count
$partial = ($lines | Select-String "flushBatch partial").Count
$flush = ($lines | Select-String "flushBatch").Count
$backlog = ($lines | Select-String "GL queue backlog").Count

Write-Host "Saved: $outFile"
Write-Host "RecoverFromStall: $stall | flushBatch: $flush | partial flush: $partial | GL backlog: $backlog"
