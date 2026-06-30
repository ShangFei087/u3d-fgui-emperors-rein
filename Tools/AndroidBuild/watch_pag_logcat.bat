@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

set "LOG=%~dp0logs\watch_pag_logcat.log"

echo [%date% %time%] watch pag logcat start > "%LOG%"

echo.
echo ========================================
echo   watch PAG logcat (live)
echo ========================================
echo.
echo [INFO] Log: %LOG%
echo [INFO] Log: %LOG% >> "%LOG%"
echo Filter: PagBridge, PagOverlayManager, PagBridgeUnity, PagUnityGlBridge, Unity, DEBUG, libc, tgfx
echo Filter: PagBridge, PagOverlayManager, PagBridgeUnity, PagUnityGlBridge, Unity, DEBUG, libc, tgfx >> "%LOG%"
echo Tombstone: adb logcat -b crash -d
echo Tombstone: adb logcat -b crash -d >> "%LOG%"
echo Press Ctrl+C to stop.
echo.

where adb >nul 2>&1
if errorlevel 1 (
    echo [ERROR] adb not found. Add Android SDK platform-tools to PATH.
    echo [ERROR] adb not found >> "%LOG%"
    goto :Failed
)

adb devices > "%TEMP%\pag_adb_devices.txt" 2>&1
type "%TEMP%\pag_adb_devices.txt" >> "%LOG%"
findstr /r /c:"device$" "%TEMP%\pag_adb_devices.txt" | findstr /v "List" >nul
if errorlevel 1 (
    echo [ERROR] No Android device found. Enable USB debugging and run: adb devices
    echo [ERROR] No Android device found >> "%LOG%"
    type "%TEMP%\pag_adb_devices.txt"
    del "%TEMP%\pag_adb_devices.txt" 2>nul
    goto :Failed
)
del "%TEMP%\pag_adb_devices.txt" 2>nul

echo [INFO] Clearing logcat buffer...
echo [INFO] adb logcat -c >> "%LOG%"
adb logcat -c >> "%LOG%" 2>&1

echo [INFO] Streaming logs (console + log file)...
echo [INFO] adb logcat stream start >> "%LOG%"
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$log = '%LOG%';" ^
  "adb logcat PagBridge:I PagOverlayManager:I PagBridgeUnity:I PagUnityGlBridge:I Unity:I DEBUG:I libc:W tgfx:W *:S 2>&1 | ForEach-Object { Write-Host $_; Add-Content -Path $log -Value $_ -Encoding UTF8 }"
set "RC=!ERRORLEVEL!"

echo.
echo [INFO] logcat ended (code !RC!).
echo [INFO] logcat ended (code !RC!) >> "%LOG%"
echo [%date% %time%] watch pag logcat finished >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b !RC!

:Failed
echo.
echo [FAILED] see log: %LOG%
echo [%date% %time%] watch pag logcat failed >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b 1
