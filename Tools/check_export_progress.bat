@echo off
call "%~dp0AndroidBuild\check_export_progress.bat" %*
exit /b %ERRORLEVEL%
