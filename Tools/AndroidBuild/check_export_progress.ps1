param(
    [string]$Root,
    [string]$LogPath
)

function Write-DirStat {
    param([string]$Path, [string]$Label)
    if (-not (Test-Path -LiteralPath $Path)) {
        $line = "${Label}: (missing)"
        Write-Host "  $line"
        Add-Content -LiteralPath $LogPath -Value $line
        return
    }
    $files = Get-ChildItem -LiteralPath $Path -Recurse -File -ErrorAction SilentlyContinue
    $sum = ($files | Measure-Object -Property Length -Sum).Sum
    $mb = [math]::Round($sum / 1MB, 1)
    $latest = ($files | Sort-Object LastWriteTime -Descending | Select-Object -First 1).LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss')
    $line = "${Label}: ${mb} MB  latest_file=${latest}"
    Write-Host "  $line"
    Add-Content -LiteralPath $LogPath -Value $line
}

$export = Join-Path $Root 'TheOutput\ExportProject\unityLibrary\src\main'
Write-DirStat (Join-Path $export 'assets') 'assets'
Write-DirStat (Join-Path $export 'Il2CppOutputProject') 'Il2CppOutputProject'
Write-DirStat (Join-Path $export 'jniLibs') 'jniLibs'
Write-DirStat (Join-Path $Root 'TheOutput\ExportProject') 'ExportProject_root'
