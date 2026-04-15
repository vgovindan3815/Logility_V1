# FXF3A Mainframe Tool — Project Context

## What this project is

A WPF desktop application targeting **.NET Framework 4.8** written in **VB.NET**.
It wraps the FedEx Freight (FDXF) CICS screen scraping library and provides a
multi-screen batch operations UI for the Logility pricing system.

The app drives a **Bluezone 3270 terminal emulator** via the `BZWHLLLIB` COM API.
The emulator session is created and held open for the lifetime of the app.

---

## Technology stack — non-negotiable constraints

| Item | Value |
|---|---|
| Language | VB.NET |
| Framework | .NET Framework 4.8 (NOT .NET 5/6/7/8) |
| UI | WPF (Windows Presentation Foundation) |
| Pattern | MVVM with `INotifyPropertyChanged` |
| IDE | VSCode + MSBuild (no Visual Studio required) |
| Target OS | Windows x64 only |
| Emulator | Bluezone (BZWHLLLIB COM API) |

**Never suggest migrating to .NET 5+ or to C#. Always stay on .NET Framework 4.8 VB.NET.**

---

## Solution structure

```
FXF3A_Tool/
├── FXF3A_Tool.vbproj            ← WPF app project
├── App.xaml / App.xaml.vb
│
├── Core/
│   ├── SessionManager.vb        ← owns ScreenScraping object, Connect/Disconnect
│   ├── BaseViewModel.vb         ← INotifyPropertyChanged base
│   ├── RelayCommand.vb          ← ICommand implementation
│   └── Exceptions.vb            ← typed exception helpers
│
├── Models/                      ← per-screen ViewModel row models
│   ├── BatchRowBase.vb          ← BatchRowBase + BatchRowWithPart base classes
│   ├── FXF3A_BatchRow.vb
│   ├── FXF3BtoG_BatchRows.vb   ← FXF3B through FXF3G batch rows (combined file)
│   └── FXF3JtoN_4M_BatchRows.vb ← FXF3J, FXF3K, FXF3M, FXF3N, FXF4M batch rows
│
├── ViewModels/
│   ├── MainViewModel.vb         ← shell nav, session state
│   ├── LoginViewModel.vb        ← connection form
│   ├── FXF3A_ViewModel.vb
│   ├── FXF3B_ViewModel.vb
│   ├── FXF3C_ViewModel.vb
│   ├── FXF3D_ViewModel.vb
│   ├── FXF3E_ViewModel.vb
│   ├── FXF3F_ViewModel.vb
│   └── FXF3G_ViewModel.vb
│
├── Views/
│   ├── MainWindow.xaml          ← shell: nav rail + content area
│   ├── LoginView.xaml           ← connection panel
│   ├── FXF3A_View.xaml
│   ├── FXF3B_View.xaml
│   ├── FXF3C_View.xaml
│   ├── FXF3D_View.xaml
│   ├── FXF3E_View.xaml
│   ├── FXF3F_View.xaml
│   └── FXF3G_View.xaml
│
├── Resources/
│   └── Styles.xaml              ← shared colours, button styles
│
└── deploy/                      ← FedEx DLLs go here (not in git)
    ├── FedEx.PABST.SS.SSLib.dll
    ├── FedEx.PABST.SS.Screens.FXF3A.dll
    ├── FedEx.PABST.SS.Screens.FXF3B.dll
    ├── FedEx.PABST.SS.Screens.FXF3C.dll
    ├── FedEx.PABST.SS.Screens.FXF3D.dll
    ├── FedEx.PABST.SS.Screens.FXF3E.dll
    ├── FedEx.PABST.SS.Screens.FXF3F.dll
    ├── FedEx.PABST.SS.Screens.FXF3G.dll
    ├── FedEx.PABST.SS.Exceptions.dll
    ├── Interop.BZWHLLLIB.dll
    └── fxf3270.rsf
```

---

## FedEx screen scraping library — key facts

### Namespace
```
FedEx.PABST.SS.SSLib.ScreenScraping      ← session object
FedEx.PABST.SS.Screens.FXF3A            ← screen classes
FedEx.PABST.SS.Screens.FXF3B
... etc
FedEx.PABST.SS.Exceptions               ← typed exceptions
```

