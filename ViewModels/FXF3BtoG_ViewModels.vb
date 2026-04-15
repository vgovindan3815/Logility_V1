Option Strict On
Option Explicit On

Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports FedEx.PABST.SS.Exceptions
Imports FXF3A_Tool.Core
Imports FXF3A_Tool.Models

' ================================================================
'  FXF3B through FXF3G ViewModels
'  All follow the identical pattern as FXF3A_ViewModel.
'
'  Per-screen differences:
'    FXF3B — GET/ADD/CHANGE/DELETE only. No cancelItem, no releaseItem, no getItems.
'    FXF3C — GET/ADD/CHANGE/DELETE only. No cancelItem, no getItems.
'    FXF3D — GET/ADD/CHANGE/CANCEL/DELETE. Has cancelItem, no getItems.
'    FXF3E — GET/ADD/CHANGE/CANCEL/DELETE. Has cancelItem, no getItems.
'    FXF3F — GET/ADD/CHANGE/CANCEL/DELETE. Has cancelItem, no getItems.
'    FXF3G — GET/ADD/CHANGE/CANCEL/DELETE. Has cancelItem, no getItems.
'
'  All B-G screens have an extra key field: Part.
' ================================================================

Namespace ViewModels

    ' ──────────────────────────────────────────────────────────────
    '  FXF3B — Discounts by State/Terminal
    '  Screen: DSNM1ST-3B
    '  Methods: getItem, addItem, changeItem, deleteItem
    '  NO cancelItem, NO releaseItem, NO getItems
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3B_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand     = New RelayCommand(AddressOf ExecuteRunBatch,
                                                    Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand       = New RelayCommand(Sub() BatchRows.Add(New FXF3B_BatchRow()))
            _clearBatchCommand   = New RelayCommand(Sub() BatchRows.Clear(),
                                                    Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3B_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3B_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3B_BatchRow)

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
        Private ReadOnly _selectAllCommand     As RelayCommand

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

            Dim selectedRows As New List(Of FXF3B_BatchRow)
            For Each r As FXF3B_BatchRow In BatchRows
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
        Private Sub ProcessRow(row As FXF3B_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3B.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3B_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3B.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass(), (row.Release = "Y"))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3B.changeItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3B.deleteItem(
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

        ' ── Import from CSV ──────────────────────────────────────────
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF3B Batch from CSV"
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
                    Dim G  As Func(Of String, String)  = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD As Func(Of String, String, String) = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF3B_BatchRow()
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

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3B_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.Release  = If(G("RELEASE").ToUpper()  = "Y", "Y", "N")
                br.PrepdIn  = If(G("PREPDIN").ToUpper()  = "Y", "Y", "N")
                br.PrepdOut = If(G("PREPDOUT").ToUpper() = "Y", "Y", "N")
                br.CollIn   = If(G("COLLIN").ToUpper()   = "Y", "Y", "N")
                br.CollOut  = If(G("COLLOUT").ToUpper()  = "Y", "Y", "N")
                BatchRows.Add(br)
            Next
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3B_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "GT1IncExc,GT1Dir,GT1Type,GT1R1Name,GT1R1Cty," &
                             "GT2IncExc,GT2Dir,GT2Type,GT2R1Name,GT2R1Cty," &
                             "RateEff,ClsZip,LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.GT1IncExc, r.GT1Dir, r.GT1Type, Q(r.GT1R1Name), Q(r.GT1R1Country),
                        r.GT2IncExc, r.GT2Dir, r.GT2Type, Q(r.GT2R1Name), Q(r.GT2R1Country),
                        r.RateEff, r.ClsZip, r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
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
    '  FXF3C — Customer Geography Discounts
    '  Screen: DSNM1GE-3C
    '  Methods: getItem, addItem, changeItem, deleteItem
    '  NO cancelItem, NO getItems
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3C_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand     = New RelayCommand(AddressOf ExecuteRunBatch,
                                                    Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand       = New RelayCommand(Sub() BatchRows.Add(New FXF3C_BatchRow()))
            _clearBatchCommand   = New RelayCommand(Sub() BatchRows.Clear(),
                                                    Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3C_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3C_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3C_BatchRow)

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
        Private ReadOnly _selectAllCommand     As RelayCommand

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

            Dim selectedRows As New List(Of FXF3C_BatchRow)
            For Each r As FXF3C_BatchRow In BatchRows
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
        Private Sub ProcessRow(row As FXF3C_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3C.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3C_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3C.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3C.changeItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3C.deleteItem(
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
            dlg.Title  = "Import FXF3C Batch from CSV"
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

                    Dim row As New FXF3C_BatchRow()
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

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3C_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.Release   = If(G("RELEASE").ToUpper() = "Y", "Y", "N")
                BatchRows.Add(br)
            Next
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3C_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "R1PlusMinus,R1Dir,R1Type,R1Name,R1State,R1Country," &
                             "SrvDaysLo,SrvDaysHi,LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.R1PlusMinus, r.R1Dir, r.R1Type, Q(r.R1Name), Q(r.R1State), Q(r.R1Country),
                        r.SrvDaysLo, r.SrvDaysHi, r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
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
    '  FXF3D — Customer Product Discounts
    '  Screen: DSNM1PR-3D
    '  Methods: getItem, addItem, changeItem, cancelItem, deleteItem
    '  NO getItems
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3D_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand     = New RelayCommand(AddressOf ExecuteRunBatch,
                                                    Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand       = New RelayCommand(Sub() BatchRows.Add(New FXF3D_BatchRow()))
            _clearBatchCommand   = New RelayCommand(Sub() BatchRows.Clear(),
                                                    Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3D_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3D_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3D_BatchRow)

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
        Private ReadOnly _selectAllCommand     As RelayCommand

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

            Dim selectedRows As New List(Of FXF3D_BatchRow)
            For Each r As FXF3D_BatchRow In BatchRows
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
        Private Sub ProcessRow(row As FXF3D_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3D.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3D_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3D.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3D.changeItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3D.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3D.deleteItem(
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
                    row.StatusMessage = "Invalid cancel date: " & ex.Message
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
            dlg.Title  = "Import FXF3D Batch from CSV"
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

                    Dim row As New FXF3D_BatchRow()
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

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3D_BatchRow()
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

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3D_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "EffDate,CanDateItem,ExcCls,P1Type,P1Prod1,P1Prod2," &
                             "LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.EffDate, r.CanDateItem, r.ExcCls,
                        r.P1Type, Q(r.P1Prod1), Q(r.P1Prod2),
                        r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
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
    '  FXF3E — Customer Rates
    '  Screen: DSNM2MB-3E
    '  Methods: getItem, addItem, changeItem, cancelItem, deleteItem
    '  NO getItems
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3E_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand     = New RelayCommand(AddressOf ExecuteRunBatch,
                                                    Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand       = New RelayCommand(Sub() BatchRows.Add(New FXF3E_BatchRow()))
            _clearBatchCommand   = New RelayCommand(Sub() BatchRows.Clear(),
                                                    Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3E_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3E_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3E_BatchRow)

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
        Private ReadOnly _selectAllCommand     As RelayCommand

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

            Dim selectedRows As New List(Of FXF3E_BatchRow)
            For Each r As FXF3E_BatchRow In BatchRows
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
        Private Sub ProcessRow(row As FXF3E_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3E.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3E_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3E.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3E.changeItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3E.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3E.deleteItem(
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
                    row.StatusMessage = "Invalid cancel date: " & ex.Message
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
            dlg.Title  = "Import FXF3E Batch from CSV"
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

                    Dim row As New FXF3E_BatchRow()
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

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3E_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.RateManually = If(G("RATEMANUALLY").ToUpper() = "Y", "Y", "N")
                BatchRows.Add(br)
            Next
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3E_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "Condition,PrepdOrCollect,EffDate,Alternation," &
                             "RT1Wgt,RT1Type,RT1Amt,LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        Q(r.Condition), r.PrepdOrCollect, r.EffDate, r.Alternation,
                        r.RT1Wgt, r.RT1Type, r.RT1Amt,
                        r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
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
    '  FXF3F — Customer Discounts/Adjustments
    '  Screen: DSNM3PP-3F
    '  Methods: getItem, addItem, changeItem, cancelItem, deleteItem
    '  NO getItems
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3F_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand     = New RelayCommand(AddressOf ExecuteRunBatch,
                                                    Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand       = New RelayCommand(Sub() BatchRows.Add(New FXF3F_BatchRow()))
            _clearBatchCommand   = New RelayCommand(Sub() BatchRows.Clear(),
                                                    Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3F_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3F_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3F_BatchRow)

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
        Private ReadOnly _selectAllCommand     As RelayCommand

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

            Dim selectedRows As New List(Of FXF3F_BatchRow)
            For Each r As FXF3F_BatchRow In BatchRows
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
        Private Sub ProcessRow(row As FXF3F_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3F.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3F_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3F.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3F.changeItem(
                            carrier, custType, row.Account,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3F.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3F.deleteItem(
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
                    row.StatusMessage = "Invalid cancel date: " & ex.Message
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
            dlg.Title  = "Import FXF3F Batch from CSV"
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

                    Dim row As New FXF3F_BatchRow()
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

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3F_BatchRow()
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

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3F_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "AdjType,Condition,PrepdOrCollect,AppRule," &
                             "RT1Wgt,RT1DiscAdjDir,RT1DiscAdjUnits,RT1Amt," &
                             "LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.AdjType, Q(r.Condition), r.PrepdOrCollect, r.AppRule,
                        r.RT1Wgt, r.RT1DiscAdjDir, r.RT1DiscAdjUnits, r.RT1Amt,
                        r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
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
    '  FXF3G — Customer Charges/Allowances
    '  Screen: DSNM1AP-3G
    '  Methods: getItem, addItem, changeItem, cancelItem, deleteItem
    '  NO getItems
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3G_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand     = New RelayCommand(AddressOf ExecuteRunBatch,
                                                    Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            _addRowCommand       = New RelayCommand(Sub() BatchRows.Add(New FXF3G_BatchRow()))
            _clearBatchCommand   = New RelayCommand(Sub() BatchRows.Clear(),
                                                    Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadCsvCommand = New RelayCommand(AddressOf ExecuteLoadCsv, Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3G_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
        End Sub

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3G_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3G_BatchRow)

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
        Private ReadOnly _selectAllCommand     As RelayCommand

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

            Dim selectedRows As New List(Of FXF3G_BatchRow)
            For Each r As FXF3G_BatchRow In BatchRows
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
        Private Sub ProcessRow(row As FXF3G_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        Dim it = _session.FXF3G.getItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part)
                        Dim resultRow As New FXF3G_BatchRow
                        resultRow.Carrier  = row.Carrier
                        resultRow.CustType = row.CustType
                        resultRow.FromItemClass(it)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            Results.Add(resultRow)
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3G.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass(), (row.Release = "Y"))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3G.changeItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.ToItemClass())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3G.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item, row.Part,
                            row.GetCancelDate())
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3G.deleteItem(
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
                    row.StatusMessage = "Invalid cancel date: " & ex.Message
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
            dlg.Title  = "Import FXF3G Batch from CSV"
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

                    Dim row As New FXF3G_BatchRow()
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

        ' ── Load rows from Excel import ──────────────────────────────
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3G_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Part      = G("PART")
                br.Release   = If(G("RELEASE").ToUpper() = "Y", "Y", "N")
                BatchRows.Add(br)
            Next
        End Sub

        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3G_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item,Part," &
                             "PrepdOrCollect,EffDate,S1Cond,S1Desc,S1Type,S1Amount," &
                             "LastMaintDate,OperatorId,Status")
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item), Q(r.Part),
                        r.PrepdOrCollect, r.EffDate,
                        Q(r.S1Cond), Q(r.S1Desc), Q(r.S1Type), r.S1Amount,
                        r.LastMaintDate, r.OperatorId,
                        r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
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

End Namespace
