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
set "PAG_SRC=%EXPORT%\pagBridge.androidlib"
set "PAG_DST=%TARGET%\pagBridge.androidlib"
set "PAG_FALLBACK=%ROOT%\Assets\Plugins\Android\pagBridge.androidlib"
set "LOG=%~dp0logs\copy_unity_export.log"

echo [%date% %time%] copy start > "%LOG%"
echo.

echo ========================================
echo   Unity 导出拷贝�?TargetProject
echo ========================================
echo.
echo 工程根目�? %ROOT%
echo.

if not exist "%EXPORT%\src\main" (
    echo [错误] 找不�?Unity 导出目录:
    echo        %EXPORT%\src\main
    echo.
    echo 请先�?Unity �? File -^> Build Settings -^> Export Project
    echo 导出�? %ROOT%\TheOutput\ExportProject
    echo [错误] Export not found >> "%LOG%"
    goto :Failed
)

if not exist "%TARGET%\src\main" (
    echo [错误] 找不�?TargetProject:
    echo        %TARGET%\src\main
    echo.
    echo 请确�?TheOutput\TargetProject 工程存在�?
    echo [错误] Target not found >> "%LOG%"
    goto :Failed
)

echo [信息] �? %EXPORT%
echo [信息] 目标: %TARGET%
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
    echo [警告] 导出目录�?pagBridge，改�?Assets 中的版本...
    set "PAG_SRC=!PAG_FALLBACK!"
)

if not exist "!PAG_SRC!\build.gradle" (
    echo [错误] 找不�?pagBridge.androidlib
    echo        Export: %EXPORT%\pagBridge.androidlib
    echo        Assets: %PAG_FALLBACK%
    goto :Failed
)

echo [信息] 拷贝 pagBridge.androidlib（跳�?build 目录�?..
if not exist "%PAG_DST%" mkdir "%PAG_DST%"
robocopy "!PAG_SRC!" "%PAG_DST%" /E /XD build .gradle /R:2 /W:5 /NFL /NDL /NJH /NJS
set "RC=!ERRORLEVEL!"
if !RC! geq 8 (
    echo [错误] pagBridge 拷贝失败，错误码 !RC!
    echo        请关�?Android Studio / Unity 后重试�?
    goto :Failed
)
echo [完成] pagBridge.androidlib
echo [OK] pagBridge >> "%LOG%"

dir /b "%TARGET%\libs\libpag-*.aar" >nul 2>&1
if not errorlevel 1 (
    echo [警告] unityLibrary\libs 下有 libpag AAR，请删除，只保留 pagBridge.androidlib\libs 里的一�?
)

echo.
echo ========================================
echo   [成功] 拷贝完成
echo ========================================
echo.
echo 下一�? 关闭 Android Studio，运�?
echo   Tools\AndroidBuild\build_android_debug.bat
echo.
echo 日志: %LOG%
echo [OK] copy finished >> "%LOG%"
if /i not "%~1"=="nopause" pause
exit /b 0

:DoRobocopy
set "SRC=%~1"
set "DST=%~2"
set "LABEL=%~3"

if not exist "%SRC%" (
    echo [错误] 导出目录缺少文件�? %SRC%
    echo [错误] missing %LABEL% >> "%LOG%"
    exit /b 16
)

echo [信息] 拷贝 %LABEL% ...
if not exist "%DST%" mkdir "%DST%"
robocopy "%SRC%" "%DST%" /MIR /R:2 /W:5 /NFL /NDL /NJH /NJS
set "RC=!ERRORLEVEL!"
if !RC! geq 8 (
    echo [错误] %LABEL% 拷贝失败，错误码 !RC!
    echo        文件可能被占用，请关�?Android Studio / Unity 后重试�?
    echo [错误] robocopy %LABEL% code !RC! >> "%LOG%"
    exit /b 16
)
echo [完成] %LABEL%
echo [OK] %LABEL% >> "%LOG%"
exit /b 0

:Failed
echo.
echo ========================================
echo   [失败] 拷贝未成�?
echo ========================================
echo 请查看上方错误信息，或打开日志: %LOG%
echo.
if /i not "%~1"=="nopause" pause
exit /b 1
