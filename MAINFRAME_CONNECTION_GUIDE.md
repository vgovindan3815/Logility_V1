# FXF3A Tool — Mainframe Connection Guide

This guide walks you through connecting the FXF3A Tool to the FedEx Freight CICS mainframe
for the first time. Follow the sections in order.

---

## Section 1: Pre-requisites

Before you attempt a connection, confirm you have all of the following.

1. **Bluezone terminal emulator installed.**
   Bluezone must be installed on the machine running the tool. Use the FedEx-provided
   license. You do **not** need to configure a Bluezone session manually — the tool
   launches and controls Bluezone entirely through its COM API (`BZWHLLLIB`), which is
   registered automatically when Bluezone is installed.

2. **Mainframe access granted.**
   Your Active Directory / network account must have been granted access to the
   **FDXF CICS region**. If you have not yet been granted access, raise an IT ticket
   with FedEx mainframe operations requesting:
   - Access to the FDXF CICS region
   - A terminal user ID (and, if separate, a Logility user ID)
   - The TN3270 host address and port for your environment

3. **TN3270 host address and port.**
   Obtain this from your FedEx IT team. It will be in the form `hostname:port` or
   `ip-address:port`, e.g. `mainframe.fedex.com:23`. Port 23 is the standard Telnet
   port; your environment may use a different port.

4. **Your terminal user ID and Logility user ID.**
   These are provided by your FedEx IT team when mainframe access is granted. In many
   environments both IDs are the same — confirm with IT if you are unsure.

