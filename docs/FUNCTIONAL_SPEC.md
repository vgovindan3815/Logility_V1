# FXF3A Tool — Functional Specification
# Version 1.0 | For Claude Sonnet 4.6 implementation

---

## 1. Application overview

A WPF desktop application for FedEx Freight pricing analysts to perform batch
CRUD operations against the FDXF CICS Logility pricing system via FedEx_Emu
TN3270 runtime. The application targets screens FXF3A through FXF4M.

---

## 2. Main window layout

```
┌─────────────────────────────────────────────────────────────────┐
│  ● FXF3A Mainframe Tool          [session status badge]    [─][□][✕]│
├──────────────┬──────────────────────────────────────────────────┤
│              │  [Connection bar — host, system, uid, connect btn] │
│  Nav Rail    ├──────────────────────────────────────────────────┤
│              │                                                    │
│  ● LOGIN     │           Content Area                            │
│  ─────────   │     (swaps UserControl based on nav selection)    │
│  FXF3A  ●   │                                                    │
│  FXF3B       │                                                    │
│  FXF3C       │                                                    │
│  FXF3D       │                                                    │
│  FXF3E       │                                                    │
│  FXF3F       │                                                    │
│  FXF3G       │                                                    │
│              │                                                    │
└──────────────┴──────────────────────────────────────────────────┘
```

- Nav rail is always visible, 180px wide, dark navy background
- Active screen is highlighted in purple
- Screens FXF3B–G are greyed out and show tooltip "FXF3A session required"
  when not connected
- Connection bar is a thin strip below the title bar, always visible
- Content area fills remaining space

---

## 3. Connection bar (always visible)

Fields (left to right, all in one row):
- **Host** TextBox — default "hostname:23"
- **System** TextBox — default "FDXF", width 80px
- **Terminal UID** TextBox
- **Logility UID** TextBox
- **Screen Layout XML** TextBox — default output `ScreenLayouts.xml`, wide
- **Timeout** TextBox — default "30000" (milliseconds), width 60px
- **CONNECT** Button — green when disconnected
- **DISCONNECT** Button — red, visible only when connected
- **Status badge** — circle indicator: grey=disconnected, green=connected, amber=connecting

On connect:
1. Disable all fields and CONNECT button
2. Show amber "Connecting..." badge
3. Open two `PasswordBox` dialogs in sequence (terminal pwd, logility pwd)
   — these are modal WPF Windows, NOT MessageBox
4. Call `SessionManager.ConnectAsync()`
5. On success: green badge, enable screens in nav
6. On failure: show error in a red banner below the connection bar,
   re-enable fields

Settings persistence: Host, System, Terminal UID, Logility UID, Screen Layout XML path,
Timeout are saved to `My.Settings` (not passwords — never).
Loaded automatically on startup.

---

## 4. Login / Welcome view (default content area view)

Shown when app starts. Contains:
- Logo area / title
- Brief instructions: "Connect using the bar above, then select a screen"
- List of screen names with one-line descriptions:
  - FXF3A — Customer Discount Items
  - FXF3B — Discounts by State/Terminal
  - FXF3C — Customer Geography Discounts
  - FXF3D — Customer Product Discounts
  - FXF3E — Customer Rates
  - FXF3F — Customer Discounts/Adjustments
  - FXF3G — Customer Charges/Allowances
- Version label bottom-right

---

## 5. Screen view pattern (same for FXF3A–G)

Each screen view follows identical layout:

