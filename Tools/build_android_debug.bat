@echo off
call "%~dp0AndroidBuild\build_android_debug.bat" %*
exit /b %ERRORLEVEL%
