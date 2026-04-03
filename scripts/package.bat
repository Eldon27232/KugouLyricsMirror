@echo off
setlocal
cd /d "%~dp0\.."

set RUNTIME=win-x64
set DIST=dist\%RUNTIME%
set OUT=release\KugouLyricsMirror-%RUNTIME%.zip

if not exist "%DIST%" (
  echo Dist folder not found: %DIST%
  echo Please run scripts\publish.ps1 first.
  exit /b 1
)

if not exist release mkdir release

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "Compress-Archive -Path '%DIST%\*' -DestinationPath '%OUT%' -Force"

echo Package created: %OUT%
exit /b 0
