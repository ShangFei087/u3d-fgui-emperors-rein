@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

set "LOG=%~dp0logs\watch_unity_editor_log.log"
set "EDITOR_LOG=%LOCALAPPDATA%\Unity\Editor\Editor.log"

echo [%date% %time%] watch unity editor log start > "%LOG%"

echo.
echo ========================================
echo   watch Unity Editor.log (Export 诊断)
echo ========================================
echo.
echo [INFO] Session log: %LOG%
echo [INFO] Source: %EDITOR_LOG%
echo [INFO] Session log: %LOG% >> "%LOG%"
echo [INFO] Source: %EDITOR_LOG% >> "%LOG%"
echo.
echo 关注关键�? Il2CPP, Building Player, Exporting, Copying, assets
echo Press Ctrl+C to stop.
echo.

if not exist "%EDITOR_LOG%" (
    echo [ERROR] Editor.log not found. Start Unity and retry Export.
    echo [ERROR] Editor.log not found: %EDITOR_LOG% >> "%LOG%"
    goto :Failed
)

for %%A in ("%EDITOR_LOG%") do echo [INFO] Editor.log size=%%~zA bytes modified=%%~tA
for %%A in ("%EDITOR_LOG%") do echo [INFO] Editor.log size=%%~zA modified=%%~tA >> "%LOG%"

echo.
echo ----- tail Editor.log (live) -----
echo ----- tail Editor.log ----- >> "%LOG%"

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$src = '%EDITOR_LOG%'; $log = '%LOG%';" ^
  "Get-Content -Path $src -Tail 5 -Encoding UTF8 | ForEach-Object { Write-Host $_ };" ^
  "Get-Content -Path $src -Wait -Tail 0 -Encoding UTF8 | ForEach-Object {" ^
  "  if ($_ -match 'Il2CPP|Building Player|Export|Copying|assets|Gradle|error|Error|failed|Failed') { Write-Host $_ }" ^
  "  Add-Content -Path $log -Value $_ -Encoding UTF8" ^
  "}"
set "RC=!ERRORLEVEL!"

echo.
echo [INFO] ended code=!RC!
echo [INFO] ended code=!RC! >> "%LOG%"
echo [%date% %time%] watch unity editor log finished >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b !RC!

:Failed
echo.
echo [FAILED] see %LOG%
if /i not "%~1"=="nopause" pause
exit /b 1