### Session creation pattern
```vb
' Create session — this connects Bluezone and logs into CICS
Dim ss As New ScreenScraping(
    ScreenScraping.sslibTypeType.Bluezone,
    hostConnectionString,       ' e.g. "hostname:23"
    screenLayoutXMLPath,        ' path to fxf3270.rsf
    waitTimeoutMs,              ' e.g. 30000
    mfSystem,                   ' e.g. "FDXF"
    mfUserId_t,                 ' terminal user ID
    mfUserId_l,                 ' logility user ID (often same)
    mfPassword_t,               ' terminal password
    mfPassword_l,               ' logility password
    ScreenScraping.connectionType.FREIGHT,
    visible:=True)

' Create screen objects — one per screen, all share same ss
Dim fxf3a As New FXF3A(ss)
Dim fxf3b As New FXF3B(ss)
' ... etc

' Disconnect
ss.Close()
If ss.SSProcess IsNot Nothing Then ss.SSProcess.Kill()
```

### Exceptions to always handle
```vb
FedEx.PABST.SS.Exceptions.AccountNotFoundException
FedEx.PABST.SS.Exceptions.NoDiscountRecordsException
FedEx.PABST.SS.Exceptions.NumericValueException
FedEx.PABST.SS.Exceptions.GenericScreenScraperException  ' has .ScreenDump property
```

### Key enums (in ScreenScraping namespace)
```
fxfCarrierEnum:   ARFW, VIKN, FXFM, FXNL, FXFC_INTRACANADA, FXFC_CROSSBORDER, DFLT
fxfCustTypeEnum:  CC, CN, NC, NN, EN, FA, SG, GR, CY, DV, GE, NA
fxfActionIndEnum: NA, A, C, D, I, N, P, R, M
fxfInterEnum:     NA, I, S, B
fxfTypeHaulEnum:  NA, SingleLine, JointLineToCarrier, JointLineFromCarrier
fxfGeoDirEnum:    NA, F, T, B, A, X
fxfGeoTypeEnum:   NA, AC, AL, CC, CN, CO, NC, NN, PC, ST, TE, ZR, GG
fxfClassZipEnum:  NA, B, C, D, N, S, X, C_3, C_5
fxfGenGeoAltrEnum:NA, Y, N, X, A, Q
fxfCurrencyEnum:  USD, CDN, CAD
fxfPrepaidOrCollectEnum: NA, C, P, T
fxfAlternationEnum: NA, K, L, N
fxfAppRuleEnum:   NA, A, B, C, M, C_3, C_5, U
fxfPlusMinusEnum: NA, Plus, Lane, Minus
fxfRateTypeEnum:  NA, R, M, F, D, DF, ED, EF
```

---

## Screen inventory — what each FXF3x screen does

| Screen | CICS Name | Business Purpose | Has getItems? | Has cancelItem? | Extra key field |
|--------|-----------|-----------------|--------------|----------------|-----------------|
| FXF3A | 3A | Customer discount items (main) | Yes | Yes | — |
| FXF3B | DSNM1ST-3B | Discounts by State/Terminal only | No | No | Part |
| FXF3C | DSNM1GE-3C | Customer geography discounts | No | No | Part |
| FXF3D | DSNM1PR-3D | Customer product discounts | No | Yes | Part |
| FXF3E | DSNM2MB-3E | Customer rates | No | Yes | Part |
| FXF3F | DSNM3PP-3F | Customer discounts/adjustments | No | Yes | Part |
| FXF3G | DSNM1AP-3G | Customer charges/allowances | No | Yes | Part |
| FXF3J | DSNM1NC-3J | Copy customer/national account | No | No | From/To custClass |
| FXF3K | DSNM1BM-3K | State matrix | No | Yes (cancelMatrix) | MatrixName+EffDate |
| FXF3M | DSNM1H3-3M | Handling unit allowance | No | Yes | Part |
| FXF3N | DSNM1HR-3N | Unit rates | No | Yes | Part |
| FXF4M | DSNM1D0-4M | Earned discount | Yes (getItems) | Yes | Part=payRule |

