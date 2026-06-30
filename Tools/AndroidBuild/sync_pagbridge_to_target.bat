@echo off

setlocal



rem Copy pagBridge.androidlib from Unity Assets to TargetProject before Gradle build.

rem Usage: Tools\AndroidBuild\sync_pagbridge_to_target.bat [nopause]



cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"



set "SRC=%ROOT%\Assets\Plugins\Android\pagBridge.androidlib"

set "DST=%ROOT%\TheOutput\TargetProject\unityLibrary\pagBridge.androidlib"

set "LOG=%~dp0logs\sync_pagbridge.log"



echo [%date% %time%] sync pagBridge start > "%LOG%"

echo.



echo ========================================

echo   sync pagBridge -^> TargetProject

echo ========================================

echo.

echo [INFO] Log: %LOG%

echo.



if not exist "%SRC%" (

    echo [ERROR] Source not found: %SRC%

    echo [ERROR] Source not found: %SRC% >> "%LOG%"

    goto :Failed

)



if not exist "%ROOT%\TheOutput\TargetProject\unityLibrary" (

    echo [ERROR] TargetProject unityLibrary not found. Export Unity project first.

    echo [ERROR] Target not found >> "%LOG%"

    goto :Failed

)



echo [INFO] FROM: %SRC%

echo [INFO] TO:   %DST%

echo [INFO] FROM: %SRC% >> "%LOG%"

echo [INFO] TO: %DST% >> "%LOG%"



if exist "%DST%" rmdir /s /q "%DST%"

xcopy /E /I /Y "%SRC%" "%DST%" >nul

if errorlevel 1 (

    echo [ERROR] xcopy failed

    echo [ERROR] xcopy failed >> "%LOG%"

    goto :Failed

)



echo.

echo [OK] pagBridge synced.

echo [OK] sync finished >> "%LOG%"

echo.

if /i not "%~1"=="nopause" pause

exit /b 0



:Failed

echo.

echo [FAILED] sync pagBridge

echo [FAILED] >> "%LOG%"

echo Log: %LOG%

echo.

if /i not "%~1"=="nopause" pause

exit /b 1

