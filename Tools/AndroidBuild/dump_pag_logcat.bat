@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

set "LOG=%~dp0logs\dump_pag_logcat.log"
set "OUT=%~dp0logs\pag_logcat_dump.txt"

echo [%date% %time%] dump pag logcat start > "%LOG%"

echo.
echo ========================================
echo   dump PAG logcat snapshot
echo ========================================
echo.
echo [INFO] Log: %LOG%
echo [INFO] Dump: %OUT%
echo [INFO] Log: %LOG% >> "%LOG%"
echo [INFO] Dump: %OUT% >> "%LOG%"

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

echo [INFO] adb logcat -d ...
echo [INFO] adb logcat -d >> "%LOG%"
adb logcat -d > "%TEMP%\pag_logcat_full.txt" 2>> "%LOG%"
if errorlevel 1 (
    echo [ERROR] adb logcat -d failed
    echo [ERROR] adb logcat -d failed >> "%LOG%"
    goto :Failed
)

findstr /i /c:"PagBridge" /c:"PagOverlayManager" /c:"PagBridgeUnity" /c:"PagUnityGlBridge" /c:"1700 PAG" /c:"PAG Path" /c:"PAG JNI" /c:"[PAG]" "%TEMP%\pag_logcat_full.txt" > "%OUT%" 2>> "%LOG%"
del "%TEMP%\pag_logcat_full.txt" 2>nul

if not exist "%OUT%" (
    echo [ERROR] Failed to create dump file.
    echo [ERROR] Failed to create dump file >> "%LOG%"
    goto :Failed
)

for %%A in ("%OUT%") do set "SIZE=%%~zA"
echo [OK] Saved %OUT% (!SIZE! bytes)
echo [OK] Saved %OUT% (!SIZE! bytes) >> "%LOG%"

echo. >> "%LOG%"
echo ----- pag_logcat_dump.txt ----- >> "%LOG%"
type "%OUT%" >> "%LOG%"
echo ----- end dump ----- >> "%LOG%"

echo [%date% %time%] dump pag logcat finished >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b 0

:Failed
echo.
echo [FAILED] see log: %LOG%
echo [%date% %time%] dump pag logcat failed >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b 1
