@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%Setup-LogilityFreightRuntime.ps1"

if not exist "%PS_SCRIPT%" (
  echo ERROR: Missing script "%PS_SCRIPT%"
  exit /b 1
)

set "INSTALL_ROOT=C:\Logility_Freight"
if not "%~1"=="" set "INSTALL_ROOT=%~1"

echo Setting up Logility Freight runtime at "%INSTALL_ROOT%"...
powershell -NoProfile -ExecutionPolicy Bypass -File "%PS_SCRIPT%" -InstallRoot "%INSTALL_ROOT%"
set "RC=%ERRORLEVEL%"

if not "%RC%"=="0" (
  echo Runtime setup failed with exit code %RC%.
  exit /b %RC%
)

echo Runtime setup completed successfully.
exit /b 0
