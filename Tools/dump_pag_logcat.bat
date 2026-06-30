@echo off
call "%~dp0AndroidBuild\dump_pag_logcat.bat" %*
exit /b %ERRORLEVEL%
