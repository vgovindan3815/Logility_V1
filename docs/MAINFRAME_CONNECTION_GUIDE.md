# Logility Freight - Mainframe Connection Guide

This guide describes the current connection model used by this repository.

## Current Connection Mode

The application uses FedEx_Emu mode through:

- ScreenScraping.sslibTypeType.FedEx_Emu
- deploy\tn3270_dll.dll as the canonical source file in the repo
- Screenlayouts\ScreenLayouts.xml as the canonical source layout in the repo
- tn3270_dll.dll copied beside the built .exe in bin\Debug or bin\Release
- E:\fedex\tn3270\tn3270_dll.dll as the supported native runtime location on user machines

This build does not use Bluezone COM registration for app connection flow.

## Canonical File Locations

| File set | Source in repo | Build output location | Supported user-machine location |
|---|---|---|---|
| Managed FedEx DLLs | deploy\FedEx.PABST.SS.*.dll | bin\<Configuration>\*.dll | Keep in the same folder as Logility_Freight.exe |
| RSF file | deploy\fxf3270.rsf | bin\<Configuration>\fxf3270.rsf | Keep in the same folder as Logility_Freight.exe |
| Screen layout XML | Screenlayouts\ScreenLayouts.xml | bin\<Configuration>\ScreenLayouts.xml | Keep in the same folder as Logility_Freight.exe |
| Native tn3270 DLL | deploy\tn3270_dll.dll | bin\<Configuration>\tn3270_dll.dll | Keep in the same folder as Logility_Freight.exe and also at E:\fedex\tn3270\tn3270_dll.dll |

Use the scripts in this docs folder to enforce these paths:

- docs\Run-Tn3270Subst.bat: business-friendly launcher for tn3270 E: mapping setup
- docs\Run-LogilityRuntimeSetup.bat: business-friendly launcher for full runtime setup
- docs\Create-Tn3270Subst.ps1: creates the E: mapping and copies tn3270_dll.dll to E:\fedex\tn3270\tn3270_dll.dll
- docs\Setup-LogilityFreightRuntime.ps1: creates a user-machine runtime folder, copies the runtime files, and runs the tn3270 subst setup

## Quick Setup Checklist

1. Confirm required managed DLLs are in deploy\:
   - FedEx.PABST.SS.SSLib.dll
   - FedEx.PABST.SS.Exceptions.dll
   - FedEx.PABST.SS.Screens.FXF3A.dll
   - FedEx.PABST.SS.Screens.FXF3B.dll
   - FedEx.PABST.SS.Screens.FXF3C.dll
   - FedEx.PABST.SS.Screens.FXF3D.dll
   - FedEx.PABST.SS.Screens.FXF3E.dll
   - FedEx.PABST.SS.Screens.FXF3F.dll
   - FedEx.PABST.SS.Screens.FXF3G.dll
   - FedEx.PABST.SS.Screens.FXF3J.dll
   - FedEx.PABST.SS.Screens.FXF3K.dll
   - FedEx.PABST.SS.Screens.FXF3M.dll
   - FedEx.PABST.SS.Screens.FXF3N.dll
   - FedEx.PABST.SS.Screens.FXF4M.dll

2. Confirm runtime source files are present in the repo:
   - deploy\fxf3270.rsf
   - deploy\tn3270_dll.dll
   - Screenlayouts\ScreenLayouts.xml

3. Create the supported tn3270 runtime mapping:

Business user launcher:

```powershell
.\docs\Run-Tn3270Subst.bat
```

Support/advanced PowerShell option:

```powershell
.\docs\Create-Tn3270Subst.ps1
```

4. For a full user-machine runtime setup, copy the app runtime and configure tn3270 in one step:

Business user launcher:

```powershell
.\docs\Run-LogilityRuntimeSetup.bat
```

Optional custom install path:

```powershell
.\docs\Run-LogilityRuntimeSetup.bat D:\Logility_Freight
```

Support/advanced PowerShell option:

```powershell
.\docs\Setup-LogilityFreightRuntime.ps1 -InstallRoot 'C:\Logility_Freight'
```

5. Verify mapping:

```powershell
subst
Test-Path E:\fedex\tn3270\tn3270_dll.dll
```

Test-Path must return True.

6. Verify the application runtime folder contains:
   - Logility_Freight.exe
   - FedEx.PABST.SS.SSLib.dll
   - FedEx.PABST.SS.Exceptions.dll
   - FedEx.PABST.SS.Screens.FXF3*.dll / FXF4M.dll set
   - tn3270_dll.dll
   - fxf3270.rsf
   - ScreenLayouts.xml

## Build and Run

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' FXF3A_Tool.vbproj /p:Configuration=Debug /t:Rebuild /v:minimal
.\bin\Debug\Logility_Freight.exe
```

Release executable:

- bin\Release\Logility_Freight.exe

## Connection Fields

Use these values in the top connection bar / login form:

- Host: TN3270 host:port
- System: FDXF
- UID T: terminal user ID
- UID L: Logility user ID
- Screen layout path: usually auto-filled to output ScreenLayouts.xml
- Timeout: milliseconds, default 30000

Timeout guidance:

- 30000 for normal network
- 60000 for high-latency VPN or unstable route

## Important Runtime Notes

1. Timeout is in milliseconds end-to-end and passed directly to ScreenScraping constructor.
2. The app persists timeout in user config as LastTimeout.
3. Connection errors are written to:
   - bin\Debug\connect_error.txt (Debug)
   - bin\Release\connect_error.txt (Release)

## Common Failures and Fixes

| Symptom | Likely cause | Action |
|---|---|---|
| Unable to login to mainframe session | Credentials, host/port, or mainframe availability | Recheck host, IDs, passwords, and CICS status |
| Freight not connected during login - 1 | tn3270 runtime not initialized correctly | Re-run docs\Run-Tn3270Subst.bat and verify E:\fedex\tn3270\tn3270_dll.dll |
| Screen layout XML file was not found | Wrong layout path | Point to the ScreenLayouts.xml kept beside Logility_Freight.exe |
| Long delay then failure | Network latency/firewall drop | Increase timeout to 60000 and verify port path with IT |
| Could not load file or assembly ... incorrect format | DLL architecture mismatch | Re-copy the managed DLL set from deploy\ and reapply FXF3N corflags fix if needed |

## FXF3N One-Time Patch

If FXF3N fails to load in x64 process, apply once on that machine:

```powershell
corflags "deploy\FedEx.PABST.SS.Screens.FXF3N.dll" /32BITREQ- /Force
```

Expected verification:

- 32BITREQ : 0

## Operational Validation

After successful connect:

1. Open FXF3A.
2. Add a row with ACTION=GET, CARRIER=FXFM, CUST TYPE=CC, known ACCOUNT.
3. Run batch.
4. Confirm STATUS success and returned data.

If login is successful on mainframe but app reports failure, inspect connect_error.txt and verify timeout, the runtime folder contents, and the tn3270 mapping first.