5. **The `fxf3270.rsf` layout file.**
   This file ships in the `deploy\` folder of the tool. It defines the 3270 screen
   field map that the screen-scraping library uses to read and write terminal fields.
   Do **not** modify or rename this file.

6. **FedEx screen-scraping DLLs in the `deploy\` folder.**
   See Section 2 for the full list. These DLLs are proprietary FedEx libraries and
   are **not** included in the source repository. They must be copied into `deploy\`
   before you build or run the tool.

---

## Section 2: Verifying the `deploy\` Folder

The `deploy\` folder must contain exactly the following 10 DLLs **and** the RSF layout
file before the tool will build and run successfully.

```
deploy\
├── FedEx.PABST.SS.SSLib.dll              ← core screen-scraping session library
├── FedEx.PABST.SS.Screens.FXF3A.dll     ← screen class: Customer discount items
├── FedEx.PABST.SS.Screens.FXF3B.dll     ← screen class: Discounts by State/Terminal
├── FedEx.PABST.SS.Screens.FXF3C.dll     ← screen class: Geography discounts
├── FedEx.PABST.SS.Screens.FXF3D.dll     ← screen class: Product discounts
├── FedEx.PABST.SS.Screens.FXF3E.dll     ← screen class: Customer rates
├── FedEx.PABST.SS.Screens.FXF3F.dll     ← screen class: Discounts/adjustments
├── FedEx.PABST.SS.Screens.FXF3G.dll     ← screen class: Charges/allowances
├── FedEx.PABST.SS.Exceptions.dll        ← typed exception classes
├── Interop.BZWHLLLIB.dll                ← COM interop wrapper for Bluezone
└── fxf3270.rsf                          ← 3270 screen field layout file
```

**Verification steps:**

1. Open File Explorer and navigate to the `deploy\` folder inside the project root.
2. Confirm all 10 `.dll` files are present.
3. Confirm `fxf3270.rsf` is present.
4. If any file is missing, contact your FedEx IT team or the person who provided the
   tool to obtain the missing files. The tool will not build without the DLLs and will
   not connect without the RSF file.

---

## Section 3: Confirming Bluezone Is Installed

The tool does **not** require you to manually configure a Bluezone session or profile.
It launches Bluezone programmatically via the `BZWHLLLIB` COM API, which is registered
in the Windows registry automatically when Bluezone is installed.

1. Open **Settings → Apps** (Windows 11) or **Control Panel → Programs and Features**
   (classic view).
2. Search for **Bluezone** in the installed programs list.
3. Confirm a Bluezone entry is present and the version is the one provided by FedEx IT.
4. If Bluezone is not installed, contact your FedEx IT team for the installer and license
   key. Re-run the Bluezone installer even if a partial installation exists, to ensure
   the COM components are properly registered.

> **Note:** If you receive a COM error such as `"Class not registered"` or
> `"BZWHLLLIB not found"` when the tool tries to connect, Bluezone is either not
> installed or its COM registration is broken. Reinstalling Bluezone fixes this.

---

## Section 4: Building and Running the Tool

If you have already received a pre-built `FXF3A_Tool.exe`, skip to step 3.

1. **Build the project** using MSBuild from the project root directory:

   ```bash
   msbuild FXF3A_Tool.vbproj /p:Configuration=Release /v:minimal
   ```

   A successful build ends with:
   ```
   Build succeeded.
       0 Warning(s)
       0 Error(s)
   ```

   The compiled executable is placed at `bin\Release\FXF3A_Tool.exe`.

2. **Confirm the DLLs were copied** to `bin\Release\` by the build. MSBuild copies
   the `deploy\` folder contents to the output directory automatically. If they are
   missing, check that the `deploy\` folder was populated before building.

3. **Run the tool:**

   ```bash
   bin\Release\FXF3A_Tool.exe
   ```

   The main window opens. You will see a connection bar across the top of the window
   and a left navigation rail with screen names (FXF3A through FXF3G) that are
   currently grayed out. All screens are locked until a successful connection is made.

---

## Section 5: Filling In the Connection Bar

The connection bar at the top of the main window contains six fields. Fill them in
before clicking CONNECT.

| Field | What to enter |
|---|---|
| **Host** | TN3270 hostname and port, e.g. `mainframe.fedex.com:23` or `10.1.2.3:23`. Include the colon and port number. |
| **System** | CICS system code. For FedEx Freight this is `FDXF`. Do not change this unless IT specifies a different region. |
| **UID T** | Your terminal user ID — the mainframe login ID provided by FedEx IT. |
| **UID L** | Your Logility user ID. In most environments this is the same as UID T. Enter the same value if you were not given a separate Logility ID. |
| **RSF Path** | Full path to the `fxf3270.rsf` layout file. The default is `C:\FXF\fxf3270.rsf`. If your file is in a different location (e.g. inside the tool's `deploy\` folder), use the Browse button to locate it. Example: `C:\Projects\FXF3A_Tool\deploy\fxf3270.rsf` |
| **Timeout** | Connection timeout in milliseconds. Use `30000` (30 seconds) on a normal corporate network. Increase to `60000` (60 seconds) if you are on a VPN or slow connection. |

> **Settings are saved automatically** after a successful connection. On subsequent
> runs, all fields except passwords will be pre-populated from the previous session.

---

## Section 6: Connecting

1. Confirm all six fields in the connection bar are filled in correctly (Section 5).
2. Click the **CONNECT** button.
3. A password dialog appears prompting for your **Terminal Password**. Enter the
   mainframe password associated with your terminal user ID (UID T). Click OK.
4. A second password dialog appears prompting for your **Logility Password**. Enter
   the Logility password associated with your Logility user ID (UID L). If both IDs
   are the same, both passwords are also typically the same — but enter it again.
   Click OK.
5. The tool connects in the background. During this time the CONNECT button is
   disabled and a status message is shown in the connection bar. Bluezone launches
   as a background process — you may briefly see a terminal window appear and then
   minimize. This is normal.
6. When the connection succeeds, the **status badge** in the top-right of the window
   changes from gray (**Disconnected**) to green (**Connected**), and the status bar
   shows a message such as `Connected — FDXF @ mainframe.fedex.com:23`.
7. The left navigation rail screens — **FXF3A through FXF3G** — become clickable.
8. The tool automatically navigates to the **FXF3A** screen.

> **Passwords are never saved.** You must enter them fresh every session. This is
> by design — the tool clears password strings from memory immediately after the
> connection is established.

---

## Section 7: Troubleshooting Connection Failures

If the connection fails, an error banner appears below the connection bar. Use the
table below to diagnose and fix common problems.

| Error / Symptom | Likely Cause | Fix |
|---|---|---|
| `"Connection failed. Check credentials and host."` | Wrong host/port, wrong user ID or password, or the mainframe host is not reachable from your machine. | Double-check the Host field (include port). Verify your user ID and password with FedEx IT. Test that you can reach the host with `ping` or `telnet hostname port`. |
| `"Bluezone not found"` or a COM exception / `"Class not registered"` | Bluezone is not installed, or its COM component (`BZWHLLLIB`) is not registered in the Windows registry. | Reinstall Bluezone using the FedEx-provided installer. After reinstalling, retry the connection. |
| Connection hangs for a long time, then times out | Network latency to the mainframe is high, or a firewall is silently dropping the connection. | Increase the Timeout field to `60000`. Confirm with IT that port 23 (or your configured port) is open to the mainframe host from your machine or VPN. |
| `"CICS not available"` or the screen shows a CICS unavailable message | The FDXF CICS region is down or restarting on the mainframe. | This is a mainframe-side issue. Wait a few minutes and retry. If the problem persists, contact FedEx mainframe operations. |
| `"Invalid user ID"` or `"User not authorized"` | Your mainframe ID does not have access to the FDXF CICS region. | Raise an IT ticket requesting CICS access for user ID `<your UID T>` to the FDXF region. |
| Bluezone window opens but never logs in; connection eventually times out | The RSF layout file path is wrong, or the RSF file is corrupt. | Verify the RSF Path field points to a valid `fxf3270.rsf` file. Confirm the file size is non-zero. |
| Connection succeeds but immediately drops | Your terminal password may have expired on the mainframe. | Log in directly via a standalone Bluezone or Rumba session to check and reset your mainframe password, then retry. |

---

## Section 8: Testing the Connection with a GET

Once connected, perform a quick end-to-end test to confirm the tool can read data
from the mainframe.

1. Click **FXF3A** in the left navigation rail. The FXF3A batch grid opens.
2. Click **+ Add Row** to add a new batch row to the grid.
3. Set the following fields in the new row:
   - **ACTION** = `GET`
   - **CARRIER** = `FXFM`
   - **CUST TYPE** = `CC`
4. Enter a known test account number in the **ACCOUNT** column. Use an account number
   you know exists in the system (e.g. a customer account from your pricing team).
5. Leave **AUTHORITY**, **NUMBER**, and **ITEM** blank. When left blank, the GET
   retrieves all items for the account.
6. Click **▶ Run Batch**.
7. The tool sends the request to the mainframe. Wait for the progress indicator to
   finish (this may take a few seconds per row).
8. Results appear in the **GET Results** panel below the grid.
9. If the **STATUS** column shows a checkmark (`✓`) for your row, the connection is
   working correctly and data was retrieved successfully.

If the STATUS column shows an error, note the error message and refer to Section 7,
or contact the tool administrator.

---

## Section 9: Disconnecting

1. Click the **■ DISCONNECT** button in the connection bar.
2. The tool closes the Bluezone TN3270 session in the background. The Bluezone
   process terminates automatically — you do not need to close it manually.
3. The status badge returns to gray (**Disconnected**) and the navigation rail
   screens are grayed out again.
4. Your connection settings (Host, System, UID T, UID L, RSF Path, Timeout) are
   saved automatically and will be pre-populated next time you run the tool.
5. **Passwords are never saved.** You will be prompted for them again at the next
   connection.

> If the tool is closed without disconnecting first (e.g. via the window X button or
> a crash), the Bluezone process may remain running in the background. Check Task
> Manager for a lingering `bzwhll.exe` process and end it manually if needed.

---

## Section 10: Using Rumba Instead of Bluezone (If Required)

The current build is configured to use **Bluezone** as the terminal emulator. If
your environment requires **Rumba** or **MicroFocus Rumba** instead, a one-line
code change is required in the session manager.

1. Open `Core\SessionManager.vb` in a text editor.
2. Locate the `ScreenScraping` constructor call inside `ConnectAsync` (around line 124).
3. Change the first argument from `sslibTypeType.Bluezone` to `sslibTypeType.Rumba`:

   ```vb
   ' Before (Bluezone):
   _ss = New ScreenScraping(
       ScreenScraping.sslibTypeType.Bluezone,
       ...

   ' After (Rumba):
   _ss = New ScreenScraping(
       ScreenScraping.sslibTypeType.Rumba,
       ...
   ```

4. Rebuild the project (see Section 4, step 1).
5. Ensure Rumba is installed on the machine. Like Bluezone, Rumba must have its COM
   API registered. Reinstalling Rumba from the vendor installer ensures this.
6. The rest of the connection procedure is identical to Sections 5–9.

> Contact your FedEx IT team if you are unsure which emulator your environment
> supports, or if you need a license for Rumba.

---

## Quick-Reference Checklist

Use this checklist before your first connection attempt:

- [ ] Bluezone is installed (visible in Apps/Programs and Features)
- [ ] Mainframe IT ticket completed; access to FDXF CICS region granted
- [ ] TN3270 host address and port obtained from FedEx IT
- [ ] Terminal user ID and Logility user ID obtained
- [ ] All 10 DLLs present in `deploy\`
- [ ] `fxf3270.rsf` present in `deploy\`
- [ ] Project built successfully (`msbuild ... /p:Configuration=Release`)
- [ ] Connection bar fields filled in (Host, System, UID T, UID L, RSF Path, Timeout)
- [ ] CONNECT clicked; Terminal and Logility passwords entered when prompted
- [ ] Status badge shows green (Connected)
- [ ] Test GET on FXF3A returns `✓` STATUS
