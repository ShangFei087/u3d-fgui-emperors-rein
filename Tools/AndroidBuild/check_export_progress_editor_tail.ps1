param(
    [string]$EditorLog,
    [string]$LogPath
)

Get-Content -LiteralPath $EditorLog -Tail 12 -Encoding UTF8 | ForEach-Object {
    Write-Host "  $_"
    Add-Content -LiteralPath $LogPath -Value $_
}
