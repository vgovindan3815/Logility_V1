Option Strict On
Option Explicit On

Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports FedEx.PABST.SS.SSLib
Imports FedEx.PABST.SS.Screens
Imports FedEx.PABST.SS.Exceptions
Imports Logility_Freight.Core
Imports Logility_Freight.Models

' ================================================================
'  FXF3J, FXF3K, FXF3M, FXF3N, FXF4M ViewModels
'  All follow the identical pattern as FXF3D_ViewModel.
'
'  Per-screen differences:
'    FXF3J — COPY only. No GET/ADD/CHANGE/CANCEL/DELETE.
'    FXF3K — GET/DELETE/CANCEL. Matrix-level ops; no custType/account.
'    FXF3M — GET/ADD/CHANGE/CANCEL/DELETE. addItem has pRelease.
'    FXF3N — GET/ADD/CHANGE/CANCEL/DELETE. addItem/changeItem use only pItemObj.
'    FXF4M — GET/GETALL/CANCEL/DELETE. Has LoadAccountCommand.
' ================================================================

Namespace ViewModels

    ' ──────────────────────────────────────────────────────────────
    '  FXF3J — Copy Customer/National Account Info
    '  Screen: DSNM1NC-3J
    '  Methods: copyAcct
    '  COPY only — no GET/ADD/CHANGE/CANCEL/DELETE
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3J_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand      = New RelayCommand(AddressOf ExecuteRunBatch,
                                                     Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand        = New RelayCommand(Sub() BatchRows.Add(New FXF3J_BatchRow()))
            _clearBatchCommand    = New RelayCommand(Sub() BatchRows.Clear(),
                                                     Function() Not _isBusy)
            _clearResultsCommand  = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3J_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3J_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3J_BatchRow)

        ' ── Progress / state ─────────────────────────────────────────
        Private _isBusy As Boolean
        Public Property IsBusy As Boolean
            Get
                Return _isBusy
            End Get
            Set(v As Boolean)
                SetField(_isBusy, v)
                _runBatchCommand.RaiseCanExecuteChanged()
                _clearBatchCommand.RaiseCanExecuteChanged()
                _loadCsvCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        Private _progressCurrent As Integer
        Public Property ProgressCurrent As Integer
            Get
                Return _progressCurrent
            End Get
            Set(v As Integer)
                SetField(_progressCurrent, v)
            End Set
        End Property

        Private _progressTotal As Integer
        Public Property ProgressTotal As Integer
            Get
                Return _progressTotal
            End Get
            Set(v As Integer)
                SetField(_progressTotal, v)
            End Set
        End Property

        Private _progressText As String = ""
        Public Property ProgressText As String
            Get
                Return _progressText
            End Get
            Set(v As String)
                SetField(_progressText, v)
            End Set
        End Property

        Private _bannerMessage As String = ""
        Public Property BannerMessage As String
            Get
                Return _bannerMessage
            End Get
            Set(v As String)
                SetField(_bannerMessage, v)
            End Set
        End Property

        Public ReadOnly Property BannerIsError As Boolean
            Get
                Return _bannerMessage.Contains("Error") OrElse _bannerMessage.Contains("failed")
            End Get
        End Property

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _runBatchCommand      As RelayCommand
        Private ReadOnly _addRowCommand        As RelayCommand
        Private ReadOnly _clearBatchCommand    As RelayCommand
        Private ReadOnly _clearResultsCommand  As RelayCommand
        Private ReadOnly _exportResultsCommand As RelayCommand
        Private ReadOnly _loadCsvCommand       As RelayCommand
        Private ReadOnly _selectAllCommand As RelayCommand

        Public ReadOnly Property RunBatchCommand      As RelayCommand
            Get
                Return _runBatchCommand
            End Get
        End Property
        Public ReadOnly Property AddRowCommand        As RelayCommand
            Get
                Return _addRowCommand
            End Get
        End Property
        Public ReadOnly Property ClearBatchCommand    As RelayCommand
            Get
                Return _clearBatchCommand
            End Get
        End Property
        Public ReadOnly Property ClearResultsCommand  As RelayCommand
            Get
                Return _clearResultsCommand
            End Get
        End Property
        Public ReadOnly Property ExportResultsCommand As RelayCommand
            Get
                Return _exportResultsCommand
            End Get
        End Property
        Public ReadOnly Property LoadCsvCommand As RelayCommand
            Get
                Return _loadCsvCommand
            End Get
        End Property
        Public ReadOnly Property SelectAllCommand As RelayCommand
            Get
                Return _selectAllCommand
            End Get
        End Property

        ' ── Run Batch ────────────────────────────────────────────────
        Private Async Sub ExecuteRunBatch()
            IsBusy = True
            BannerMessage = ""

            If Not _session.IsConnected Then
                BannerMessage = "Not connected to mainframe. Please connect before running the batch."
                IsBusy = False
                Return
            End If

            Dim selectedRows As New List(Of FXF3J_BatchRow)
            For Each r As FXF3J_BatchRow In BatchRows
                If r.IsSelected Then selectedRows.Add(r)
            Next
            If selectedRows.Count = 0 Then
                BannerMessage = "No rows selected. Use the checkboxes to select rows to process."
                IsBusy = False
                Return
            End If
            Dim total   = selectedRows.Count
            Dim ok      = 0
            Dim err     = 0
            Dim skipped = 0
            ProgressTotal   = total
            ProgressCurrent = 0

            For i As Integer = 0 To selectedRows.Count - 1
                Dim row = selectedRows(i)
                ProgressCurrent = i + 1
                ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", i + 1, total, row.Action, row.FromName)

                If String.IsNullOrWhiteSpace(row.Action) Then
                    row.Status = OperationStatus.Skipped
                    skipped   += 1
                    Continue For
                End If

                row.Status        = OperationStatus.Running
                row.StatusMessage = ""

                Try
                    Await Task.Run(Sub() ProcessRow(row))
                    ok += 1
                Catch ex As Exception
                    err += 1
                End Try

                Await Task.Delay(10)
            Next

            ProgressText  = String.Format("Complete — {0} OK, {1} errors, {2} skipped", ok, err, skipped)
            BannerMessage = ProgressText
            IsBusy        = False
        End Sub

        ' ── Process single row (background thread) ───────────────────
        Private Sub ProcessRow(row As FXF3J_BatchRow)
            Try
                Select Case row.Action.ToUpper()

                    Case "COPY"
                        Dim parms As New FXF3J.parmClassv1()
                        parms.carrier = row.Carrier
                        parms.fromCust = New FXF3J.custClass()
                        parms.fromCust.type = ParseCustType(If(String.IsNullOrWhiteSpace(row.FromType), row.CustType, row.FromType))
                        parms.fromCust.name = row.FromName
                        parms.fromCust.auth = row.FromAuth
                        parms.fromCust.nbr  = row.FromNbr
                        parms.fromCust.item = row.FromItem
                        parms.fromCust.part = row.FromPart
                        parms.toCust = New FXF3J.custClass()
                        parms.toCust.type = ParseCustType(row.ToType)
                        parms.toCust.name = row.ToName
                        parms.toCust.auth = row.ToAuth
                        parms.toCust.nbr  = row.ToNbr
                        parms.toCust.item = row.ToItem
                        parms.toCust.part = row.ToPart
                        parms.toC = row.ToCarrier
                        parms.toRel = (row.ToRelease = "Y")
                        If Not String.IsNullOrWhiteSpace(row.CopyEffDate) Then
                            Dim d As Date
                            If Date.TryParse(row.CopyEffDate, d) Then parms.effDate = d
                        End If
                        _session.FXF3J.copyAcctv1(parms)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case Else
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status        = OperationStatus.Skipped
                            row.StatusMessage = "Unsupported action — use COPY"
                        End Sub)
                End Select

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Account not found: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                DebugLogger.LogError(row, ex)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            End Try
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        ' -- Import from CSV
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF3J Batch from CSV"
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim lines As String()
            Try
                lines = IO.File.ReadAllLines(dlg.FileName)
            Catch ex As Exception
                BannerMessage = "Cannot read file: " & ex.Message
                Return
            End Try

            If lines.Length < 2 Then
                BannerMessage = "CSV has no data rows."
                Return
            End If

            Dim hdr = Core.CsvHelper.BuildHeaderMap(lines(0))
            Dim count = 0, errors = 0

            For i As Integer = 1 To lines.Length - 1
                Dim line = lines(i).Trim()
                If String.IsNullOrEmpty(line) Then Continue For
                Try
                    Dim f = Core.CsvHelper.SplitLine(line)
                    Dim G  As Func(Of String, String)         = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD As Func(Of String, String, String) = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF3J_BatchRow()
                    row.Action      = G("Action")
                    row.Carrier     = GD("Carrier",  "FXFM")
                    row.FromType    = GD("FromType", GD("CustType", "CC"))
                    row.CustType    = row.FromType
                    row.FromName    = G("FromName")
                    row.FromAuth    = G("FromAuth")
                    row.FromNbr     = G("FromNbr")
                    row.FromItem    = G("FromItem")
                    row.FromPart    = G("FromPart")
                    row.ToType      = GD("ToType", "CC")
                    row.ToName      = G("ToName")
                    row.ToAuth      = G("ToAuth")
                    row.ToNbr       = G("ToNbr")
                    row.ToItem      = G("ToItem")
                    row.ToPart      = G("ToPart")
                    row.ToCarrier   = G("ToCarrier")
                    row.CopyEffDate = G("CopyEffDate")
                    BatchRows.Add(row)
                    count += 1
                Catch
                    errors += 1
                End Try
            Next

            BannerMessage = If(errors > 0,
                String.Format("{0} rows imported, {1} skipped from {2}.", count, errors, IO.Path.GetFileName(dlg.FileName)),
                String.Format("{0} rows imported from {1}.", count, IO.Path.GetFileName(dlg.FileName)))
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3J_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,FromAuth,FromNbr,FromItem,FromPart," &
                             "ToType,ToAuth,ToNbr,ToItem,ToPart,ToCarrier,CopyEffDate,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType,
                        Q(r.FromAuth), Q(r.FromNbr), Q(r.FromItem), Q(r.FromPart),
                        r.ToType,
                        Q(r.ToAuth), Q(r.ToNbr), Q(r.ToItem), Q(r.ToPart),
                        r.ToCarrier, r.CopyEffDate,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
        End Sub

        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3J_BatchRow()
                br.Action       = G("ACTION")
                br.Carrier      = GD("CARRIER", "FXFM")
                br.FromType     = GD("FROMTYPE", GD("CUSTTYPE", "CC"))
                br.CustType     = br.FromType
                br.FromName     = G("FROMNAME")
                br.FromAuth     = G("FROMAUTH")
                br.FromNbr      = G("FROMNBR")
                br.FromItem     = G("FROMITEM")
                br.FromPart     = G("FROMPART")
                br.ToType       = GD("TOTYPE", "CC")
                br.ToName       = G("TONAME")
                br.ToAuth       = G("TOAUTH")
                br.ToNbr        = G("TONBR")
                br.ToItem       = G("TOITEM")
                br.ToPart       = G("TOPART")
                br.ToCarrier    = GD("TOCARRIER", "FXFM")
                br.ToRelease    = If(G("TORELEASE").ToUpper() = "Y", "Y", "N")
                br.CopyEffDate  = G("COPYEFFDATE")
                BatchRows.Add(br)
            Next
        End Sub

        ' ── Helpers ──────────────────────────────────────────────────
        Private Shared Function ParseCarrier(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum)
        End Function

        Private Shared Function ParseCustType(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum)
        End Function

        Private Shared Function Q(s As String) As String
            If s Is Nothing Then Return ""
            If s.Contains(",") Then Return """" & s.Replace("""", """""") & """"
            Return s
        End Function

    End Class


    ' ──────────────────────────────────────────────────────────────
    '  FXF3K — Cust/Nat'l Acct State Matrix
    '  Screen: DSNM1BM-3K
    '  Methods: getMatrix, deleteMatrix, cancelMatrix
    '  GET/DELETE/CANCEL — no custType/account for matrix ops
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3K_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand      = New RelayCommand(AddressOf ExecuteRunBatch,
                                                     Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand        = New RelayCommand(Sub() BatchRows.Add(New FXF3K_BatchRow()))
            _clearBatchCommand    = New RelayCommand(Sub() BatchRows.Clear(),
                                                     Function() Not _isBusy)
            _clearResultsCommand  = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3K_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3K_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3K_BatchRow)

        ' ── Progress / state ─────────────────────────────────────────
        Private _isBusy As Boolean
        Public Property IsBusy As Boolean
            Get
                Return _isBusy
            End Get
            Set(v As Boolean)
                SetField(_isBusy, v)
                _runBatchCommand.RaiseCanExecuteChanged()
                _clearBatchCommand.RaiseCanExecuteChanged()
                _loadCsvCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        Private _progressCurrent As Integer
        Public Property ProgressCurrent As Integer
            Get
                Return _progressCurrent
            End Get
            Set(v As Integer)
                SetField(_progressCurrent, v)
            End Set
        End Property

        Private _progressTotal As Integer
        Public Property ProgressTotal As Integer
            Get
                Return _progressTotal
            End Get
            Set(v As Integer)
                SetField(_progressTotal, v)
            End Set
        End Property

        Private _progressText As String = ""
        Public Property ProgressText As String
            Get
                Return _progressText
            End Get
            Set(v As String)
                SetField(_progressText, v)
            End Set
        End Property

        Private _bannerMessage As String = ""
        Public Property BannerMessage As String
            Get
                Return _bannerMessage
            End Get
            Set(v As String)
                SetField(_bannerMessage, v)
            End Set
        End Property

        Public ReadOnly Property BannerIsError As Boolean
            Get
                Return _bannerMessage.Contains("Error") OrElse _bannerMessage.Contains("failed")
            End Get
        End Property

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _runBatchCommand      As RelayCommand
        Private ReadOnly _addRowCommand        As RelayCommand
        Private ReadOnly _clearBatchCommand    As RelayCommand
        Private ReadOnly _clearResultsCommand  As RelayCommand
        Private ReadOnly _exportResultsCommand As RelayCommand
        Private ReadOnly _loadCsvCommand       As RelayCommand
        Private ReadOnly _selectAllCommand As RelayCommand

        Public ReadOnly Property RunBatchCommand      As RelayCommand
            Get
                Return _runBatchCommand
            End Get
        End Property
        Public ReadOnly Property AddRowCommand        As RelayCommand
            Get
                Return _addRowCommand
            End Get
        End Property
        Public ReadOnly Property ClearBatchCommand    As RelayCommand
            Get
                Return _clearBatchCommand
            End Get
        End Property
        Public ReadOnly Property ClearResultsCommand  As RelayCommand
            Get
                Return _clearResultsCommand
            End Get
        End Property
        Public ReadOnly Property ExportResultsCommand As RelayCommand
            Get
                Return _exportResultsCommand
            End Get
        End Property
        Public ReadOnly Property LoadCsvCommand As RelayCommand
            Get
                Return _loadCsvCommand
            End Get
        End Property
        Public ReadOnly Property SelectAllCommand As RelayCommand
            Get
                Return _selectAllCommand
            End Get
        End Property

        ' ── Run Batch ────────────────────────────────────────────────
        Private Async Sub ExecuteRunBatch()
            IsBusy = True
            BannerMessage = ""

            If Not _session.IsConnected Then
                BannerMessage = "Not connected to mainframe. Please connect before running the batch."
                IsBusy = False
                Return
            End If

            Dim selectedRows As New List(Of FXF3K_BatchRow)
            For Each r As FXF3K_BatchRow In BatchRows
                If r.IsSelected Then selectedRows.Add(r)
            Next
            If selectedRows.Count = 0 Then
                BannerMessage = "No rows selected. Use the checkboxes to select rows to process."
                IsBusy = False
                Return
            End If
            Dim total   = selectedRows.Count
            Dim ok      = 0
            Dim err     = 0
            Dim skipped = 0
            ProgressTotal   = total
            ProgressCurrent = 0

            For i As Integer = 0 To selectedRows.Count - 1
                Dim row = selectedRows(i)
                ProgressCurrent = i + 1
                ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", i + 1, total, row.Action, row.MatrixName)

                If String.IsNullOrWhiteSpace(row.Action) Then
                    row.Status = OperationStatus.Skipped
                    skipped   += 1
                    Continue For
                End If

                row.Status        = OperationStatus.Running
                row.StatusMessage = ""

                Try
                    Await Task.Run(Sub() ProcessRow(row))
                    ok += 1
                Catch ex As Exception
                    err += 1
                End Try

                Await Task.Delay(10)
            Next

            ProgressText  = String.Format("Complete — {0} OK, {1} errors, {2} skipped", ok, err, skipped)
            BannerMessage = ProgressText
            IsBusy        = False
        End Sub

        ' ── Process single row (background thread) ───────────────────
        Private Sub ProcessRow(row As FXF3K_BatchRow)
            Try
                Dim carrier = ParseCarrier(row.Carrier)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3K.getMatrix(carrier, row.MatrixName, ParseDate(row.MatrixEffDate))
                        Dim resultRow As New FXF3K_BatchRow
                        resultRow.Carrier          = row.Carrier
                        resultRow.MatrixName       = it.itemHeader.stMatrixName
                        resultRow.MatrixEffDate    = FormatDate(it.effectiveDate)
                        resultRow.MatrixCancelDate = FormatDate(it.cancelDate)
                        resultRow.FsAuthority      = it.fsAuthority
                        resultRow.FsNumber         = it.fsNumber
                        resultRow.FsItem           = it.fsItem
                        resultRow.LastMaintDate    = FormatDate(it.lastMaintenanceDate)
                        resultRow.OperatorId       = it.operatorId
                        resultRow.Revision         = it.revision
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3K.deleteMatrix(carrier, row.MatrixName, ParseDate(row.MatrixEffDate))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3K.cancelMatrix(carrier, row.MatrixName, ParseDate(row.MatrixCancelDate))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case Else
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status        = OperationStatus.Skipped
                            row.StatusMessage = "Unsupported action — use GET/DELETE/CANCEL"
                        End Sub)
                End Select

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Account not found: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                DebugLogger.LogError(row, ex)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            End Try
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        ' -- Import from CSV
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF3K Batch from CSV"
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim lines As String()
            Try
                lines = IO.File.ReadAllLines(dlg.FileName)
            Catch ex As Exception
                BannerMessage = "Cannot read file: " & ex.Message
                Return
            End Try

            If lines.Length < 2 Then
                BannerMessage = "CSV has no data rows."
                Return
            End If

            Dim hdr = Core.CsvHelper.BuildHeaderMap(lines(0))
            Dim count = 0, errors = 0

            For i As Integer = 1 To lines.Length - 1
                Dim line = lines(i).Trim()
                If String.IsNullOrEmpty(line) Then Continue For
                Try
                    Dim f = Core.CsvHelper.SplitLine(line)
                    Dim G  As Func(Of String, String)         = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD As Func(Of String, String, String) = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF3K_BatchRow()
                    row.Action           = G("Action")
                    row.Carrier          = GD("Carrier", "FXFM")
                    row.MatrixName       = G("MatrixName")
                    row.MatrixEffDate    = G("MatrixEffDate")
                    row.MatrixCancelDate = G("MatrixCancelDate")
                    row.FsAuthority      = G("FsAuthority")
                    row.FsNumber         = G("FsNumber")
                    row.FsItem           = G("FsItem")
                    row.LastMaintDate    = G("LastMaintDate")
                    row.OperatorId       = G("OperatorId")
                    row.Revision         = G("Revision")
                    BatchRows.Add(row)
                    count += 1
                Catch
                    errors += 1
                End Try
            Next

            BannerMessage = If(errors > 0,
                String.Format("{0} rows imported, {1} skipped from {2}.", count, errors, IO.Path.GetFileName(dlg.FileName)),
                String.Format("{0} rows imported from {1}.", count, IO.Path.GetFileName(dlg.FileName)))
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3K_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,MatrixName,MatrixEffDate,MatrixCancelDate,FsAuthority,FsNumber,FsItem,LastMaintDate,OperatorId,Revision,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, Q(r.MatrixName),
                        r.MatrixEffDate, r.MatrixCancelDate,
                        Q(r.FsAuthority), Q(r.FsNumber), Q(r.FsItem),
                        r.LastMaintDate, Q(r.OperatorId), Q(r.Revision),
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
        End Sub

        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3K_BatchRow()
                br.Action           = G("ACTION")
                br.Carrier          = GD("CARRIER", "FXFM")
                br.MatrixName       = G("MATRIXNAME")
                br.MatrixEffDate    = G("MATRIXEFFDATE")
                br.MatrixCancelDate = G("MATRIXCANCELDATE")
                br.FsAuthority      = G("FSAUTHORITY")
                br.FsNumber         = G("FSNUMBER")
                br.FsItem           = G("FSITEM")
                br.LastMaintDate    = G("LASTMAINTDATE")
                br.OperatorId       = G("OPERATORID")
                br.Revision         = G("REVISION")
                BatchRows.Add(br)
            Next
        End Sub

        ' ── Helpers ──────────────────────────────────────────────────
        Private Shared Function ParseCarrier(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum)
        End Function

        Private Shared Function ParseCustType(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum)
        End Function

        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return Nothing
            Dim d As Date
            If Date.TryParse(s, d) Then Return d
            Return Nothing
        End Function

        Private Shared Function FormatDate(d As Date) As String
            If d = Nothing Then Return ""
            Return d.ToString("MM/dd/yy")
        End Function

        Private Shared Function Q(s As String) As String
            If s Is Nothing Then Return ""
            If s.Contains(",") Then Return """" & s.Replace("""", """""") & """"
            Return s
        End Function

    End Class


    ' ──────────────────────────────────────────────────────────────
    '  FXF3M — Cust/Nat'l Acct Handling Unit Allowance
    '  Screen: DSNM1H3-3M
    '  Methods: getItem, addItem, changeItem, cancelItem, deleteItem
    '  GET/ADD/CHANGE/CANCEL/DELETE
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3M_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand      = New RelayCommand(AddressOf ExecuteRunBatch,
                                                     Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand        = New RelayCommand(Sub() BatchRows.Add(New FXF3M_BatchRow()))
            _clearBatchCommand    = New RelayCommand(Sub() BatchRows.Clear(),
                                                     Function() Not _isBusy)
            _clearResultsCommand  = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3M_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3M_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3M_BatchRow)

        ' ── Progress / state ─────────────────────────────────────────
        Private _isBusy As Boolean
        Public Property IsBusy As Boolean
            Get
                Return _isBusy
            End Get
            Set(v As Boolean)
                SetField(_isBusy, v)
                _runBatchCommand.RaiseCanExecuteChanged()
                _clearBatchCommand.RaiseCanExecuteChanged()
                _loadCsvCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        Private _progressCurrent As Integer
        Public Property ProgressCurrent As Integer
            Get
                Return _progressCurrent
            End Get
            Set(v As Integer)
                SetField(_progressCurrent, v)
            End Set
        End Property

        Private _progressTotal As Integer
        Public Property ProgressTotal As Integer
            Get
                Return _progressTotal
            End Get
            Set(v As Integer)
                SetField(_progressTotal, v)
            End Set
        End Property

        Private _progressText As String = ""
        Public Property ProgressText As String
            Get
                Return _progressText
            End Get
            Set(v As String)
                SetField(_progressText, v)
            End Set
        End Property

        Private _bannerMessage As String = ""
        Public Property BannerMessage As String
            Get
                Return _bannerMessage
            End Get
            Set(v As String)
                SetField(_bannerMessage, v)
            End Set
        End Property

        Public ReadOnly Property BannerIsError As Boolean
            Get
                Return _bannerMessage.Contains("Error") OrElse _bannerMessage.Contains("failed")
            End Get
        End Property

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _runBatchCommand      As RelayCommand
        Private ReadOnly _addRowCommand        As RelayCommand
        Private ReadOnly _clearBatchCommand    As RelayCommand
        Private ReadOnly _clearResultsCommand  As RelayCommand
        Private ReadOnly _exportResultsCommand As RelayCommand
        Private ReadOnly _loadCsvCommand       As RelayCommand
        Private ReadOnly _selectAllCommand As RelayCommand

        Public ReadOnly Property RunBatchCommand      As RelayCommand
            Get
                Return _runBatchCommand
            End Get
        End Property
        Public ReadOnly Property AddRowCommand        As RelayCommand
            Get
                Return _addRowCommand
            End Get
        End Property
        Public ReadOnly Property ClearBatchCommand    As RelayCommand
            Get
                Return _clearBatchCommand
            End Get
        End Property
        Public ReadOnly Property ClearResultsCommand  As RelayCommand
            Get
                Return _clearResultsCommand
            End Get
        End Property
        Public ReadOnly Property ExportResultsCommand As RelayCommand
            Get
                Return _exportResultsCommand
            End Get
        End Property
        Public ReadOnly Property LoadCsvCommand As RelayCommand
            Get
                Return _loadCsvCommand
            End Get
        End Property
        Public ReadOnly Property SelectAllCommand As RelayCommand
            Get
                Return _selectAllCommand
            End Get
        End Property

        ' ── Run Batch ────────────────────────────────────────────────
        Private Async Sub ExecuteRunBatch()
            IsBusy = True
            BannerMessage = ""

            If Not _session.IsConnected Then
                BannerMessage = "Not connected to mainframe. Please connect before running the batch."
                IsBusy = False
                Return
            End If

            Dim selectedRows As New List(Of FXF3M_BatchRow)
            For Each r As FXF3M_BatchRow In BatchRows
                If r.IsSelected Then selectedRows.Add(r)
            Next
            If selectedRows.Count = 0 Then
                BannerMessage = "No rows selected. Use the checkboxes to select rows to process."
                IsBusy = False
                Return
            End If
            Dim total   = selectedRows.Count
            Dim ok      = 0
            Dim err     = 0
            Dim skipped = 0
            ProgressTotal   = total
            ProgressCurrent = 0

            For i As Integer = 0 To selectedRows.Count - 1
                Dim row = selectedRows(i)
                ProgressCurrent = i + 1
                ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", i + 1, total, row.Action, row.Account)

                If String.IsNullOrWhiteSpace(row.Action) Then
                    row.Status = OperationStatus.Skipped
                    skipped   += 1
                    Continue For
                End If

                row.Status        = OperationStatus.Running
                row.StatusMessage = ""

                Try
                    Await Task.Run(Sub() ProcessRow(row))
                    ok += 1
                Catch ex As Exception
                    err += 1
                End Try

                Await Task.Delay(10)
            Next

            ProgressText  = String.Format("Complete — {0} OK, {1} errors, {2} skipped", ok, err, skipped)
            BannerMessage = ProgressText
            IsBusy        = False
        End Sub

        ' ── Process single row (background thread) ───────────────────
        Private Sub ProcessRow(row As FXF3M_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3M.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3M_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3M.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass(), (row.Release = "Y"))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3M.changeItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3M.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3M.deleteItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case Else
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status        = OperationStatus.Skipped
                            row.StatusMessage = "Unknown action: " & row.Action
                        End Sub)
                End Select

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Account not found: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                DebugLogger.LogError(row, ex)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            End Try
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        ' -- Import from CSV
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF3M Batch from CSV"
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim lines As String()
            Try
                lines = IO.File.ReadAllLines(dlg.FileName)
            Catch ex As Exception
                BannerMessage = "Cannot read file: " & ex.Message
                Return
            End Try

            If lines.Length < 2 Then
                BannerMessage = "CSV has no data rows."
                Return
            End If

            Dim hdr = Core.CsvHelper.BuildHeaderMap(lines(0))
            Dim count = 0, errors = 0

            For i As Integer = 1 To lines.Length - 1
                Dim line = lines(i).Trim()
                If String.IsNullOrEmpty(line) Then Continue For
                Try
                    Dim f = Core.CsvHelper.SplitLine(line)
                    Dim G  As Func(Of String, String)         = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD As Func(Of String, String, String) = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF3M_BatchRow()
                    row.Action    = G("Action")
                    row.Carrier   = GD("Carrier",  "FXFM")
                    row.CustType  = GD("CustType", "CC")
                    row.Account   = G("Account")
                    row.Authority = G("Authority")
                    row.Number    = G("Number")
                    row.Item      = G("Item")
                    row.Part      = G("Part")
                    row.FsAuth    = G("FsAuth")
                    row.FsNum     = G("FsNum")
                    row.FsItem    = G("FsItem")
                    row.HuType    = G("HuType")
                    row.Condition = G("Condition")
                    row.PrepdOrCollect = GD("PrepdOrCollect", "NA")
                    row.EffDate   = G("EffDate")
                    row.CanDateItem = G("CanDateItem")
                    row.Comments  = G("Comments")
                    row.CalcRule  = G("CalcRule")
                    row.AllowMaxNum = G("AllowMaxNum")
                    row.AllowMaxTotWgt = G("AllowMaxTotWgt")
                    row.AllowMaxPerWgt = G("AllowMaxPerWgt")
                    BatchRows.Add(row)
                    count += 1
                Catch
                    errors += 1
                End Try
            Next

            BannerMessage = If(errors > 0,
                String.Format("{0} rows imported, {1} skipped from {2}.", count, errors, IO.Path.GetFileName(dlg.FileName)),
                String.Format("{0} rows imported from {1}.", count, IO.Path.GetFileName(dlg.FileName)))
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3M_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "HuType,Condition,PrepdOrCollect,EffDate,CalcRule," &
                             "LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.HuType, r.Condition, r.PrepdOrCollect,
                        r.EffDate, r.CalcRule,
                        r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
        End Sub

        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3M_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.FsAuth           = G("FSAUTH")
                br.FsNum            = G("FSNUM")
                br.FsItem           = G("FSITEM")
                br.Release          = If(G("RELEASE").ToUpper()          = "Y", "Y", "N")
                br.RateManual       = If(G("RATEMANUAL").ToUpper()       = "Y", "Y", "N")
                br.HuType           = G("HUTYPE")
                br.Condition        = G("CONDITION")
                br.PrepdOrCollect   = GD("PREPDORCOLLECT", "NA")
                br.EffDate          = G("EFFDATE")
                br.CanDateItem      = G("CANDATEITEM")
                br.Comments         = G("COMMENTS")
                br.CalcRule         = G("CALCRULE")
                br.AllowMaxNum      = G("ALLOWMAXNUM")
                br.AllowMaxTotWgt   = G("ALLOWMAXTOTWGT")
                br.AllowMaxPerWgt   = G("ALLOWMAXPERWGT")
                br.EwrCls           = If(G("EWRCLS").ToUpper()           = "Y", "Y", "N")
                br.EwrClsNum        = G("EWRCLSNUM")
                br.EwrLowRate       = If(G("EWRLOWRATE").ToUpper()       = "Y", "Y", "N")
                br.EwrHighRate      = If(G("EWRHIGHRATE").ToUpper()      = "Y", "Y", "N")
                br.EwrHighestVolByWgt = If(G("EWRHIGHESTVOLBYWGT").ToUpper() = "Y", "Y", "N")
                br.LastMaintDate    = G("LASTMAINTDATE")
                br.OperatorId       = G("OPERATORID")
                br.Revision         = G("REVISION")
                BatchRows.Add(br)
            Next
        End Sub

        ' ── Helpers ──────────────────────────────────────────────────
        Private Shared Function ParseCarrier(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum)
        End Function

        Private Shared Function ParseCustType(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum)
        End Function

        Private Shared Function Q(s As String) As String
            If s Is Nothing Then Return ""
            If s.Contains(",") Then Return """" & s.Replace("""", """""") & """"
            Return s
        End Function

    End Class


    ' ──────────────────────────────────────────────────────────────
    '  FXF3N — Cust/Nat'l Acct Unit Rates
    '  Screen: DSNM1HR-3N
    '  Methods: getItem, addItem, changeItem, cancelItem, deleteItem
    '  GET/ADD/CHANGE/CANCEL/DELETE
    '  NOTE: addItem and changeItem use only pItemObj (no separate key params,
    '        no pRelease on addItem)
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3N_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand      = New RelayCommand(AddressOf ExecuteRunBatch,
                                                     Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand        = New RelayCommand(Sub() BatchRows.Add(New FXF3N_BatchRow()))
            _clearBatchCommand    = New RelayCommand(Sub() BatchRows.Clear(),
                                                     Function() Not _isBusy)
            _clearResultsCommand  = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3N_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3N_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3N_BatchRow)

        ' ── Progress / state ─────────────────────────────────────────
        Private _isBusy As Boolean
        Public Property IsBusy As Boolean
            Get
                Return _isBusy
            End Get
            Set(v As Boolean)
                SetField(_isBusy, v)
                _runBatchCommand.RaiseCanExecuteChanged()
                _clearBatchCommand.RaiseCanExecuteChanged()
                _loadCsvCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        Private _progressCurrent As Integer
        Public Property ProgressCurrent As Integer
            Get
                Return _progressCurrent
            End Get
            Set(v As Integer)
                SetField(_progressCurrent, v)
            End Set
        End Property

        Private _progressTotal As Integer
        Public Property ProgressTotal As Integer
            Get
                Return _progressTotal
            End Get
            Set(v As Integer)
                SetField(_progressTotal, v)
            End Set
        End Property

        Private _progressText As String = ""
        Public Property ProgressText As String
            Get
                Return _progressText
            End Get
            Set(v As String)
                SetField(_progressText, v)
            End Set
        End Property

        Private _bannerMessage As String = ""
        Public Property BannerMessage As String
            Get
                Return _bannerMessage
            End Get
            Set(v As String)
                SetField(_bannerMessage, v)
            End Set
        End Property

        Public ReadOnly Property BannerIsError As Boolean
            Get
                Return _bannerMessage.Contains("Error") OrElse _bannerMessage.Contains("failed")
            End Get
        End Property

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _runBatchCommand      As RelayCommand
        Private ReadOnly _addRowCommand        As RelayCommand
        Private ReadOnly _clearBatchCommand    As RelayCommand
        Private ReadOnly _clearResultsCommand  As RelayCommand
        Private ReadOnly _exportResultsCommand As RelayCommand
        Private ReadOnly _loadCsvCommand       As RelayCommand
        Private ReadOnly _selectAllCommand As RelayCommand

        Public ReadOnly Property RunBatchCommand      As RelayCommand
            Get
                Return _runBatchCommand
            End Get
        End Property
        Public ReadOnly Property AddRowCommand        As RelayCommand
            Get
                Return _addRowCommand
            End Get
        End Property
        Public ReadOnly Property ClearBatchCommand    As RelayCommand
            Get
                Return _clearBatchCommand
            End Get
        End Property
        Public ReadOnly Property ClearResultsCommand  As RelayCommand
            Get
                Return _clearResultsCommand
            End Get
        End Property
        Public ReadOnly Property ExportResultsCommand As RelayCommand
            Get
                Return _exportResultsCommand
            End Get
        End Property
        Public ReadOnly Property LoadCsvCommand As RelayCommand
            Get
                Return _loadCsvCommand
            End Get
        End Property
        Public ReadOnly Property SelectAllCommand As RelayCommand
            Get
                Return _selectAllCommand
            End Get
        End Property

        ' ── Run Batch ────────────────────────────────────────────────
        Private Async Sub ExecuteRunBatch()
            IsBusy = True
            BannerMessage = ""

            If Not _session.IsConnected Then
                BannerMessage = "Not connected to mainframe. Please connect before running the batch."
                IsBusy = False
                Return
            End If

            Dim selectedRows As New List(Of FXF3N_BatchRow)
            For Each r As FXF3N_BatchRow In BatchRows
                If r.IsSelected Then selectedRows.Add(r)
            Next
            If selectedRows.Count = 0 Then
                BannerMessage = "No rows selected. Use the checkboxes to select rows to process."
                IsBusy = False
                Return
            End If
            Dim total   = selectedRows.Count
            Dim ok      = 0
            Dim err     = 0
            Dim skipped = 0
            ProgressTotal   = total
            ProgressCurrent = 0

            For i As Integer = 0 To selectedRows.Count - 1
                Dim row = selectedRows(i)
                ProgressCurrent = i + 1
                ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", i + 1, total, row.Action, row.Account)

                If String.IsNullOrWhiteSpace(row.Action) Then
                    row.Status = OperationStatus.Skipped
                    skipped   += 1
                    Continue For
                End If

                row.Status        = OperationStatus.Running
                row.StatusMessage = ""

                Try
                    Await Task.Run(Sub() ProcessRow(row))
                    ok += 1
                Catch ex As Exception
                    err += 1
                End Try

                Await Task.Delay(10)
            Next

            ProgressText  = String.Format("Complete — {0} OK, {1} errors, {2} skipped", ok, err, skipped)
            BannerMessage = ProgressText
            IsBusy        = False
        End Sub

        ' ── Process single row (background thread) ───────────────────
        Private Sub ProcessRow(row As FXF3N_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3N.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3N_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3N.addItem(carrier, custType, row.Account, row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3N.changeItem(carrier, custType, row.Account, row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3N.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3N.deleteItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case Else
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status        = OperationStatus.Skipped
                            row.StatusMessage = "Unknown action: " & row.Action
                        End Sub)
                End Select

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Account not found: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                DebugLogger.LogError(row, ex)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            End Try
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        ' -- Import from CSV
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF3N Batch from CSV"
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim lines As String()
            Try
                lines = IO.File.ReadAllLines(dlg.FileName)
            Catch ex As Exception
                BannerMessage = "Cannot read file: " & ex.Message
                Return
            End Try

            If lines.Length < 2 Then
                BannerMessage = "CSV has no data rows."
                Return
            End If

            Dim hdr = Core.CsvHelper.BuildHeaderMap(lines(0))
            Dim count = 0, errors = 0

            For i As Integer = 1 To lines.Length - 1
                Dim line = lines(i).Trim()
                If String.IsNullOrEmpty(line) Then Continue For
                Try
                    Dim f = Core.CsvHelper.SplitLine(line)
                    Dim G  As Func(Of String, String)         = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD As Func(Of String, String, String) = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF3N_BatchRow()
                    row.Action    = G("Action")
                    row.Carrier   = GD("Carrier",  "FXFM")
                    row.CustType  = GD("CustType", "CC")
                    row.Account   = G("Account")
                    row.Authority = G("Authority")
                    row.Number    = G("Number")
                    row.Item      = G("Item")
                    row.Part      = G("Part")
                    row.Condition = G("Condition")
                    row.PrepdOrCollect = GD("PrepdOrCollect", "NA")
                    row.EffDate   = G("EffDate")
                    row.CanDateItem = G("CanDateItem")
                    row.Alternation = GD("Alternation", "NA")
                    row.ClassRates = GD("ClassRates", "NA")
                    row.ClsTrfAuth = G("ClsTrfAuth")
                    row.ClsTrfNum  = G("ClsTrfNum")
                    row.ClsTrfSec  = G("ClsTrfSec")
                    row.RateEffDate = G("RateEffDate")
                    row.HuType     = GD("HuType", "NA")
                    row.MileageAuth = G("MileageAuth")
                    row.MileageNum  = G("MileageNum")
                    row.MileageRangeLow = G("MileageRangeLow")
                    row.MileageRangeHigh = G("MileageRangeHigh")
                    row.Comments   = G("Comments")
                    row.RT1MinH    = G("RT1MinH")
                    row.RT1MaxH    = G("RT1MaxH")
                    row.RT1AvgMin  = G("RT1AvgMin")
                    row.RT1AvgMax  = G("RT1AvgMax")
                    row.RT1RateType = GD("RT1RateType", "NA")
                    row.RT1Amt     = G("RT1Amt")
                    row.RT2MinH    = G("RT2MinH")
                    row.RT2MaxH    = G("RT2MaxH")
                    row.RT2AvgMin  = G("RT2AvgMin")
                    row.RT2AvgMax  = G("RT2AvgMax")
                    row.RT2RateType = GD("RT2RateType", "NA")
                    row.RT2Amt     = G("RT2Amt")
                    row.RT3MinH    = G("RT3MinH")
                    row.RT3MaxH    = G("RT3MaxH")
                    row.RT3AvgMin  = G("RT3AvgMin")
                    row.RT3AvgMax  = G("RT3AvgMax")
                    row.RT3RateType = GD("RT3RateType", "NA")
                    row.RT3Amt     = G("RT3Amt")
                    row.RT4MinH    = G("RT4MinH")
                    row.RT4MaxH    = G("RT4MaxH")
                    row.RT4AvgMin  = G("RT4AvgMin")
                    row.RT4AvgMax  = G("RT4AvgMax")
                    row.RT4RateType = GD("RT4RateType", "NA")
                    row.RT4Amt     = G("RT4Amt")
                    row.RT5MinH    = G("RT5MinH")
                    row.RT5MaxH    = G("RT5MaxH")
                    row.RT5AvgMin  = G("RT5AvgMin")
                    row.RT5AvgMax  = G("RT5AvgMax")
                    row.RT5RateType = GD("RT5RateType", "NA")
                    row.RT5Amt     = G("RT5Amt")
                    row.RT6MinH    = G("RT6MinH")
                    row.RT6MaxH    = G("RT6MaxH")
                    row.RT6AvgMin  = G("RT6AvgMin")
                    row.RT6AvgMax  = G("RT6AvgMax")
                    row.RT6RateType = GD("RT6RateType", "NA")
                    row.RT6Amt     = G("RT6Amt")
                    row.RT7MinH    = G("RT7MinH")
                    row.RT7MaxH    = G("RT7MaxH")
                    row.RT7AvgMin  = G("RT7AvgMin")
                    row.RT7AvgMax  = G("RT7AvgMax")
                    row.RT7RateType = GD("RT7RateType", "NA")
                    row.RT7Amt     = G("RT7Amt")
                    row.RT8MinH    = G("RT8MinH")
                    row.RT8MaxH    = G("RT8MaxH")
                    row.RT8AvgMin  = G("RT8AvgMin")
                    row.RT8AvgMax  = G("RT8AvgMax")
                    row.RT8RateType = GD("RT8RateType", "NA")
                    row.RT8Amt     = G("RT8Amt")
                    row.RT9MinH    = G("RT9MinH")
                    row.RT9MaxH    = G("RT9MaxH")
                    row.RT9AvgMin  = G("RT9AvgMin")
                    row.RT9AvgMax  = G("RT9AvgMax")
                    row.RT9RateType = GD("RT9RateType", "NA")
                    row.RT9Amt     = G("RT9Amt")
                    row.RT10MinH    = G("RT10MinH")
                    row.RT10MaxH    = G("RT10MaxH")
                    row.RT10AvgMin  = G("RT10AvgMin")
                    row.RT10AvgMax  = G("RT10AvgMax")
                    row.RT10RateType = GD("RT10RateType", "NA")
                    row.RT10Amt     = G("RT10Amt")
                    row.LastMaintDate = G("LastMaintDate")
                    row.OperatorId  = G("OperatorId")
                    row.Revision    = G("Revision")
                    BatchRows.Add(row)
                    count += 1
                Catch
                    errors += 1
                End Try
            Next

            BannerMessage = If(errors > 0,
                String.Format("{0} rows imported, {1} skipped from {2}.", count, errors, IO.Path.GetFileName(dlg.FileName)),
                String.Format("{0} rows imported from {1}.", count, IO.Path.GetFileName(dlg.FileName)))
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3N_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "Condition,PrepdOrCollect,EffDate,HuType," &
                             "RT1MinH,RT1MaxH,RT1RateType,RT1Amt," &
                             "LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.Condition, r.PrepdOrCollect, r.EffDate, r.HuType,
                        r.RT1MinH, r.RT1MaxH, r.RT1RateType, r.RT1Amt,
                        r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
        End Sub

        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3N_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.Condition = G("CONDITION")
                br.PrepdOrCollect = GD("PREPDORCOLLECT", "NA")
                br.EffDate   = G("EFFDATE")
                br.CanDateItem = G("CANDATEITEM")
                br.Alternation = GD("ALTERNATION", "NA")
                br.ClassRates = GD("CLASSRATES", "NA")
                br.ClsTrfAuth = G("CLSTRFAUTH")
                br.ClsTrfNum  = G("CLSTRFNUM")
                br.ClsTrfSec  = G("CLSTRFSEC")
                br.RateEffDate = G("RATEEFFDATE")
                br.HuType     = GD("HUTYPE", "NA")
                br.MileageAuth = G("MILEAGEAUTH")
                br.MileageNum  = G("MILEAGENUM")
                br.MileageRangeLow = G("MILEAGERANGELOW")
                br.MileageRangeHigh = G("MILEAGERANGEHIGH")
                br.Comments   = G("COMMENTS")
                br.RT1MinH    = G("RT1MINH")
                br.RT1MaxH    = G("RT1MAXH")
                br.RT1AvgMin  = G("RT1AVGMIN")
                br.RT1AvgMax  = G("RT1AVGMAX")
                br.RT1RateType = GD("RT1RATETYPE", "NA")
                br.RT1Amt     = G("RT1AMT")
                br.RT2MinH    = G("RT2MINH")
                br.RT2MaxH    = G("RT2MAXH")
                br.RT2AvgMin  = G("RT2AVGMIN")
                br.RT2AvgMax  = G("RT2AVGMAX")
                br.RT2RateType = GD("RT2RATETYPE", "NA")
                br.RT2Amt     = G("RT2AMT")
                br.RT3MinH    = G("RT3MINH")
                br.RT3MaxH    = G("RT3MAXH")
                br.RT3AvgMin  = G("RT3AVGMIN")
                br.RT3AvgMax  = G("RT3AVGMAX")
                br.RT3RateType = GD("RT3RATETYPE", "NA")
                br.RT3Amt     = G("RT3AMT")
                br.RT4MinH    = G("RT4MINH")
                br.RT4MaxH    = G("RT4MAXH")
                br.RT4AvgMin  = G("RT4AVGMIN")
                br.RT4AvgMax  = G("RT4AVGMAX")
                br.RT4RateType = GD("RT4RATETYPE", "NA")
                br.RT4Amt     = G("RT4AMT")
                br.RT5MinH    = G("RT5MINH")
                br.RT5MaxH    = G("RT5MAXH")
                br.RT5AvgMin  = G("RT5AVGMIN")
                br.RT5AvgMax  = G("RT5AVGMAX")
                br.RT5RateType = GD("RT5RATETYPE", "NA")
                br.RT5Amt     = G("RT5AMT")
                br.RT6MinH    = G("RT6MINH")
                br.RT6MaxH    = G("RT6MAXH")
                br.RT6AvgMin  = G("RT6AVGMIN")
                br.RT6AvgMax  = G("RT6AVGMAX")
                br.RT6RateType = GD("RT6RATETYPE", "NA")
                br.RT6Amt     = G("RT6AMT")
                br.RT7MinH    = G("RT7MINH")
                br.RT7MaxH    = G("RT7MAXH")
                br.RT7AvgMin  = G("RT7AVGMIN")
                br.RT7AvgMax  = G("RT7AVGMAX")
                br.RT7RateType = GD("RT7RATETYPE", "NA")
                br.RT7Amt     = G("RT7AMT")
                br.RT8MinH    = G("RT8MINH")
                br.RT8MaxH    = G("RT8MAXH")
                br.RT8AvgMin  = G("RT8AVGMIN")
                br.RT8AvgMax  = G("RT8AVGMAX")
                br.RT8RateType = GD("RT8RATETYPE", "NA")
                br.RT8Amt     = G("RT8AMT")
                br.RT9MinH    = G("RT9MINH")
                br.RT9MaxH    = G("RT9MAXH")
                br.RT9AvgMin  = G("RT9AVGMIN")
                br.RT9AvgMax  = G("RT9AVGMAX")
                br.RT9RateType = GD("RT9RATETYPE", "NA")
                br.RT9Amt     = G("RT9AMT")
                br.RT10MinH   = G("RT10MINH")
                br.RT10MaxH   = G("RT10MAXH")
                br.RT10AvgMin = G("RT10AVGMIN")
                br.RT10AvgMax = G("RT10AVGMAX")
                br.RT10RateType = GD("RT10RATETYPE", "NA")
                br.RT10Amt    = G("RT10AMT")
                br.LastMaintDate = G("LASTMAINTDATE")
                br.OperatorId  = G("OPERATORID")
                br.Revision    = G("REVISION")
                BatchRows.Add(br)
            Next
        End Sub

        ' ── Helpers ──────────────────────────────────────────────────
        Private Shared Function ParseCarrier(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum)
        End Function

        Private Shared Function ParseCustType(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum)
        End Function

        Private Shared Function Q(s As String) As String
            If s Is Nothing Then Return ""
            If s.Contains(",") Then Return """" & s.Replace("""", """""") & """"
            Return s
        End Function

    End Class


    ' ──────────────────────────────────────────────────────────────
    '  FXF4M — Earned Discount
    '  Screen: DSNM1D0-4M
    '  Methods: getItem, getItems, cancelItem, deleteItem
    '  GET/GETALL/CANCEL/DELETE — no addItem, no changeItem
    '  Has LoadAccountCommand (like FXF3A)
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF4M_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand      = New RelayCommand(AddressOf ExecuteRunBatch,
                                                     Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand        = New RelayCommand(Sub() BatchRows.Add(New FXF4M_BatchRow()))
            _clearBatchCommand    = New RelayCommand(Sub() BatchRows.Clear(),
                                                     Function() Not _isBusy)
            _clearResultsCommand  = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadAccountCommand   = New RelayCommand(AddressOf ExecuteLoadAccount,
                                                     Function() _session.IsConnected AndAlso
                                                                Not String.IsNullOrWhiteSpace(_quickAccount) AndAlso
                                                                Not _isBusy)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF4M_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Quick-load bar ───────────────────────────────────────────
        Private _quickCarrier  As String = "FXFM"
        Private _quickCustType As String = "CC"
        Private _quickAccount  As String = ""

        Public Property QuickCarrier As String
            Get
                Return _quickCarrier
            End Get
            Set(v As String)
                SetField(_quickCarrier, v)
            End Set
        End Property
        Public Property QuickCustType As String
            Get
                Return _quickCustType
            End Get
            Set(v As String)
                SetField(_quickCustType, v)
            End Set
        End Property
        Public Property QuickAccount As String
            Get
                Return _quickAccount
            End Get
            Set(v As String)
                SetField(_quickAccount, v)
                _loadAccountCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF4M_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF4M_BatchRow)

        ' ── Progress / state ─────────────────────────────────────────
        Private _isBusy As Boolean
        Public Property IsBusy As Boolean
            Get
                Return _isBusy
            End Get
            Set(v As Boolean)
                SetField(_isBusy, v)
                _runBatchCommand.RaiseCanExecuteChanged()
                _clearBatchCommand.RaiseCanExecuteChanged()
                _loadAccountCommand.RaiseCanExecuteChanged()
                _loadCsvCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        Private _progressCurrent As Integer
        Public Property ProgressCurrent As Integer
            Get
                Return _progressCurrent
            End Get
            Set(v As Integer)
                SetField(_progressCurrent, v)
            End Set
        End Property

        Private _progressTotal As Integer
        Public Property ProgressTotal As Integer
            Get
                Return _progressTotal
            End Get
            Set(v As Integer)
                SetField(_progressTotal, v)
            End Set
        End Property

        Private _progressText As String = ""
        Public Property ProgressText As String
            Get
                Return _progressText
            End Get
            Set(v As String)
                SetField(_progressText, v)
            End Set
        End Property

        Private _bannerMessage As String = ""
        Public Property BannerMessage As String
            Get
                Return _bannerMessage
            End Get
            Set(v As String)
                SetField(_bannerMessage, v)
            End Set
        End Property

        Public ReadOnly Property BannerIsError As Boolean
            Get
                Return _bannerMessage.Contains("Error") OrElse _bannerMessage.Contains("failed")
            End Get
        End Property

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _runBatchCommand      As RelayCommand
        Private ReadOnly _addRowCommand        As RelayCommand
        Private ReadOnly _clearBatchCommand    As RelayCommand
        Private ReadOnly _clearResultsCommand  As RelayCommand
        Private ReadOnly _exportResultsCommand As RelayCommand
        Private ReadOnly _loadAccountCommand   As RelayCommand
        Private ReadOnly _loadCsvCommand       As RelayCommand
        Private ReadOnly _selectAllCommand As RelayCommand

        Public ReadOnly Property RunBatchCommand      As RelayCommand
            Get
                Return _runBatchCommand
            End Get
        End Property
        Public ReadOnly Property AddRowCommand        As RelayCommand
            Get
                Return _addRowCommand
            End Get
        End Property
        Public ReadOnly Property ClearBatchCommand    As RelayCommand
            Get
                Return _clearBatchCommand
            End Get
        End Property
        Public ReadOnly Property ClearResultsCommand  As RelayCommand
            Get
                Return _clearResultsCommand
            End Get
        End Property
        Public ReadOnly Property ExportResultsCommand As RelayCommand
            Get
                Return _exportResultsCommand
            End Get
        End Property
        Public ReadOnly Property LoadAccountCommand   As RelayCommand
            Get
                Return _loadAccountCommand
            End Get
        End Property
        Public ReadOnly Property LoadCsvCommand As RelayCommand
            Get
                Return _loadCsvCommand
            End Get
        End Property
        Public ReadOnly Property SelectAllCommand As RelayCommand
            Get
                Return _selectAllCommand
            End Get
        End Property

        ' ── Quick Load ───────────────────────────────────────────────
        Private Async Sub ExecuteLoadAccount()
            IsBusy = True
            BannerMessage = ""
            Try
                Dim carrier  = _quickCarrier
                Dim custType = _quickCustType
                Dim acct     = _quickAccount

                Dim itemList = Await Task.Run(Function()
                    Return _session.FXF4M.getItems(
                        ParseCarrier(carrier),
                        ParseCustType(custType),
                        acct, True)
                End Function)

                Await Application.Current.Dispatcher.InvokeAsync(Sub()
                    BatchRows.Clear()
                    If itemList Is Nothing OrElse itemList.Count = 0 Then
                        BannerMessage = "No active items found for account " & acct
                        Return
                    End If
                    For i As Integer = 0 To itemList.Count - 1
                        Dim hdr = itemList(i)
                        Dim row As New FXF4M_BatchRow
                        row.Action    = "GET"
                        row.Carrier   = carrier
                        row.CustType  = custType
                        row.Account   = acct
                        row.Authority = hdr.auhority
                        row.Number    = hdr.number
                        row.Item      = hdr.item
                        row.Part      = hdr.payRule
                        row.EffDate   = FormatDate(hdr.effDate)
                        row.ExpDate   = FormatDate(hdr.expDate)
                        BatchRows.Add(row)
                    Next
                    BannerMessage = itemList.Count & " items loaded for account " & acct
                End Sub)

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.Invoke(Sub()
                    BannerMessage = "Account not found: " & _quickAccount
                End Sub)
            Catch ex As Exception
                Application.Current.Dispatcher.Invoke(Sub()
                    BannerMessage = "Error: " & ex.Message
                End Sub)
            Finally
                IsBusy = False
            End Try
        End Sub

        ' ── Run Batch ────────────────────────────────────────────────
        Private Async Sub ExecuteRunBatch()
            IsBusy = True
            BannerMessage = ""

            If Not _session.IsConnected Then
                BannerMessage = "Not connected to mainframe. Please connect before running the batch."
                IsBusy = False
                Return
            End If

            Dim selectedRows As New List(Of FXF4M_BatchRow)
            For Each r As FXF4M_BatchRow In BatchRows
                If r.IsSelected Then selectedRows.Add(r)
            Next
            If selectedRows.Count = 0 Then
                BannerMessage = "No rows selected. Use the checkboxes to select rows to process."
                IsBusy = False
                Return
            End If
            Dim total   = selectedRows.Count
            Dim ok      = 0
            Dim err     = 0
            Dim skipped = 0
            ProgressTotal   = total
            ProgressCurrent = 0

            For i As Integer = 0 To selectedRows.Count - 1
                Dim row = selectedRows(i)
                ProgressCurrent = i + 1
                ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", i + 1, total, row.Action, row.Account)

                If String.IsNullOrWhiteSpace(row.Action) Then
                    row.Status = OperationStatus.Skipped
                    skipped   += 1
                    Continue For
                End If

                row.Status        = OperationStatus.Running
                row.StatusMessage = ""

                Try
                    Await Task.Run(Sub() ProcessRow(row))
                    ok += 1
                Catch ex As Exception
                    err += 1
                End Try

                Await Task.Delay(10)
            Next

            ProgressText  = String.Format("Complete — {0} OK, {1} errors, {2} skipped", ok, err, skipped)
            BannerMessage = ProgressText
            IsBusy        = False
        End Sub

        ' ── Process single row (background thread) ───────────────────
        Private Sub ProcessRow(row As FXF4M_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "ADD", "CHANGE"
                        If Not row.HasEdTablePayload() Then
                            Throw New InvalidOperationException("FXF4M ADD/CHANGE requires edTable payload fields (OpcoTtl, FxfTtl, EdPct, Dt, dates, or update user).")
                        End If

                        Dim payRule = If(String.IsNullOrWhiteSpace(row.Part), row.PayRule, row.Part)
                        _session.FXF4M.setEdTable(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, payRule,
                            row.ToEdTableCollection(row.Action))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "GET"
                        Dim it = _session.FXF4M.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF4M_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "GETALL"
                        Dim items = _session.FXF4M.getItems(carrier, custType, row.Account)
                        If items IsNot Nothing Then
                            For j As Integer = 0 To items.Count - 1
                                Dim header = items(j)
                                Dim resultRow As New FXF4M_BatchRow
                                resultRow.Carrier    = row.Carrier
                                resultRow.CustType   = row.CustType
                                resultRow.Account    = row.Account
                                resultRow.Authority  = header.auhority
                                resultRow.Number     = header.number
                                resultRow.Item       = header.item
                                resultRow.Part       = header.payRule
                                resultRow.EffDate    = FormatDate(header.effDate)
                                resultRow.ExpDate    = FormatDate(header.expDate)
                                Application.Current.Dispatcher.InvokeAsync(Sub()
                                    Results.Add(resultRow)
                                End Sub)
                            Next
                        End If
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF4M.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF4M.deleteItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case Else
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status        = OperationStatus.Skipped
                            row.StatusMessage = "Unknown action: " & row.Action
                        End Sub)
                End Select

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Account not found: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                DebugLogger.LogError(row, ex)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
                DebugLogger.LogError(row, ex)
                Throw
            End Try
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        ' -- Import from CSV
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF4M Batch from CSV"
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim lines As String()
            Try
                lines = IO.File.ReadAllLines(dlg.FileName)
            Catch ex As Exception
                BannerMessage = "Cannot read file: " & ex.Message
                Return
            End Try

            If lines.Length < 2 Then
                BannerMessage = "CSV has no data rows."
                Return
            End If

            Dim hdr = Core.CsvHelper.BuildHeaderMap(lines(0))
            Dim count = 0, errors = 0

            For i As Integer = 1 To lines.Length - 1
                Dim line = lines(i).Trim()
                If String.IsNullOrEmpty(line) Then Continue For
                Try
                    Dim f = Core.CsvHelper.SplitLine(line)
                    Dim G  As Func(Of String, String)         = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD As Func(Of String, String, String) = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF4M_BatchRow()
                    row.Action    = G("Action")
                    row.Carrier   = GD("Carrier",  "FXFM")
                    row.CustType  = GD("CustType", "CC")
                    row.Account   = G("Account")
                    row.Authority = G("Authority")
                    row.Number    = G("Number")
                    row.Item      = G("Item")
                    row.Part      = G("Part")
                    row.PayRule   = G("PayRule")
                    row.OpcoTtl   = G("OpcoTtl")
                    row.FxfTtl    = G("FxfTtl")
                    row.EdPct     = G("EdPct")
                    row.Dt        = G("Dt")
                    row.EffDate   = G("EffDate")
                    row.ExpDate   = G("ExpDate")
                    row.UpdDate   = G("UpdDate")
                    row.UpdUserId = G("UpdUserId")
                    BatchRows.Add(row)
                    count += 1
                Catch
                    errors += 1
                End Try
            Next

            BannerMessage = If(errors > 0,
                String.Format("{0} rows imported, {1} skipped from {2}.", count, errors, IO.Path.GetFileName(dlg.FileName)),
                String.Format("{0} rows imported from {1}.", count, IO.Path.GetFileName(dlg.FileName)))
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF4M_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "EffDate,ExpDate,LastMaintDate,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.EffDate, r.ExpDate, r.LastMaintDate,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
        End Sub

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF4M_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.PayRule   = G("PAYRULE")
                br.EffDate   = G("EFFDATE")
                br.ExpDate   = G("EXPDATE")
                br.PrepaidInbound   = If(G("PREPAIDIN").ToUpper()   = "Y", "Y", "N")
                br.PrepaidOutbound  = If(G("PREPAIDOUT").ToUpper()  = "Y", "Y", "N")
                br.CollectInbound   = If(G("COLLECTIN").ToUpper()   = "Y", "Y", "N")
                br.CollectOutbound  = If(G("COLLECTOUT").ToUpper()  = "Y", "Y", "N")
                br.ThirdParty       = If(G("THIRDPARTY").ToUpper()  = "Y", "Y", "N")
                br.OpcoTtl          = G("OPCOTTL")
                br.FxfTtl           = G("FXFTTL")
                br.EdPct            = G("EDPCT")
                br.Dt               = G("DT")
                br.UpdDate          = G("UPDDATE")
                br.UpdUserId        = G("UPDUSERID")
                BatchRows.Add(br)
            Next
        End Sub

        ' ── Helpers ──────────────────────────────────────────────────
        Private Shared Function ParseCarrier(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum)
        End Function

        Private Shared Function ParseCustType(s As String) _
                As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum
            Return DirectCast([Enum].Parse(
                GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum), s, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum)
        End Function

        Private Shared Function FormatDate(d As Date) As String
            If d = Nothing Then Return ""
            Return d.ToString("MM/dd/yy")
        End Function

        Private Shared Function Q(s As String) As String
            If s Is Nothing Then Return ""
            If s.Contains(",") Then Return """" & s.Replace("""", """""") & """"
            Return s
        End Function

    End Class

End Namespace
