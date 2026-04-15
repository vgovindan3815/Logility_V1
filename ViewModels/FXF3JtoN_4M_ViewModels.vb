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
Imports FXF3A_Tool.Core
Imports FXF3A_Tool.Models

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
                        parms.fromCust.type = ParseCustType(row.CustType)
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
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
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
                    row.CustType    = GD("CustType", "CC")
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
                br.CustType     = GD("CUSTTYPE", "CC")
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
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
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
                sw.WriteLine("Timestamp,Carrier,MatrixName,MatrixEffDate,MatrixCancelDate,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, Q(r.MatrixName),
                        r.MatrixEffDate, r.MatrixCancelDate,
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
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
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
                br.Release          = If(G("RELEASE").ToUpper()          = "Y", "Y", "N")
                br.RateManual       = If(G("RATEMANUAL").ToUpper()       = "Y", "Y", "N")
                br.EwrCls           = If(G("EWRCLS").ToUpper()           = "Y", "Y", "N")
                br.EwrLowRate       = If(G("EWRLOWRATE").ToUpper()       = "Y", "Y", "N")
                br.EwrHighRate      = If(G("EWRHIGHRATE").ToUpper()      = "Y", "Y", "N")
                br.EwrHighestVolByWgt = If(G("EWRHIGHESTVOLBYWGT").ToUpper() = "Y", "Y", "N")
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
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
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
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    BannerMessage = "Account not found: " & _quickAccount
                End Sub)
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
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
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                ' Warning — not rethrown
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = "Invalid numeric value: " & ex.Message
                End Sub)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message &
                        If(Not String.IsNullOrWhiteSpace(ex.ScreenDump), " [screen dump available]", "")
                End Sub)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status        = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
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
                br.EffDate   = G("EFFDATE")
                br.ExpDate   = G("EXPDATE")
                br.PrepaidInbound   = If(G("PREPAIDIN").ToUpper()   = "Y", "Y", "N")
                br.PrepaidOutbound  = If(G("PREPAIDOUT").ToUpper()  = "Y", "Y", "N")
                br.CollectInbound   = If(G("COLLECTIN").ToUpper()   = "Y", "Y", "N")
                br.CollectOutbound  = If(G("COLLECTOUT").ToUpper()  = "Y", "Y", "N")
                br.ThirdParty       = If(G("THIRDPARTY").ToUpper()  = "Y", "Y", "N")
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
