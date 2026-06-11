@echo off

setlocal EnableDelayedExpansion

cd /d "%~dp0"

if not exist "%~dp0logs" mkdir "%~dp0logs"



set "LOG=%~dp0logs\build_pag_unity_gl_bridge.log"

for %%I in ("%~dp0..\..") do set "ROOT=%%~fI"

set "JNI=%ROOT%\Assets\Plugins\Android\pagBridge.androidlib\src\main\jni"

set "OUT=%ROOT%\Assets\Plugins\Android\pagBridge.androidlib\src\main\jniLibs\armeabi-v7a"

set "SO=%OUT%\libpag_unity_gl_bridge.so"

set "NESTED=%OUT%\armeabi-v7a\libpag_unity_gl_bridge.so"

set "NM=E:\UnityNDK\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-nm.exe"



echo [%date% %time%] build pag_unity_gl_bridge start > "%LOG%"



echo.

echo ========================================

echo   build libpag_unity_gl_bridge.so

echo ========================================

echo.

echo [INFO] Log: %LOG%

echo [INFO] Log: %LOG% >> "%LOG%"



if not exist "E:\UnityNDK\ndk-build.cmd" (

    echo [INFO] Creating junction E:\UnityNDK ...

    echo [INFO] Creating junction E:\UnityNDK ... >> "%LOG%"

    mklink /J E:\UnityNDK "E:\Unity Hub\2020.3.17f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK" >> "%LOG%" 2>&1

    if errorlevel 1 (

        echo [ERROR] mklink failed >> "%LOG%"

        goto :Failed

    )

)



if not exist "%OUT%" mkdir "%OUT%"



echo [INFO] JNI: %JNI%

echo [INFO] OUT: %OUT%

echo [INFO] JNI: %JNI% >> "%LOG%"

echo [INFO] OUT: %OUT% >> "%LOG%"



echo [INFO] Building libpag_unity_gl_bridge.so ...

echo [INFO] ndk-build start >> "%LOG%"

call E:\UnityNDK\ndk-build.cmd NDK_PROJECT_PATH=%JNI% APP_BUILD_SCRIPT=%JNI%\Android.mk NDK_APPLICATION_MK=%JNI%\Application.mk NDK_OUT=%OUT%\obj NDK_LIBS_OUT=%OUT% >> "%LOG%" 2>&1

if errorlevel 1 (

    echo [ERROR] ndk-build failed

    echo [ERROR] ndk-build failed >> "%LOG%"

    goto :Failed

)

echo [OK] ndk-build >> "%LOG%"



if exist "%NESTED%" (

    echo [INFO] Flatten nested output: %NESTED% -^> %SO%

    echo [INFO] Flatten nested output >> "%LOG%"

    copy /Y "%NESTED%" "%SO%" >> "%LOG%" 2>&1

    if errorlevel 1 (

        echo [ERROR] copy nested .so failed

        echo [ERROR] copy nested .so failed >> "%LOG%"

        goto :Failed

    )

    rmdir /s /q "%OUT%\armeabi-v7a" 2>nul

)



if not exist "%SO%" (

    echo [ERROR] Output not found: %SO%

    echo [ERROR] Output not found: %SO% >> "%LOG%"

    goto :Failed

)



for %%A in ("%SO%") do set "SO_SIZE=%%~zA"

if !SO_SIZE! LSS 17000 (

    echo [ERROR] .so too small !SO_SIZE! bytes, expected at least 17000. Nested flatten may have failed.

    echo [ERROR] .so too small !SO_SIZE! >> "%LOG%"

    goto :Failed

)



if not exist "%NM%" (

    echo [WARN] llvm-nm not found, skip symbol check: %NM%

    echo [WARN] llvm-nm not found >> "%LOG%"

) else (

    "%NM%" -D "%SO%" 2>>"%LOG%" | findstr /C:"PagGl_GetSetupPagGpuEventId" >nul

    if errorlevel 1 (

        echo [ERROR] PagGl_GetSetupPagGpuEventId not exported in %SO%

        echo [ERROR] missing PagGl_GetSetupPagGpuEventId >> "%LOG%"

        goto :Failed

    )

    "%NM%" -D "%SO%" 2>>"%LOG%" | findstr /C:"PagGl_GetFlushPagGpuEventId" >nul

    if errorlevel 1 (

        echo [ERROR] PagGl_GetFlushPagGpuEventId not exported in %SO%

        echo [ERROR] missing PagGl_GetFlushPagGpuEventId >> "%LOG%"

        goto :Failed

    )

    echo [OK] symbol check: PagGl_GetSetupPagGpuEventId, PagGl_GetFlushPagGpuEventId

    echo [OK] symbol check passed >> "%LOG%"

)



echo [OK] %SO% (!SO_SIZE! bytes)

echo [OK] %SO% (!SO_SIZE! bytes) >> "%LOG%"

echo [%date% %time%] build pag_unity_gl_bridge finished >> "%LOG%"

if /i not "%~1"=="nopause" pause

exit /b 0



:Failed

echo.

echo [FAILED] see log: %LOG%

echo [%date% %time%] build pag_unity_gl_bridge failed >> "%LOG%"

if /i not "%~1"=="nopause" pause

exit /b 1

