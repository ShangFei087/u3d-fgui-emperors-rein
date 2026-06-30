@echo off

setlocal EnableDelayedExpansion



rem Clean TargetProject build outputs to avoid EOCD / corrupted APK issues.

rem Run this with Android Studio fully closed.

rem Usage: Tools\AndroidBuild\clean_android_target.bat [nopause]



cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"

for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"



set "TARGET=%ROOT%\TheOutput\TargetProject"

set "LOG=%~dp0logs\clean_android_target.log"



echo [%date% %time%] clean start > "%LOG%"

echo.



echo ========================================

echo   clean TargetProject build

echo ========================================

echo.

echo [INFO] Log: %LOG%

echo.



if not exist "%TARGET%\gradlew.bat" (

    echo [ERROR] TargetProject not found: %TARGET%

    echo [ERROR] TargetProject not found >> "%LOG%"

    goto :Failed

)



echo [INFO] TargetProject: %TARGET%

echo [INFO] Target: %TARGET% >> "%LOG%"

cd /d "%TARGET%"



echo [INFO] Stopping Gradle Daemon...

call gradlew.bat --stop

echo [OK] gradlew --stop >> "%LOG%"

timeout /t 2 /nobreak >nul



echo [INFO] Removing launcher\build ...

if exist "launcher\build" rmdir /s /q "launcher\build"

echo [OK] removed launcher\build >> "%LOG%"



echo [INFO] Removing unityLibrary\build ...

if exist "unityLibrary\build" rmdir /s /q "unityLibrary\build"

echo [OK] removed unityLibrary\build >> "%LOG%"



echo [INFO] Removing unityLibrary\pagBridge.androidlib\build ...

if exist "unityLibrary\pagBridge.androidlib\build" rmdir /s /q "unityLibrary\pagBridge.androidlib\build"

echo [OK] removed pagBridge\build >> "%LOG%"



echo [INFO] Running gradlew clean ...

call gradlew.bat clean >nul 2>&1

echo [OK] gradlew clean >> "%LOG%"



echo.

echo [OK] Clean finished.

echo      Next: Tools\AndroidBuild\build_android_debug.bat

echo.

echo [OK] clean finished >> "%LOG%"

if /i not "%~1"=="nopause" pause

exit /b 0



:Failed

echo.

echo [FAILED] clean

echo [FAILED] >> "%LOG%"

echo Log: %LOG%

echo.

if /i not "%~1"=="nopause" pause

exit /b 1

