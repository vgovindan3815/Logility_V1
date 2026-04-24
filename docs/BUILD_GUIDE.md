# Logility Freight - Build Guide

This guide is for the current repository state and current project file settings.

## Project Facts

- Framework: .NET Framework 4.8
- Language: VB.NET + WPF
- Platform target: x64
- Assembly name: Logility_Freight
- Project file: FXF3A_Tool.vbproj

## Prerequisites

1. Windows x64 with .NET Framework 4.8.
2. MSBuild available. In this repo, the reliable local command is the explicit Framework path shown below.
3. Required managed FedEx DLLs present in deploy\.
4. Runtime support source files present in the repo:
   - deploy\tn3270_dll.dll
   - deploy\fxf3270.rsf
   - Screenlayouts\ScreenLayouts.xml
5. For user-machine setup, use:
   - docs\Run-Tn3270Subst.bat
   - docs\Run-LogilityRuntimeSetup.bat
   - docs\Create-Tn3270Subst.ps1
   - docs\Setup-LogilityFreightRuntime.ps1

## Build Commands

Debug build:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' FXF3A_Tool.vbproj /p:Configuration=Debug /t:Rebuild /v:minimal
```

Release build:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' FXF3A_Tool.vbproj /p:Configuration=Release /t:Rebuild /v:minimal
```

## Expected Outputs

- Debug: bin\Debug\Logility_Freight.exe
- Release: bin\Release\Logility_Freight.exe

## Run Commands

Debug run:

```powershell
.\bin\Debug\Logility_Freight.exe
```

Release run:

```powershell
.\bin\Release\Logility_Freight.exe
```

## VS Code Tasks

Available tasks in this workspace:

- Build Debug
- Build Release
- Run Debug
- Clean

Note: the current VS Code tasks call bare msbuild. If msbuild is not on PATH, use the explicit Framework MSBuild command from this guide instead.

## Runtime Source And Target Layout

- Source managed DLLs: deploy\FedEx.PABST.SS.*.dll
- Source native tn3270 DLL: deploy\tn3270_dll.dll
- Source RSF: deploy\fxf3270.rsf
- Source layout XML: Screenlayouts\ScreenLayouts.xml
- Built runtime target: keep all of the above beside Logility_Freight.exe in bin\Debug, bin\Release, or the final user install folder
- Native tn3270 machine path: E:\fedex\tn3270\tn3270_dll.dll

Recommended machine setup commands:

```powershell
.\docs\Run-LogilityRuntimeSetup.bat
```

Optional custom install path:

```powershell
.\docs\Run-LogilityRuntimeSetup.bat D:\Logility_Freight
```

If only the tn3270 mapping must be refreshed:

```powershell
.\docs\Run-Tn3270Subst.bat
```

Support/advanced PowerShell options:

```powershell
.\docs\Setup-LogilityFreightRuntime.ps1 -InstallRoot 'C:\Logility_Freight'
.\docs\Create-Tn3270Subst.ps1
```

## Build Verification

After a successful build, verify these files exist in output folder:

- Logility_Freight.exe
- FedEx.PABST.SS.SSLib.dll
- FedEx.PABST.SS.Exceptions.dll
- FedEx.PABST.SS.Screens.*.dll set
- tn3270_dll.dll
- fxf3270.rsf
- ScreenLayouts.xml

## Common Errors

| Error | Cause | Fix |
|---|---|---|
| Could not find file deploy\*.dll | Missing dependency package | Re-copy required FedEx DLLs into deploy\ |
| Screen layout XML file was not found | Missing or wrong path | Ensure Screenlayouts\ScreenLayouts.xml exists and is copied beside the executable |
| Unable to login to mainframe session | Runtime/environment issue | Validate E: mapping, runtime folder contents, and connection fields |
| incorrect format when loading FXF3N | 32-bit required flag set | Apply corflags patch once on FXF3N DLL |

## One-Time FXF3N Patch (if needed)

```powershell
corflags "deploy\FedEx.PABST.SS.Screens.FXF3N.dll" /32BITREQ- /Force
```

## Notes

1. The project currently uses FedEx_Emu mode, not Bluezone COM mode.
2. Timeout is configured in milliseconds and persisted as LastTimeout in user settings.
3. Connection diagnostics are written to connect_error.txt in output directory on failed connect.
4. tn3270_dll.dll is copied beside the built executable, but the supported machine setup also keeps a copy at E:\fedex\tn3270\tn3270_dll.dll.
