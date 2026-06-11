@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"

set "LOG=%~dp0logs\check_export_progress.log"
set "EXPORT=%ROOT%\TheOutput\ExportProject\unityLibrary\src\main"
set "EDITOR_LOG=%LOCALAPPDATA%\Unity\Editor\Editor.log"
set "WATCH=0"
if /i "%~1"=="watch" set "WATCH=1"
if /i "%~1"=="loop" set "WATCH=1"

:Snapshot
echo [%date% %time%] check export progress >> "%LOG%"
echo.
echo ========================================
echo   Unity Export 进度快照
echo ========================================
echo [INFO] Log: %LOG%
echo.

echo --- Editor.log ---
if exist "%EDITOR_LOG%" (
    for %%A in ("%EDITOR_LOG%") do (
        echo   size=%%~zA bytes  modified=%%~tA
        echo   Editor.log size=%%~zA modified=%%~tA >> "%LOG%"
    )
    echo   last lines:
    echo   Editor.log tail: >> "%LOG%"
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0check_export_progress_editor_tail.ps1" -EditorLog "%EDITOR_LOG%" -LogPath "%LOG%"
) else (
    echo   [WARN] Editor.log not found
    echo   [WARN] Editor.log not found >> "%LOG%"
)

echo.
echo --- ExportProject unityLibrary\src\main ---
if not exist "%EXPORT%" (
    echo   [WARN] Export dir not found: %EXPORT%
    echo   [WARN] Export dir not found >> "%LOG%"
    goto :MaybeLoop
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0check_export_progress.ps1" -Root "%ROOT%" -LogPath "%LOG%"
goto :AfterExport

:MaybeLoop

:AfterExport
if "%WATCH%"=="1" (
    echo.
    echo [INFO] watch mode: refresh in 30s, Ctrl+C to stop
    echo [INFO] watch sleep 30s >> "%LOG%"
    timeout /t 30 /nobreak >nul
    goto :Snapshot
)

echo.
echo [OK] snapshot done.
echo      Re-run: Tools\AndroidBuild\check_export_progress.bat watch
if /i not "%~1"=="nopause" pause
exit /b 0
