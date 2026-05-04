param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "Requires .NET 10 SDK." -ForegroundColor Cyan

$DotNet = "dotnet"
try {
    & $DotNet --info *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet --info failed"
    }
}
catch {
    $fallbackDotNet = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $fallbackDotNet) {
        $DotNet = $fallbackDotNet
    }
    else {
        throw "dotnet was not found on PATH and $fallbackDotNet does not exist."
    }
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & $DotNet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

Write-Host "[1/3] Restoring..." -ForegroundColor Cyan
Invoke-DotNet @("restore")

Write-Host "[2/3] Publishing single-file EXE..." -ForegroundColor Cyan
Invoke-DotNet @(
  "publish",
  ".\KugouLyricsMirror.csproj",
  "-c", $Configuration,
  "-r", $Runtime,
  "--self-contained", "true",
  "-p:PublishSingleFile=true",
  "-p:IncludeNativeLibrariesForSelfExtract=true",
  "-p:DebugType=None",
  "-p:DebugSymbols=false",
  "-o", ".\dist\$Runtime"
)

Write-Host "[3/3] Done." -ForegroundColor Green
Write-Host "EXE output: .\dist\$Runtime\KugouLyricsMirror.exe" -ForegroundColor Yellow
