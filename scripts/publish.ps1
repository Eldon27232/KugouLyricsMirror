param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "[1/3] Restoring..." -ForegroundColor Cyan
dotnet restore

Write-Host "[2/3] Publishing single-file EXE..." -ForegroundColor Cyan
dotnet publish .\KugouLyricsMirror.csproj `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o ".\dist\$Runtime"

Write-Host "[3/3] Done." -ForegroundColor Green
Write-Host "EXE output: .\dist\$Runtime\KugouLyricsMirror.exe" -ForegroundColor Yellow
