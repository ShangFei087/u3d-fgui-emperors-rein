@echo off
setlocal EnableDelayedExpansion

rem Unity ExportProject -> TargetProject copy script
rem Run after Unity Export Project to TheOutput\ExportProject
rem Usage - double-click this file, or run Tools\AndroidBuild\copy_unity_export_to_target.bat

cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"
for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"

set "EXPORT=%ROOT%\TheOutput\ExportProject\unityLibrary"
set "TARGET=%ROOT%\TheOutput\TargetProject\unityLibrary"
set "EXPORT_LAUNCHER=%ROOT%\TheOutput\ExportProject\launcher"
set "TARGET_LAUNCHER=%ROOT%\TheOutput\TargetProject\launcher"
set "PAG_SRC=%EXPORT%\pagBridge.androidlib"
set "PAG_DST=%TARGET%\pagBridge.androidlib"
set "PAG_FALLBACK=%ROOT%\Assets\Plugins\Android\pagBridge.androidlib"
set "LOG=%~dp0logs\copy_unity_export.log"

echo [%date% %time%] copy start > "%LOG%"
echo.

echo ========================================
echo   Unity Export copy -^> TargetProject
echo ========================================
echo.
echo ROOT: %ROOT%
echo.

if not exist "%EXPORT%\src\main" (
    echo [ERROR] Export not found:
    echo        %EXPORT%\src\main
    echo.
    echo Please Export Project in Unity to:
    echo   %ROOT%\TheOutput\ExportProject
    echo [ERROR] Export not found >> "%LOG%"
    goto :Failed
)

if not exist "%TARGET%\src\main" (
    echo [ERROR] TargetProject not found:
    echo        %TARGET%\src\main
    echo.
    echo Please ensure TheOutput\TargetProject exists.
    echo [ERROR] Target not found >> "%LOG%"
    goto :Failed
)

echo [INFO] FROM: %EXPORT%
echo [INFO] TO:   %TARGET%
echo.

call :DoRobocopy "%EXPORT%\src\main\assets" "%TARGET%\src\main\assets" "src\main\assets"
if errorlevel 8 goto :Failed

call :DoRobocopy "%EXPORT%\src\main\Il2CppOutputProject" "%TARGET%\src\main\Il2CppOutputProject" "src\main\Il2CppOutputProject"
if errorlevel 8 goto :Failed

call :DoRobocopy "%EXPORT%\src\main\jniLibs" "%TARGET%\src\main\jniLibs" "src\main\jniLibs"
if errorlevel 8 goto :Failed

call :DoRobocopy "%EXPORT%\src\main\jniStaticLibs" "%TARGET%\src\main\jniStaticLibs" "src\main\jniStaticLibs"
if errorlevel 8 goto :Failed

if not exist "!PAG_SRC!\build.gradle" (
    echo [WARN] Export has no pagBridge, fallback to Assets...
    set "PAG_SRC=!PAG_FALLBACK!"
)

if not exist "!PAG_SRC!\build.gradle" (
    echo [ERROR] pagBridge.androidlib not found
    echo        Export: %EXPORT%\pagBridge.androidlib
    echo        Assets: %PAG_FALLBACK%
    goto :Failed
)

echo [INFO] copy pagBridge.androidlib (skip build)...
if not exist "%PAG_DST%" mkdir "%PAG_DST%"
robocopy "!PAG_SRC!" "%PAG_DST%" /E /XD build .gradle /R:2 /W:5 /NFL /NDL /NJH /NJS
set "RC=!ERRORLEVEL!"
if !RC! geq 8 (
    echo [ERROR] pagBridge copy failed, code !RC!
    echo        Close Android Studio / Unity and retry.
    goto :Failed
)
echo [OK] pagBridge.androidlib
echo [OK] pagBridge >> "%LOG%"

call :SyncLauncherIdentity
if errorlevel 1 goto :Failed

dir /b "%TARGET%\libs\libpag-*.aar" >nul 2>&1
if not errorlevel 1 (
    echo [WARN] libpag AAR under unityLibrary\libs - remove it, keep only pagBridge.androidlib\libs
)

echo.
echo ========================================
echo   [SUCCESS] copy finished
echo ========================================
echo.
echo Next: close Android Studio, run
echo   Tools\AndroidBuild\build_android_debug.bat
echo.
echo Log: %LOG%
echo [OK] copy finished >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b 0

:DoRobocopy
set "SRC=%~1"
set "DST=%~2"
set "LABEL=%~3"

if not exist "%SRC%" (
    echo [ERROR] missing export folder: %SRC%
    echo [ERROR] missing %LABEL% >> "%LOG%"
    exit /b 16
)

echo [INFO] copy %LABEL% ...
if not exist "%DST%" mkdir "%DST%"
robocopy "%SRC%" "%DST%" /MIR /R:2 /W:5 /NFL /NDL /NJH /NJS
set "RC=!ERRORLEVEL!"
if !RC! geq 8 (
    echo [ERROR] %LABEL% copy failed, code !RC!
    echo        File may be locked. Close Android Studio / Unity and retry.
    echo [ERROR] robocopy %LABEL% code !RC! >> "%LOG%"
    exit /b 16
)
echo [OK] %LABEL%
echo [OK] %LABEL% >> "%LOG%"
exit /b 0

:SyncLauncherIdentity
rem Sync display name (strings.xml) and package id (applicationId + manifest package)
rem Keep Target launcher customizations (permissions / minSdk / version* etc.)

echo [INFO] sync launcher app_name + applicationId ...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0sync_launcher_identity.ps1" -ExportLauncher "%EXPORT_LAUNCHER%" -TargetLauncher "%TARGET_LAUNCHER%"
if errorlevel 1 (
    echo [ERROR] sync launcher identity failed
    echo [ERROR] sync launcher identity failed >> "%LOG%"
    exit /b 1
)

echo [OK] launcher strings.xml + applicationId + manifest package
echo [OK] launcher identity >> "%LOG%"
exit /b 0

:Failed
echo.
echo ========================================
echo   [FAILED] copy not completed
echo ========================================
echo Check errors above, or open log: %LOG%
echo.
if /i not "%~1"=="nopause" pause
exit /b 1
