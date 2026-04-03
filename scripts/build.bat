@echo off
setlocal
cd /d "%~dp0\.."

echo [1/2] Building project...
dotnet build -c Release
if errorlevel 1 (
  echo Build failed.
  exit /b 1
)

echo [2/2] Done.
exit /b 0