**Important notes:**
- FXF3B through FXF3N all have a `Part`/`payRule` key field in addition to Authority/Number/Item.
- FXF3J uses `copyAcct(parmClass)` not standard CRUD — `parmClass.fromCust`/`toCust` are `custClass` objects.
- FXF3K uses `getMatrix`/`changeMatrix`/`addMatrix`/`deleteMatrix`/`cancelMatrix` (not getItem/addItem).
- FXF4M `itemHeaderClass` uses `payRule` (not `part`) and `auhority` (typo in source, must match exactly).
- `cancelItem` 4th optional param is named `payRule` in actual source (maps to the Part/payRule field).

**changeItem signature differences (critical):**
- FXF3B, FXF3E, FXF3G, FXF3M: `changeItem(carrier, custType, account, authority, number, item, part, pItemObj)` — separate key params
- FXF3C, FXF3D, FXF3F, FXF3N: `changeItem(carrier, custType, account, pItemObj)` — keys pulled from pItemObj.itemHeader

**addItem pRelease differences:**
- FXF3A, FXF3B, FXF3G, FXF3M: `addItem(..., pItemObj, Optional pRelease = False)` — HAS pRelease
- FXF3C, FXF3D, FXF3E, FXF3F, FXF3N: `addItem(..., pItemObj)` — NO pRelease

---

## MVVM conventions

- ViewModels inherit `BaseViewModel` which implements `INotifyPropertyChanged`
- Commands use `RelayCommand` (parameterless) or `RelayCommand(Of T)` (typed)
- `SessionManager` is a shared singleton — injected into all ViewModels
- The `MainViewModel` holds the active screen ViewModel in `CurrentView` property
- Views use `DataContext` binding, no code-behind except event wiring
- All screen operations run on a background `Task` with `Await` to keep UI responsive
- Use `Application.Current.Dispatcher.InvokeAsync` to update UI from background threads
- `ObservableCollection(Of T)` for all DataGrid sources

---

## Coding standards for this project

- Always use `Option Strict On` and `Option Explicit On` at top of every file
- Use `Async/Await` for all screen scraping calls (they block on terminal I/O)
- Wrap all screen scraping calls in Try/Catch — never let unhandled exceptions crash the app
- Use named constants for enum values — never hardcode integer ordinals
- Date formatting for mainframe: always `MM/dd/yy`
- NULL sentinel values from the library: `ScreenScraping.NULL_STRING`, `ScreenScraping.NULL_DATE`, `ScreenScraping.NULL_INT`, `ScreenScraping.NULL_DEC`
- When checking if a date field is empty: `If d = ScreenScraping.NULL_DATE`
- Passwords: use `SecureString` or at minimum clear from memory after use — never store in a field

---

## Build commands (MSBuild — no Visual Studio needed)

Use the .NET Framework 4.8 MSBuild (NOT `dotnet msbuild`). `dotnet msbuild` lacks the
WinFX targets needed for MarkupCompilePass1 (XAML → .g.vb generation).

```powershell
# Build (Debug)
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' FXF3A_Tool.vbproj /p:Configuration=Debug /t:Rebuild

# Build (Release)
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' FXF3A_Tool.vbproj /p:Configuration=Release /t:Rebuild

# Run (after build)
bin\Debug\FXF3A_Tool.exe
```

**IMPORTANT — VB language version constraints:**
The VBC at that path supports only up to Visual Basic 2012 (VB11). Do NOT use:
- `$"..."` string interpolation → use `String.Format("...", args)` instead
- `NameOf(X)` → use `"X"` string literal instead
- `Public ReadOnly Property X As New Y` (ReadOnly auto-prop with init) → use `Public Property X As New Y` or explicit backing field + Get
- `Public ReadOnly Property X As Y` (ReadOnly auto-prop without init) → requires explicit backing field + Get block
- Tuple literals `(a, b, c)` → use `Tuple.Create(a, b, c)` with typed variable

---

## What NOT to do

- Do not use .NET Core / .NET 5+ APIs
- Do not use `async void` except in event handlers
- Do not use MessageBox in ViewModels — raise an event or use a dialog service
- Do not access ScreenScraping from the UI thread — always wrap in Task.Run
- Do not store passwords in any ViewModel property or settings file
- Do not use global static variables for the session — use SessionManager
