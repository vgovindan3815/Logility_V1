@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%Create-Tn3270Subst.ps1"

if not exist "%PS_SCRIPT%" (
  echo ERROR: Missing script "%PS_SCRIPT%"
  exit /b 1
)

echo Setting up E: mapping and tn3270 DLL...
powershell -NoProfile -ExecutionPolicy Bypass -File "%PS_SCRIPT%"
set "RC=%ERRORLEVEL%"

if not "%RC%"=="0" (
  echo Setup failed with exit code %RC%.
  exit /b %RC%
)

echo Setup completed successfully.
exit /b 0
