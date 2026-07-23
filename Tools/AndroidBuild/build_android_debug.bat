@echo off
setlocal EnableDelayedExpansion

rem Build debug APK from command line (same as AS assembleDebug).
rem Usage:
rem   Tools\AndroidBuild\build_android_debug.bat [nopause]              full copy (ExportProject -> Target) + build
rem   Tools\AndroidBuild\build_android_debug.bat hotfix [nopause]       hotfix assets only + build
rem   Tools\AndroidBuild\build_android_debug.bat skipcopy [nopause]     skip copy (pagBridge-only changes) + build
rem APK name follows applicationId: com.lftlive.treasury.debug.machine.v1_2_0 -> treasury_debug_machine_v1_2_0.apk

cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"
for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"

set "TARGET=%ROOT%\TheOutput\TargetProject"
set "APK_DIR=%TARGET%\launcher\build\outputs\apk\debug"
set "APK="
set "LOG=%~dp0logs\build_android_debug.log"
set "NOPAUSE=0"
set "COPY_MODE=full"

for %%A in (%*) do (
    if /i "%%A"=="nopause" set "NOPAUSE=1"
    if /i "%%A"=="hotfix" set "COPY_MODE=hotfix"
    if /i "%%A"=="skipcopy" set "COPY_MODE=skipcopy"
)

echo [%date% %time%] build debug start copy_mode=!COPY_MODE! > "%LOG%"
echo.

echo ========================================
echo   build debug APK
echo ========================================
echo.
echo [INFO] Log: %LOG%
echo [INFO] Copy mode: !COPY_MODE!
echo.

if /i "!COPY_MODE!"=="skipcopy" (
    echo [INFO] Step 0/6: skip copy - skipcopy mode...
    echo [STEP 0] skip copy >> "%LOG%"
) else if /i "!COPY_MODE!"=="hotfix" (
    echo [INFO] Step 0/6: copy hotfix StreamingAssets -^> TargetProject...
    echo [STEP 0] copy hotfix assets >> "%LOG%"
    call "%~dp0copy_hotfix_assets_to_target.bat" nopause
    if errorlevel 1 (
        echo [ERROR] copy hotfix assets failed >> "%LOG%"
        goto :Failed
    )
    echo [OK] copy hotfix assets >> "%LOG%"
) else (
    echo [INFO] Step 0/6: copy Unity ExportProject -^> TargetProject...
    echo [STEP 0] copy unity export >> "%LOG%"
    call "%~dp0copy_unity_export_to_target.bat" nopause
    if errorlevel 1 (
        echo [ERROR] copy unity export failed >> "%LOG%"
        goto :Failed
    )
    echo [OK] copy unity export >> "%LOG%"
)

call :ResolveApkPath
if errorlevel 1 goto :Failed
echo [INFO] Expected APK: !APK!
echo [INFO] Expected APK: !APK! >> "%LOG%"

echo.
echo [INFO] Step 1/6: build libpag_unity_gl_bridge.so...
echo [STEP 1] build pag_unity_gl_bridge >> "%LOG%"
call "%~dp0build_pag_unity_gl_bridge.bat" nopause
if errorlevel 1 (
    echo [ERROR] build pag_unity_gl_bridge failed >> "%LOG%"
    goto :Failed
)
echo [OK] build pag_unity_gl_bridge >> "%LOG%"

echo.
echo [INFO] Step 2/6: sync pagBridge.androidlib...
echo [STEP 2] sync pagBridge >> "%LOG%"
call "%~dp0sync_pagbridge_to_target.bat" nopause
if errorlevel 1 (
    echo [ERROR] sync pagBridge failed >> "%LOG%"
    goto :Failed
)
echo [OK] sync pagBridge >> "%LOG%"

echo.
echo [INFO] Step 3/6: clean build directories...
echo [STEP 3] clean >> "%LOG%"
call "%~dp0clean_android_target.bat" nopause
if errorlevel 1 (
    echo [ERROR] clean failed >> "%LOG%"
    goto :Failed
)
echo [OK] clean >> "%LOG%"

echo.
echo [INFO] Step 4/6: gradlew :launcher:assembleDebug (about 3-8 min)...
echo [STEP 4] gradlew assembleDebug start >> "%LOG%"
cd /d "%TARGET%"
echo [INFO] Stopping Gradle Daemon (release file locks)...
call gradlew.bat --stop >> "%LOG%" 2>&1
timeout /t 2 /nobreak >nul
rem Re-resolve after copy/sync in case applicationId changed
call :ResolveApkPath
if errorlevel 1 goto :Failed
if exist "!APK!" del /f /q "!APK!" 2>nul
if exist "%APK_DIR%\launcher-debug.apk" del /f /q "%APK_DIR%\launcher-debug.apk" 2>nul
call gradlew.bat :launcher:assembleDebug
if errorlevel 1 (
    echo.
    echo [ERROR] Build failed. Check console output above.
    echo         Log: %LOG%
    echo [ERROR] gradlew assembleDebug failed >> "%LOG%"
    goto :Failed
)
echo [OK] gradlew assembleDebug >> "%LOG%"

echo.
echo [INFO] Step 5/6: verify APK...
echo [STEP 5] verify APK >> "%LOG%"
call :ResolveApkPath
if errorlevel 1 goto :Failed
if not exist "!APK!" (
    echo [ERROR] APK not found: !APK!
    echo [ERROR] APK not found: !APK! >> "%LOG%"
    goto :Failed
)

for %%A in ("!APK!") do set "APK_SIZE=%%~zA"
if "!APK_SIZE!"=="0" (
    echo [ERROR] APK size is 0 bytes.
    echo [ERROR] APK size 0 >> "%LOG%"
    goto :Failed
)

echo.
echo ========================================
echo   [OK] Build successful
echo ========================================
echo      APK: !APK!
echo      Size: !APK_SIZE! bytes
echo      Log: %LOG%
echo.
echo [OK] APK: !APK! size=!APK_SIZE! >> "%LOG%"
echo [OK] build finished >> "%LOG%"
if "!NOPAUSE!"=="0" pause
exit /b 0

:ResolveApkPath
rem Derive APK name from launcher/build.gradle applicationId
rem com.lftlive.treasury.debug.machine.v1_2_0 -> treasury_debug_machine_v1_2_0.apk
set "APK="
set "APP_ID="
for /f "tokens=2 delims='" %%A in ('findstr /C:"applicationId" "%TARGET%\launcher\build.gradle"') do (
    set "APP_ID=%%A"
    goto :ResolveApkPath_HaveId
)
echo [ERROR] applicationId not found in launcher\build.gradle
echo [ERROR] applicationId not found >> "%LOG%"
exit /b 1
:ResolveApkPath_HaveId
set "APK_BASE=!APP_ID!"
if /i "!APK_BASE:~0,12!"=="com.lftlive." set "APK_BASE=!APK_BASE:~12!"
set "APK_NAME=!APK_BASE:.=_!.apk"
set "APK=%APK_DIR%\!APK_NAME!"
exit /b 0

:Failed
echo.
echo ========================================
echo   [FAILED] build_android_debug
echo ========================================
echo      Log: %LOG%
echo.
echo [FAILED] build_android_debug >> "%LOG%"
if "!NOPAUSE!"=="0" pause
exit /b 1
