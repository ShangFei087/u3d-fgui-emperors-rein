@echo off
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0HotfixDeploy\pack_hotfix_delta.ps1" %*
set "RC=%ERRORLEVEL%"
if /i not "%~1"=="nopause" pause
exit /b %RC%
