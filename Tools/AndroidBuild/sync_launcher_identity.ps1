# Sync launcher display name + package id from ExportProject to TargetProject.
# Keeps Target customizations (permissions / minSdk / version* etc.).
# IMPORTANT: applicationId / package replacements use Latin1 1:1 byte mapping
# so non-ASCII comments keep their original encoding intact.
param(
    [Parameter(Mandatory = $true)][string]$ExportLauncher,
    [Parameter(Mandatory = $true)][string]$TargetLauncher
)

$ErrorActionPreference = 'Stop'

function Get-AppIdFromGradle([string]$Path) {
    # applicationId is ASCII-only; Latin1 maps bytes 1:1
    $latin1 = [System.Text.Encoding]::GetEncoding(28591)
    $text = $latin1.GetString([System.IO.File]::ReadAllBytes($Path))
    if ($text -notmatch "applicationId\s+'([^']+)'") {
        throw "applicationId not found in: $Path"
    }
    return $Matches[1]
}

function Replace-AsciiInFilePreservingBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Replacement
    )

    $latin1 = [System.Text.Encoding]::GetEncoding(28591)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $text = $latin1.GetString($bytes)

    if (-not [regex]::IsMatch($text, $Pattern)) {
        throw "Pattern not found in: $Path  pattern=$Pattern"
    }

    $newText = [regex]::Replace($text, $Pattern, $Replacement, 1)
    if ($newText -eq $text) {
        return $false
    }

    [System.IO.File]::WriteAllBytes($Path, $latin1.GetBytes($newText))
    return $true
}

$strSrc = Join-Path $ExportLauncher 'src\main\res\values\strings.xml'
$strDst = Join-Path $TargetLauncher 'src\main\res\values\strings.xml'
$gradleSrc = Join-Path $ExportLauncher 'build.gradle'
$gradleDst = Join-Path $TargetLauncher 'build.gradle'
$manifestDst = Join-Path $TargetLauncher 'src\main\AndroidManifest.xml'

foreach ($p in @($strSrc, $gradleSrc, $gradleDst, $manifestDst)) {
    if (-not (Test-Path -LiteralPath $p)) {
        throw "Missing file: $p"
    }
}

$dstDir = Split-Path -Parent $strDst
if (-not (Test-Path -LiteralPath $dstDir)) {
    New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
}

# Binary copy preserves encoding of strings.xml
Copy-Item -LiteralPath $strSrc -Destination $strDst -Force

$appId = Get-AppIdFromGradle -Path $gradleSrc
if ([string]::IsNullOrWhiteSpace($appId)) {
    throw 'Export applicationId is empty'
}

$null = Replace-AsciiInFilePreservingBytes `
    -Path $gradleDst `
    -Pattern "applicationId\s+'[^']+'" `
    -Replacement "applicationId '$appId'"

$null = Replace-AsciiInFilePreservingBytes `
    -Path $manifestDst `
    -Pattern 'package="[^"]+"' `
    -Replacement ('package="{0}"' -f $appId)

$appName = '?'
$strBytes = [System.IO.File]::ReadAllBytes($strDst)
$strText = [System.Text.Encoding]::UTF8.GetString($strBytes)
if ($strText -match '<string name="app_name">([^<]+)</string>') {
    $appName = $Matches[1]
}

Write-Host ("[OK] app_name={0} applicationId={1}" -f $appName, $appId)
