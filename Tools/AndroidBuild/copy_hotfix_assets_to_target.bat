@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"

set "LOG=%~dp0logs\copy_hotfix_assets.log"
set "SRC_STREAM=%ROOT%\Assets\StreamingAssets"
set "SRC_EXPORT=%ROOT%\TheOutput\ExportProject\unityLibrary\src\main\assets"
set "DST=%ROOT%\TheOutput\TargetProject\unityLibrary\src\main\assets"

echo [%date% %time%] copy hotfix assets start > "%LOG%"

echo.
echo ========================================
echo   仅拷�?Hotfix StreamingAssets -^> TargetProject
echo   (跳过 Unity 全量 Export，改完热更后�?
echo ========================================
echo.
echo [INFO] Log: %LOG%

if not exist "%DST%" (
    echo [ERROR] Target assets not found: %DST%
    echo [ERROR] Target not found >> "%LOG%"
    goto :Failed
)

set "SRC="
if exist "%SRC_EXPORT%" (
    set "SRC=%SRC_EXPORT%"
    echo [INFO] FROM ExportProject assets
) else if exist "%SRC_STREAM%" (
    set "SRC=%SRC_STREAM%"
    echo [INFO] FROM Assets\StreamingAssets
) else (
    echo [ERROR] No source assets found.
    echo [ERROR] no source >> "%LOG%"
    goto :Failed
)

echo [INFO] FROM: !SRC!
echo [INFO] TO:   %DST%
echo [INFO] FROM: !SRC! >> "%LOG%"
echo [INFO] TO: %DST% >> "%LOG%"

robocopy "!SRC!" "%DST%" /E /XO /R:2 /W:5 /NFL /NDL /NJH /NJS >> "%LOG%" 2>&1
set "RC=!ERRORLEVEL!"
if !RC! geq 8 (
    echo [ERROR] robocopy failed code=!RC!
    echo [ERROR] robocopy !RC! >> "%LOG%"
    goto :Failed
)

echo [OK] Hotfix assets synced.
echo [OK] done >> "%LOG%"
echo.
echo 下一�? Tools\AndroidBuild\build_android_debug.bat nopause
if /i not "%~1"=="nopause" pause
exit /b 0

:Failed
echo [FAILED] see %LOG%
if /i not "%~1"=="nopause" pause
exit /b 1
