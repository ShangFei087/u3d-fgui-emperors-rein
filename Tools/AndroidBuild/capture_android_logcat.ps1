# 多设备时指定序列号导出 logcat（复现 UFO 闪退后用）
# 用法:
#   .\capture_android_logcat.ps1
#   .\capture_android_logcat.ps1 -Serial emulator-5554
#   .\capture_android_logcat.ps1 -ClearFirst -OutFile ..\..\crash_after_repro.txt

param(
    [string]$Serial = "",
    [string]$OutFile = "..\..\crash_after_repro.txt",
    [switch]$ClearFirst
)

$adb = Get-Command adb -ErrorAction SilentlyContinue
if (-not $adb) {
    Write-Error "adb 不在 PATH。请把 Android SDK platform-tools 加入环境变量后再运行。"
    exit 1
}

$devices = @(adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "\tdevice$" })
if ($devices.Count -eq 0) {
    Write-Error "没有已连接的 device。请先启动模拟器或连接真机。"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($Serial)) {
    if ($devices.Count -eq 1) {
        $Serial = ($devices[0] -split "\t")[0].Trim()
        Write-Host "自动选择设备: $Serial"
    } else {
        Write-Host "检测到多台设备，请用 -Serial 指定其一:"
        adb devices
        exit 1
    }
}

$outPath = Join-Path $PSScriptRoot $OutFile
$outPath = [System.IO.Path]::GetFullPath($outPath)

if ($ClearFirst) {
    Write-Host "清空 logcat buffer: $Serial"
    adb -s $Serial logcat -c
    Write-Host "请在游戏中复现闪退，完成后再次运行本脚本（不要加 -ClearFirst）导出日志。"
    exit 0
}

Write-Host "导出 logcat -> $outPath"
adb -s $Serial logcat -d | Out-File -FilePath $outPath -Encoding utf8
Write-Host "完成。可搜索: recycled bitmap, FATAL EXCEPTION, lowmemorykiller, PlayTurnTableEnterSequence finished"
