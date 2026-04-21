# FXF3A Tool — Backend Workflow Technical Reference

## Table of Contents
1. [Connect to Mainframe](#1-connect-to-mainframe)
2. [FXF3A Screen — Customer Discount Items](#2-fxf3a-screen--customer-discount-items)

---

## 1. Connect to Mainframe

### 1.1 User-Facing Entry Points

There are two paths through the UI that trigger a connection, both converging on the same
shared connect logic:

| Path | Trigger | Where handled |
|---|---|---|
| **Login View (full form)** | User fills fields, clicks "Connect" in `LoginView.xaml` | `LoginView.xaml.vb` code-behind reads PasswordBox values → calls `LoginViewModel.ConnectWithPasswords(pwdT, pwdL)` |
| **Top-bar quick connect** | Connect icon/button in the main toolbar | `LoginViewModel.ExecuteConnect()` raises `RequestPassword` events → view shows `PasswordDialog` modal × 2 → callback returns each password |

### 1.2 Pre-Connect Validation

Before any network or mainframe call is made, `CanConnect` gates the button:

```
CanConnect = NOT IsConnected AND NOT IsBusy
             AND Host is not blank
             AND RsfPath is not blank
```

`ExecuteConnect` / `ConnectWithPasswords` also validate Terminal Password is non-empty before
proceeding. If any check fails an `ErrorBanner` string is raised on the UI (no dialog boxes).

### 1.3 Settings Loaded at Startup

When the application launches, `LoginViewModel.LoadSettings()` restores the last-used
non-sensitive values from `My.MySettings.Default` (stored in the user-profile `user.config`):

| Setting key | Maps to field | Default |
|---|---|---|
| `LastHost` | `Host` | `"hostname:23"` |
| `LastSystem` | `SystemCode` | `"FDXF"` |
| `LastUidT` | `UidT` (terminal user ID) | `""` |
| `LastUidL` | `UidL` (Logility user ID) | `""` |
| `LastRsfPath` | `RsfPath` | `"C:\FXF\fxf3270.rsf"` |
| `LastTimeout` | `Timeout` (ms) | `30000` |

**Passwords are never persisted** — they exist only as local string variables during the
connect call and are overwritten before going out of scope.

### 1.4 Connect Execution Flow

```
LoginViewModel.DoConnectAsync(pwdT, pwdL)
│
├─ IsBusy = True  (disables Connect button, shows spinner)
│
├─ Await SessionManager.ConnectAsync(
│      host, rsfPath, systemCode, uidT, uidL,
│      pwdT, pwdL, timeoutMs)
│
│   [Runs on background Task.Run — does NOT block UI thread]
│   │
│   ├─ Raises StatusChanged: "Connecting to <host>..."
│   │
│   ├─ new ScreenScraping(
│   │      sslibTypeType.FedEx_Emu,  ← emulator type
│   │      host,                     ← e.g. "mfhost.example.com:23"
│   │      xmlPath,                  ← path to fxf3270.rsf screen layout
│   │      timeoutMs,                ← e.g. 30000 ms
│   │      system,                   ← e.g. "FDXF"
│   │      uidT,                     ← terminal RACF/CICS user ID
│   │      uidL,                     ← Logility user ID
│   │      pwdT,                     ← terminal password (in-memory only)
│   │      pwdL,                     ← Logility password (in-memory only)
│   │      connectionType.FREIGHT,   ← FedEx Freight connection type
│   │      visible := True)          ← Bluezone window visible
│   │
│   │   *** ScreenScraping constructor BLOCKS until TN3270 login completes ***
│   │   Bluezone opens, connects to host:23, sends RACF credentials,
│   │   navigates through CICS sign-on screens, reaches the Logility
│   │   pricing system menu. Returns when the session is ready.
│   │
│   ├─ Instantiate all 12 screen objects (all share same _ss session):
│   │      _fxf3a = New FXF3A(_ss)
│   │      _fxf3b = New FXF3B(_ss)
│   │      _fxf3c = New FXF3C(_ss)
│   │      _fxf3d = New FXF3D(_ss)
│   │      _fxf3e = New FXF3E(_ss)
│   │      _fxf3f = New FXF3F(_ss)
│   │      _fxf3g = New FXF3G(_ss)
│   │      _fxf3j = New FXF3J(_ss)
│   │      _fxf3k = New FXF3K(_ss)
│   │      _fxf3m = New FXF3M(_ss)
│   │      _fxf3n = New FXF3N(_ss)
│   │      _fxf4m = New FXF4M(_ss)
│   │
│   ├─ _isConnected = True
│   ├─ Raises StatusChanged: "Connected — FDXF @ host"
│   └─ Raises ConnectionChanged event
│
├─ [On success] SaveSettings() → persists non-sensitive fields to user.config
│
├─ [On failure] ErrorBanner = "Connection failed. Check credentials and host."
│               SessionManager.CleanupSession() nulls all screen objects
│
└─ Finally: overwrite passwords in memory
       pwdT = New String("X"c, 10) : pwdT = ""
       pwdL = New String("X"c, 10) : pwdL = ""
   IsBusy = False
```

### 1.5 Connection State Propagation

After `ConnectionChanged` fires, the following UI updates occur automatically via
property-change bindings:

- `LoginViewModel.IsConnected` notifies → Login View shows/hides connected state
- All 12 screen ViewModels' `RunBatchCommand` and `LoadAccountCommand` re-evaluate
  `CanExecute` (they require `IsConnected = True`)
- The top-bar Disconnect button becomes enabled; Connect button becomes disabled

### 1.6 Disconnect Flow

```
LoginViewModel.ExecuteDisconnect()
└─ SessionManager.Disconnect()
   ├─ Raises StatusChanged: "Disconnecting..."
   ├─ CleanupSession():
   │    ├─ Nulls all 12 screen objects (_fxf3a … _fxf4m = Nothing)
   │    ├─ _ss.Close()          ← graceful CICS sign-off
   │    └─ _ss.SSProcess.Kill() ← terminate Bluezone process
   ├─ _isConnected = False
   ├─ Raises StatusChanged: "Disconnected"
   └─ Raises ConnectionChanged event
```

---

## 2. FXF3A Screen — Customer Discount Items

**CICS screen:** FXF3A (Customer Discount Items — main discount header)  
**Library class:** `FedEx.PABST.SS.Screens.FXF3A`  
**ViewModel:** `FXF3A_ViewModel`  
**Batch row model:** `FXF3A_BatchRow`

### 2.1 Data Model — FXF3A_BatchRow

Each row in the DataGrid maps to one FXF3A record on the mainframe. The model is a
flattened view of `FXF3A.itemClass`:

#### Identity / Key Fields (required for all operations)

| Property | Excel / CSV Column | Mainframe Field | Description |
|---|---|---|---|
| `Action` | ACTION | — | Operation code: GET, ADD, CHANGE, CANCEL, DELETE, RELEASE |
| `Carrier` | CARRIER | carrier (fxfCarrierEnum) | e.g. FXFM, ARFW, FXNL |
| `CustType` | CUST_TYPE | custType (fxfCustTypeEnum) | e.g. CC, CN, NC, NN |
| `Account` | ACCOUNT | account | Customer account number |
| `Authority` | AUTHORITY | itemHeader.auhority | Authority code (note: intentional typo in library) |
| `Number` | NUMBER | itemHeader.number | Item number |
| `Item` | ITEM | itemHeader.item | Item sub-number |
| `Release` | RELEASE? | pRelease (Boolean) | Whether to release the item after ADD/CHANGE |

#### Discount Table Fields (up to 3 discount rows, flattened)

| Properties | Description |
|---|---|
| `Disc1`, `EffDate1`, `CanDate1` | Discount %, effective date, cancel date — row 1 |
| `Disc2`, `EffDate2`, `CanDate2` | Discount row 2 (optional) |
| `Disc3`, `EffDate3`, `CanDate3` | Discount row 3 (optional) |
| `MinChg1/2/3` | Minimum charge discount per row |
| `MaxWgt1/2/3` | Maximum weight per row |
| `FloorMin1/2/3` | Floor minimum per row |

Dates are formatted `MM/dd/yy`. A discount row is omitted from the built discount table if
both `Disc` and `EffDate` are blank.

#### Item Attribute Fields

| Property | Mainframe Field | Type / Enum |
|---|---|---|
| `Currency` | currency | fxfCurrencyEnum (USD, CDN) |
| `Inter` | inter | fxfInterEnum (NA, I, S, B) |
| `TypeHaul` | typeHaul | fxfTypeHaulEnum (NA, SingleLine, JointLineToCarrier, JointLineFromCarrier) |
| `Matrix` | matrix | String — state matrix name |
| `GeoDir1/2` | geoDir1/2 | fxfGeoDirEnum (NA, F, T, B, A, X) |
| `GeoType1/2` | geoType1/2 | fxfGeoTypeEnum (NA, AC, AL, CC, CN, CO, NC, NN, PC, ST, TE, ZR, GG) |
| `GeoName1/2` | geoName1/2 | String |
| `ClsZip` | classZip | fxfClassZipEnum |
| `ApplyArbs` | applyArbs | Boolean |
| `Fak` | fak | Boolean |
| Various boolean flags | prepaidInbound, collectOutbound, thirdParty, etc. | Boolean |
| NMFC class flags | nmfc50 … nmfc500 (18 flags) | Boolean |

#### Read-Only Fields (populated only by GET)

| Property | Description |
|---|---|
| `LastMaintDate` | Date last maintained on mainframe |
| `OperatorId` | User ID that last modified the record |
| `Revision` | Revision number |
| `Status` | OperationStatus enum: Pending, Running, Success, Warning, Error, Skipped |
| `StatusMessage` | Error or warning message text |

### 2.2 Data Loading Paths (No Connection Required)

There are three ways to populate the FXF3A batch grid before running:

#### Path A — Import from CSV (`LoadCsvCommand`)
1. `OpenFileDialog` filtered to `*.csv`
2. Reads all lines; row 1 = header, row 2+ = data
3. `CsvHelper.BuildHeaderMap(lines(0))` — case-insensitive column name map
4. For each line: `CsvHelper.SplitLine(line)` → field array → mapped to `FXF3A_BatchRow`
5. Rows appended to `BatchRows` (existing rows are NOT cleared)

#### Path B — Load from Excel (`LoadAllFromExcelCommand` on MainViewModel)
1. `OpenFileDialog` filtered to `*.xlsx;*.xlsm`
2. `ExcelLoader.LoadSheet(path, "FXF3A_Batch", headerRowIndex:=1)` is called
   - Uses `Microsoft.ACE.OLEDB.16.0` provider (`HDR=NO;IMEX=1`)
   - Row 0 = title row (ignored); Row 1 (0-based) = header row; Row 2+ = data rows
   - Column names normalised: `.ToUpper().Replace("_","").Replace("?","")` → `CUST_TYPE` → `CUSTTYPE`
   - Stops at first row where ACTION cell is blank
   - Returns `List(Of Dictionary(Of String, String))`
3. `FXF3A_ViewModel.LoadRows(rows)` is called:
   - **Clears** `BatchRows` first
   - Maps dictionary keys (normalised) to `FXF3A_BatchRow` properties
   - Adds rows with `IsSelected = False`

**Excel sheet expected format:**

| Row | Content |
|---|---|
| Row 1 | Title / description text (ignored by loader) |
| Row 2 | Column headers: ACTION, CARRIER, CUST_TYPE, ACCOUNT, AUTHORITY, NUMBER, ITEM, RELEASE?, DISC1, EFF_DATE1, CAN_DATE1, DISC2, EFF_DATE2, CAN_DATE2, DISC3, EFF_DATE3, CAN_DATE3, CURRENCY, INTER, TYPE_HAUL, MATRIX, GEO_DIR1, GEO_TYPE1, GEO_NAME1 |
| Row 3+ | Data rows. Reading stops at first blank ACTION cell. |

#### Path C — Quick Load from Mainframe (`LoadAccountCommand`)
1. Requires `IsConnected = True` and `QuickAccount` not blank
2. Runs on background `Task.Run`
3. Calls `_session.FXF3A.getItems(carrier, custType, account, activeOnly:=True)`
4. Returns a list of item headers (`itemHeaderClass` list)
5. For each header: creates a `FXF3A_BatchRow` with `Action = "GET"` and partial discount data
6. **Clears** `BatchRows` and adds the new rows on the UI dispatcher thread
7. `BannerMessage` shows count of items loaded or "Account not found" error

### 2.3 Run Batch Flow

```
ExecuteRunBatch()   [Async Sub — runs on UI thread, work dispatched to background]
│
├─ Collect selected rows:
│    selectedRows = BatchRows.Where(r.IsSelected = True)
│
├─ IsBusy = True, ProgressTotal = selectedRows.Count
│
├─ For each row in selectedRows:
│    ├─ ProgressCurrent = i + 1
│    ├─ ProgressText = "Row N/Total — ACTION ACCOUNT"
│    ├─ If Action is blank → Status = Skipped, Continue
│    ├─ row.Status = Running
│    └─ Await Task.Run(Sub() ProcessRow(row))
│         [ProcessRow runs on background thread]
│
└─ ProgressText / BannerMessage = "Complete — X OK, Y errors, Z skipped"
   IsBusy = False
```

### 2.4 ProcessRow — Per-Operation Details

`ProcessRow` runs on a **background thread**. All UI updates inside it use
`Application.Current.Dispatcher.InvokeAsync`.

Enum values in the row model (string) are parsed to typed enums via `[Enum].Parse` before
being passed to the library.

---

#### GET — Read record(s) from mainframe

**Single item GET** (when Authority + Number + Item are all provided):
```
_session.FXF3A.getItem(carrier, custType, account, authority, number, item)
→ Returns: FXF3A.itemClass
→ Action: FXF3A_BatchRow.FromItemClass(itemClass) → populates all fields
→ Adds resulting row to Results collection
```

**All items GET** (when item key is partial/blank):
```
_session.FXF3A.getItems(carrier, custType, account, activeOnly:=True)
→ Returns: List of itemHeaderClass (summary list only)
→ For each header: calls getItem(...) to get full detail
→ Adds each fully-populated FXF3A_BatchRow to Results collection
```

The `Results` collection is separate from `BatchRows` — it accumulates GET results across
multiple batch runs and is exportable to CSV.

---

#### ADD — Create new record

```
row.ToItemClass() builds FXF3A.itemClass:
  ├─ itemHeader.auhority = row.Authority
  ├─ itemHeader.number   = row.Number
  ├─ itemHeader.item     = row.Item
  ├─ itemHeader.discTable = BuildDiscTable()   ← FXF3A.DiscCollection with up to 3 rows
  ├─ currency, inter, typeHaul, matrix, ...
  ├─ geoDir1/2, geoType1/2, geoName1/2, ...
  ├─ 18 NMFC class flags (nmfc50 … nmfc500)
  └─ payRule1, payRule2

_session.FXF3A.addItem(carrier, custType, account, itemClass, release:=row.Release)
```

`BuildDiscTable()` creates a `FXF3A.DiscCollection`. Each discount row is included only if
`Disc` or `EffDate` is non-blank. Fields `disc` (Single), `minChargeDisc` (Single),
`maxWgt` (Integer), `floorMin` (Single) are parsed from strings. Dates use
`ScreenScraping.NULL_DATE` when blank.

---

#### CHANGE — Modify existing record

```
_session.FXF3A.changeItem(
    carrier, custType, account,
    authority, number, item,   ← existing key to locate the record
    row.ToItemClass(),         ← updated item data
    row.Release)               ← whether to release after change
```

---

#### CANCEL — Cancel item with a cancel date

```
_session.FXF3A.cancelItem(
    carrier, custType, account,
    authority, number, item,
    row.GetCancelDate(),   ← parses CanDate1 as DateTime; uses NULL_DATE if blank
    row.Release)
```

---

#### DELETE — Permanently remove record

```
_session.FXF3A.deleteItem(carrier, custType, account, authority, number, item)
```

No confirmation — the operation executes immediately on the mainframe.

---

#### RELEASE — Release a held/unreleased item

```
_session.FXF3A.releaseItem(carrier, custType, account, authority, number, item)
```

---

### 2.5 Exception Handling

All exceptions from the screen-scraping library are caught and translated to row-level status.
`ProcessRow` re-throws after setting row status so the caller's error counter increments.

| Exception Type | Row Status | Behaviour |
|---|---|---|
| `AccountNotFoundException` | Error | "Account not found: {message}" — re-thrown |
| `NoDiscountRecordsException` | Warning | "No discount records" — **not re-thrown** (does not count as error) |
| `NumericValueException` | Error | "Invalid cancel date: {message}" — re-thrown |
| `GenericScreenScraperException` | Error | "{message} [screen dump available]" — `ex.ScreenDump` stored; re-thrown |
| Any other `Exception` | Error | `ex.Message` shown — re-thrown |

A `Warning` row is counted as OK (not error) in the final summary.

### 2.6 Export Results to CSV (`ExportResultsCommand`)

Available when `Results.Count > 0`. Writes a CSV with the following header:

```
Timestamp, Carrier, CustType, Account, Authority, Number, Item,
Released, Disc1, EffDate1, CancelDate1, Currency, Inter, TypeHaul,
GeoDir1, GeoType1, GeoName1, LastMaintDate, OperatorId, Status
```

File name defaults to `FXF3A_Results_yyyyMMdd_HHmmss.csv`.

### 2.7 Select All / Row Selection

- `SelectAllCommand`: sets `IsSelected = True` on every row in `BatchRows`
- `IsSelected` is a property on `BatchRowBase` (shared by all 13 screen models)
- Defaults to `False` when rows are loaded (from CSV, Excel, or Quick Load)
- Run Batch skips any row where `IsSelected = False`

---

*Document covers: Connect workflow, FXF3A data model, all 6 FXF3A operations (GET/ADD/CHANGE/CANCEL/DELETE/RELEASE), data loading paths, exception handling.*