```
┌─────────────────────────────────────────────────────────────────┐
│  FXF3A — Customer Discount Items                                 │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Carrier [▼] │ CustType [▼] │ Account [____] │ [Load ▶]   │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
│  ┌─ Batch Operations ─────────────────────────────────────────┐  │
│  │ [▶ Run Batch]  [+ Add Row]  [✖ Clear]  [⬇ Export CSV]     │  │
│  │ Progress: ████████░░░░  12/50 rows  Errors: 2              │  │
│  ├──────────────────────────────────────────────────────────┐ │  │
│  │ ACTION │ ACCOUNT │ AUTHORITY │ NUMBER │ ITEM │ ... │STATUS│ │  │
│  │ GET    │ 123456  │ FEDX      │ 100    │ 1    │ ... │  ✓   │ │  │
│  │ ADD    │ 789012  │ FEDX      │ 101    │ 1.5  │ ... │  ✗   │ │  │
│  └──────────────────────────────────────────────────────────┘ │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌─ Results ──────────────────────────────────────────────────┐  │
│  │ [▼ collapse]  89 rows  [⬇ Export CSV]  [✖ Clear]           │  │
│  │ Timestamp │ Account │ Authority │ Number │ Item │ ...       │  │
│  └────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.1 Quick Load bar
- Carrier ComboBox — values from `fxfCarrierEnum`, default FXFM
- CustType ComboBox — values from `fxfCustTypeEnum`, default CC
- Account TextBox
- **Load button**: calls `getItems` (FXF3A only) or navigates to account
  and loads the item list. Populates the batch grid with GET rows.

### 5.2 Batch grid columns

**All screens — common key columns:**

| Column | Type | Values / Notes |
|--------|------|---------------|
| ACTION | ComboBox | GET / ADD / CHANGE / CANCEL / DELETE |
| CARRIER | ComboBox | fxfCarrierEnum values |
| CUST_TYPE | ComboBox | fxfCustTypeEnum values |
| ACCOUNT | TextBox | customer account number |
| AUTHORITY | TextBox | tariff authority |
| NUMBER | TextBox | tariff number |
| ITEM | TextBox | item number (e.g. "1.00000") |
| PART | TextBox | **FXF3B–G only** — part identifier |
| RELEASE? | CheckBox | release after ADD/CHANGE/CANCEL |
| CANCEL_DATE | DatePicker | required for CANCEL action |
| STATUS | ReadOnly | ✓ OK / ✗ Error / ⏳ Running / — Skipped |

**FXF3A additional columns** (discount table — up to 3 rows inline):

| Column | Type |
|--------|------|
| DISC1, MIN_CHG1, MAX_WGT1, FLOOR_MIN1, EFF_DATE1, CAN_DATE1 | TextBox/DatePicker |
| DISC2, MIN_CHG2, MAX_WGT2, FLOOR_MIN2, EFF_DATE2, CAN_DATE2 | TextBox/DatePicker |
| DISC3, MIN_CHG3, MAX_WGT3, FLOOR_MIN3, EFF_DATE3, CAN_DATE3 | TextBox/DatePicker |
| CURRENCY, FS_AUTH, FS_NUM, FS_ITEM | TextBox |
| PREPD_IN, PREPD_OUT, COLL_IN, COLL_OUT, 3RD_PARTY | CheckBox |
| INTER | ComboBox | fxfInterEnum |
| TYPE_HAUL | ComboBox | fxfTypeHaulEnum |
| COUNTRY, MATRIX | TextBox |
| GEO_DIR1, GEO_TYPE1, GEO_NAME1, GEO_ST1, GEO_CTY1 | ComboBox/TextBox |
| GEO_DIR2, GEO_TYPE2, GEO_NAME2, GEO_ST2, GEO_CTY2 | ComboBox/TextBox |
| RATES_EFF | DatePicker |
| SRV_DY_LO, SRV_DY_HI | TextBox |
| CLS_ZIP | ComboBox | fxfClassZipEnum |
| CZ_AUTH, CZ_NUM, CZ_SEC | TextBox |
| APPLY_ARBS, EXC_CLS, EXC_MAX_W, GEN_GEO_A | CheckBox/ComboBox |
| MIN_WGT, MAX_WGT | TextBox |
| INC_EXEMPT, FAK | CheckBox |
| ED_AGG | TextBox |
| NMFC 50–500 (17 columns) | CheckBox |
| PAY_RULE1, PAY_RULE2 | TextBox |

**FXF3B additional columns** (geo table — up to 5 rows):
GEO_INC_EXC, GEO_DIR, GEO_TYPE, GEO_NAME, GEO_COUNTRY (repeated ×5)
Plus: FS_AUTH, FS_NUM, FS_ITEM, PREPD_IN, PREPD_OUT, COLL_IN, COLL_OUT,
RATE_EFF, CLS_ZIP, CZ_AUTH, CZ_NUM, CZ_SEC, EXC_CLS, EXC_MAX_W, GEN_GEO_A

**FXF3C additional columns**: GEO table rows (plusMinus, dir, type, name, state, country ×5)
Plus: SERV_DAYS_LO, SERV_DAYS_HI

**FXF3D additional columns**: EFF_DATE, CANCEL_DATE, EXC_CLS, EXC_MAX_W
Plus PROD table rows: TYPE, PRODUCT1, PRODUCT2, EXC_CLS (×5 rows)

**FXF3E additional columns**: CONDITION, PREPAID_COLLECT, EFF_DATE, CANCEL_DATE,
COMMENTS, ALTERNATION, CLASS_RATES, RATE_MANUALLY, CLS_TRF_AUTH, CLS_TRF_NUM,
CLS_TRF_SEC, RATE_EFF_DATE
Plus RATE table rows: WEIGHT, TYPE, AMOUNT (×10 rows)

**FXF3F additional columns**: CONDITION, PREPAID_COLLECT, EFF_DATE, CANCEL_DATE,
COMMENTS, APP_RULE
Plus RATE table rows: WEIGHT, DISC_ADJ_DIR, DISC_ADJ_UNITS, DISC_ADJ_TYPE, AMOUNT (×10 rows)

**FXF3G additional columns**: PREPAID_COLLECT, EFF_DATE, CANCEL_DATE, COMMENTS
Plus SCHG table rows: COND, DESC, MIN_WGT, MAX_WGT, TYPE, AMOUNT, MIN_AMT,
MAX_AMT, APP, COND_ID (×10 rows)

### 5.3 Batch operations

**Run Batch** iterates rows top to bottom. For each non-blank ACTION row:
1. Show ⏳ in STATUS, update progress bar
2. Call the appropriate screen method on a background Task
3. On success: show ✓ green in STATUS
4. On failure: show ✗ red in STATUS, show error text in STATUS cell tooltip,
   show a non-blocking notification banner (NOT MsgBox — app keeps running)
5. After all rows: show summary "X succeeded, Y failed" in banner

**GET behaviour:**
- FXF3A: If Auth+Num+Item are all filled → `getItem` → append to Results grid
- FXF3A: If any of Auth/Num/Item blank → `getItems` for account → append all to Results grid
- FXF3B–G: `getItem` with Part field → append to Results grid

**CANCEL behaviour:**
- CANCEL_DATE must be filled — validate before submitting
- If blank, default to today's date and show tooltip warning

### 5.4 Results panel

- Collapsible (chevron button)
- Shows all GET results — appends, never overwrites
- Columns: Timestamp, Screen, Action, Carrier, CustType + all item fields
- Export CSV button → `SaveFileDialog` → write all rows to CSV
- Clear button → confirm dialog → clears all results

---

## 6. SessionManager specification

```vb
' SessionManager is a singleton, shared across all ViewModels
Public Class SessionManager

    ' State
    Public ReadOnly Property IsConnected As Boolean
    Public ReadOnly Property StatusMessage As String

    ' Events — ViewModels subscribe to these
    Public Event ConnectionChanged(sender As Object, e As EventArgs)
    Public Event StatusChanged(sender As Object, e As String)

    ' Connect — runs on background thread
    Public Async Function ConnectAsync(
        host As String, xmlPath As String, system As String,
        uidT As String, uidL As String,
        pwdT As String, pwdL As String,
        timeoutMs As Integer) As Task(Of Boolean)

    ' Disconnect
    Public Sub Disconnect()

    ' Screen accessors — only valid after ConnectAsync returns True
    Public ReadOnly Property FXF3A As FedEx.PABST.SS.Screens.FXF3A
    Public ReadOnly Property FXF3B As FedEx.PABST.SS.Screens.FXF3B
    Public ReadOnly Property FXF3C As FedEx.PABST.SS.Screens.FXF3C
    Public ReadOnly Property FXF3D As FedEx.PABST.SS.Screens.FXF3D
    Public ReadOnly Property FXF3E As FedEx.PABST.SS.Screens.FXF3E
    Public ReadOnly Property FXF3F As FedEx.PABST.SS.Screens.FXF3F
    Public ReadOnly Property FXF3G As FedEx.PABST.SS.Screens.FXF3G

End Class
```

---

## 7. BaseViewModel and RelayCommand

```vb
' BaseViewModel — INotifyPropertyChanged base
Public MustInherit Class BaseViewModel
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(<CallerMemberName> Optional name As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub

    ' Shorthand: set field and raise changed event
    Protected Function SetField(Of T)(ByRef field As T, value As T,
        <CallerMemberName> Optional name As String = "") As Boolean
        If EqualityComparer(Of T).Default.Equals(field, value) Then Return False
        field = value
        OnPropertyChanged(name)
        Return True
    End Function
End Class

' RelayCommand — parameterless
Public Class RelayCommand : Implements ICommand
    Private ReadOnly _execute As Action
    Private ReadOnly _canExecute As Func(Of Boolean)

    Public Sub New(execute As Action,
                   Optional canExecute As Func(Of Boolean) = Nothing)
    Public Function CanExecute(parameter As Object) As Boolean _
        Implements ICommand.CanExecute
    Public Sub Execute(parameter As Object) Implements ICommand.Execute
    Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
    Public Sub RaiseCanExecuteChanged()
End Class

' RelayCommand(Of T) — typed parameter version
Public Class RelayCommand(Of T) : Implements ICommand
    ' same pattern with typed execute/canExecute
End Class
```

---

## 8. Colour palette and styles

Define all colours in `Resources/Styles.xaml` as resources:

| Resource key | Hex | Usage |
|---|---|---|
| NavyBrush | #0F1C3F | Nav rail, title bar |
| PurpleBrush | #460073 | Active nav item, section headers |
| AccentBrush | #A100FF | Accent highlights, badges |
| TealBrush | #00B0B9 | Connect button when connected |
| SuccessBrush | #065F46 | OK status, success badges |
| ErrorBrush | #7F1D1D | Error status, error banners |
| WarningBrush | #92400E | Warning banners |
| ConnectBrush | #1A7A40 | CONNECT button |
| DisconnectBrush | #AA2222 | DISCONNECT button |
| LightGrey | #F0F2F5 | Row alternating bg |
| GridHeaderBrush | #1A2E5A | DataGrid column headers |

Button styles: ConnectButtonStyle, DisconnectButtonStyle, RunButtonStyle,
NavItemStyle (for nav rail items), DataGridHeaderStyle.

---

## 9. Error handling rules

1. All screen scraping calls are wrapped in Try/Catch
2. `AccountNotFoundException` → show "Account {n} not found" in STATUS
3. `NoDiscountRecordsException` → show "No discount records" in STATUS (not an error — yellow)
4. `NumericValueException` → show "Invalid cancel date range" in STATUS
5. `GenericScreenScraperException` → show error message in STATUS; expose `.ScreenDump`
   via an expandable detail panel or copy-to-clipboard button in the STATUS tooltip
6. Any other Exception → show "Unexpected error: {message}" in STATUS
7. Never use `MessageBox.Show` in ViewModels — raise a notification event instead
8. The UI shows a non-blocking notification banner at the top of the content area
9. Errors do not stop the batch — processing continues to the next row

---

## 10. CSV export format

```
Timestamp,Screen,Action,Carrier,CustType,Account,Authority,Number,Item[,Part],
  [screen-specific fields...],Status,ErrorDetail
```

Dates formatted as `MM/dd/yy`. Booleans as `Y`/`N`.
Quote fields that contain commas.

---

## 11. Settings persistence (My.Settings)

Save (non-sensitive only):
- `LastHost` As String
- `LastSystem` As String
- `LastUidT` As String
- `LastUidL` As String
- `LastRsfPath` As String
- `LastTimeout` As Integer = 30000
- `WindowLeft`, `WindowTop`, `WindowWidth`, `WindowHeight` As Double
- `NavRailWidth` As Double = 180

Load all on `Application.Startup`.
Save all on `Application.Exit`.

---

## 12. Build and deployment

Build output: `bin\Release\Logility_Freight.exe`

Canonical source files in the repo:
- `deploy\FedEx.PABST.SS.SSLib.dll`
- `deploy\FedEx.PABST.SS.Exceptions.dll`
- `deploy\FedEx.PABST.SS.Screens.*.dll`
- `deploy\tn3270_dll.dll`
- `deploy\fxf3270.rsf`
- `Screenlayouts\ScreenLayouts.xml`

Deployment folder on the user machine (all in same directory as .exe):
- `Logility_Freight.exe`
- All FedEx DLLs (copied by MSBuild `Private=True`)
- `tn3270_dll.dll` (copied by MSBuild `Content` item)
- `fxf3270.rsf` (copied by MSBuild `Content` item)
- `ScreenLayouts.xml` (copied by MSBuild `Content` item)

No installer needed — xcopy deployment.
FedEx_Emu TN3270 runtime requirements must be satisfied on the target machine.

Native tn3270 supported machine location:
- `E:\fedex\tn3270\tn3270_dll.dll`

Supported setup scripts:
- `docs\Run-Tn3270Subst.bat` business-friendly launcher for tn3270 mapping
- `docs\Run-LogilityRuntimeSetup.bat` business-friendly launcher for full runtime setup
- `docs\Create-Tn3270Subst.ps1` creates the `E:` mapping and copies `tn3270_dll.dll`
- `docs\Setup-LogilityFreightRuntime.ps1` creates a runtime folder, copies the app dependencies, layout XML, RSF, tn3270 DLL, and runs the subst setup
