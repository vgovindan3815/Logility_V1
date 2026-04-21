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

            Await _session.RunOnSessionThreadAsync(Sub()
                For i As Integer = 0 To selectedRows.Count - 1
                    Dim row = selectedRows(i)
                    Dim idx = i

                    Application.Current.Dispatcher.InvokeAsync(Sub()
                        ProgressCurrent = idx + 1
                        ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", idx + 1, total, row.Action, row.Account)
                        row.Status        = OperationStatus.Running
                        row.StatusMessage = ""
                    End Sub)

                    If String.IsNullOrWhiteSpace(row.Action) Then
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                        End Sub)
                        skipped += 1
                        Continue For
                    End If

                    Try
                        ProcessRow(row)
                        ok += 1
                    Catch ex As Exception
                        err += 1
                    End Try
                Next
            End Sub)

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
                    row.Release   = If(Core.CsvHelper.GetBool(f, hdr, "Release"), "Y", "N")
                    row.PrepdIn   = If(Core.CsvHelper.GetBool(f, hdr, "PrepdIn"),  "Y", "N")
                    row.PrepdOut  = If(Core.CsvHelper.GetBool(f, hdr, "PrepdOut"), "Y", "N")
                    row.CollIn    = If(Core.CsvHelper.GetBool(f, hdr, "CollIn"),   "Y", "N")
                    row.CollOut   = If(Core.CsvHelper.GetBool(f, hdr, "CollOut"),  "Y", "N")
                    row.GT1IncExc = GD("GT1IncExc", "NA")
                    row.GT1Dir    = GD("GT1Dir",    "NA")
                    row.GT1Type   = GD("GT1Type",   "NA")
                    row.GT1R1Name    = G("GT1R1Name")
                    row.GT1R1Country = G("GT1R1Cty")
                    row.GT1R2Name    = G("GT1R2Name")
                    row.GT1R2Country = G("GT1R2Cty")
                    row.GT1R3Name    = G("GT1R3Name")
                    row.GT1R3Country = G("GT1R3Cty")
                    row.GT1R4Name    = G("GT1R4Name")
                    row.GT1R4Country = G("GT1R4Cty")
                    row.GT1R5Name    = G("GT1R5Name")
                    row.GT1R5Country = G("GT1R5Cty")
                    row.GT2IncExc = GD("GT2IncExc", "NA")
                    row.GT2Dir    = GD("GT2Dir",    "NA")
                    row.GT2Type   = GD("GT2Type",   "NA")
                    row.GT2R1Name    = G("GT2R1Name")
                    row.GT2R1Country = G("GT2R1Cty")
                    row.GT2R2Name    = G("GT2R2Name")
                    row.GT2R2Country = G("GT2R2Cty")
                    row.GT2R3Name    = G("GT2R3Name")
                    row.GT2R3Country = G("GT2R3Cty")
                    row.GT2R4Name    = G("GT2R4Name")
                    row.GT2R4Country = G("GT2R4Cty")
                    row.GT2R5Name    = G("GT2R5Name")
                    row.GT2R5Country = G("GT2R5Cty")
                    row.FsAuth  = G("FsAuth")
                    row.FsNum   = G("FsNum")
                    row.FsItem  = G("FsItem")
                    row.RateEff = G("RateEff")
                    row.ClsZip  = GD("ClsZip",  "NA")
                    row.GenGeoA = GD("GenGeoA", "NA")
                    row.DT1Disc = G("DT1Disc") : row.DT1MinChg = G("DT1MinChg") : row.DT1MaxWgt = G("DT1MaxWgt") : row.DT1FloorMin = G("DT1FloorMin") : row.DT1EffDate = G("DT1EffDate") : row.DT1CanDate = G("DT1CanDate")
                    row.DT2Disc = G("DT2Disc") : row.DT2MinChg = G("DT2MinChg") : row.DT2MaxWgt = G("DT2MaxWgt") : row.DT2FloorMin = G("DT2FloorMin") : row.DT2EffDate = G("DT2EffDate") : row.DT2CanDate = G("DT2CanDate")
                    row.DT3Disc = G("DT3Disc") : row.DT3MinChg = G("DT3MinChg") : row.DT3MaxWgt = G("DT3MaxWgt") : row.DT3FloorMin = G("DT3FloorMin") : row.DT3EffDate = G("DT3EffDate") : row.DT3CanDate = G("DT3CanDate")
                    row.DT4Disc = G("DT4Disc") : row.DT4MinChg = G("DT4MinChg") : row.DT4MaxWgt = G("DT4MaxWgt") : row.DT4FloorMin = G("DT4FloorMin") : row.DT4EffDate = G("DT4EffDate") : row.DT4CanDate = G("DT4CanDate")
                    row.DT5Disc = G("DT5Disc") : row.DT5MinChg = G("DT5MinChg") : row.DT5MaxWgt = G("DT5MaxWgt") : row.DT5FloorMin = G("DT5FloorMin") : row.DT5EffDate = G("DT5EffDate") : row.DT5CanDate = G("DT5CanDate")
                    row.IsSelected = True
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
                br.Release   = If(G("RELEASE").ToUpper()  = "Y", "Y", "N")
                br.PrepdIn   = If(G("PREPDIN").ToUpper()  = "Y", "Y", "N")
                br.PrepdOut  = If(G("PREPDOUT").ToUpper() = "Y", "Y", "N")
                br.CollIn    = If(G("COLLIN").ToUpper()   = "Y", "Y", "N")
                br.CollOut   = If(G("COLLOUT").ToUpper()  = "Y", "Y", "N")
                br.GT1IncExc = GD("GT1INCEXC", "NA")
                br.GT1Dir    = GD("GT1DIR",    "NA")
                br.GT1Type   = GD("GT1TYPE",   "NA")
                br.GT1R1Name    = G("GT1R1NAME")
                br.GT1R1Country = G("GT1R1CTY")
                br.GT1R2Name    = G("GT1R2NAME")
                br.GT1R2Country = G("GT1R2CTY")
                br.GT1R3Name    = G("GT1R3NAME")
                br.GT1R3Country = G("GT1R3CTY")
                br.GT1R4Name    = G("GT1R4NAME")
                br.GT1R4Country = G("GT1R4CTY")
                br.GT1R5Name    = G("GT1R5NAME")
                br.GT1R5Country = G("GT1R5CTY")
                br.GT2IncExc = GD("GT2INCEXC", "NA")
                br.GT2Dir    = GD("GT2DIR",    "NA")
                br.GT2Type   = GD("GT2TYPE",   "NA")
                br.GT2R1Name    = G("GT2R1NAME")
                br.GT2R1Country = G("GT2R1CTY")
                br.GT2R2Name    = G("GT2R2NAME")
                br.GT2R2Country = G("GT2R2CTY")
                br.GT2R3Name    = G("GT2R3NAME")
                br.GT2R3Country = G("GT2R3CTY")
                br.GT2R4Name    = G("GT2R4NAME")
                br.GT2R4Country = G("GT2R4CTY")
                br.GT2R5Name    = G("GT2R5NAME")
                br.GT2R5Country = G("GT2R5CTY")
                br.FsAuth   = G("FSAUTH")
                br.FsNum    = G("FSNUM")
                br.FsItem   = G("FSITEM")
                br.RateEff  = G("RATEEFF")
                br.ClsZip   = GD("CLSZIP",   "NA")
                br.GenGeoA  = GD("GENGEOA",  "NA")
                br.DT1Disc = G("DT1DISC") : br.DT1MinChg = G("DT1MINCHG") : br.DT1MaxWgt = G("DT1MAXWGT") : br.DT1FloorMin = G("DT1FLOORMIN") : br.DT1EffDate = G("DT1EFFDATE") : br.DT1CanDate = G("DT1CANDATE")
                br.DT2Disc = G("DT2DISC") : br.DT2MinChg = G("DT2MINCHG") : br.DT2MaxWgt = G("DT2MAXWGT") : br.DT2FloorMin = G("DT2FLOORMIN") : br.DT2EffDate = G("DT2EFFDATE") : br.DT2CanDate = G("DT2CANDATE")
                br.DT3Disc = G("DT3DISC") : br.DT3MinChg = G("DT3MINCHG") : br.DT3MaxWgt = G("DT3MAXWGT") : br.DT3FloorMin = G("DT3FLOORMIN") : br.DT3EffDate = G("DT3EFFDATE") : br.DT3CanDate = G("DT3CANDATE")
                br.DT4Disc = G("DT4DISC") : br.DT4MinChg = G("DT4MINCHG") : br.DT4MaxWgt = G("DT4MAXWGT") : br.DT4FloorMin = G("DT4FLOORMIN") : br.DT4EffDate = G("DT4EFFDATE") : br.DT4CanDate = G("DT4CANDATE")
                br.DT5Disc = G("DT5DISC") : br.DT5MinChg = G("DT5MINCHG") : br.DT5MaxWgt = G("DT5MAXWGT") : br.DT5FloorMin = G("DT5FLOORMIN") : br.DT5EffDate = G("DT5EFFDATE") : br.DT5CanDate = G("DT5CANDATE")
                br.IsSelected = True
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

            Await _session.RunOnSessionThreadAsync(Sub()
                For i As Integer = 0 To selectedRows.Count - 1
                    Dim row = selectedRows(i)
                    Dim idx = i

                    Application.Current.Dispatcher.InvokeAsync(Sub()
                        ProgressCurrent = idx + 1
                        ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", idx + 1, total, row.Action, row.Account)
                        row.Status        = OperationStatus.Running
                        row.StatusMessage = ""
                    End Sub)

                    If String.IsNullOrWhiteSpace(row.Action) Then
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                        End Sub)
                        skipped += 1
                        Continue For
                    End If

                    Try
                        ProcessRow(row)
                        ok += 1
                    Catch ex As Exception
                        err += 1
                    End Try
                Next
            End Sub)

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
                br.Release      = If(G("RELEASE").ToUpper() = "Y", "Y", "N")
                br.R1PlusMinus  = GD("R1PLUSMINUS", "NA")
                br.R1Dir        = GD("R1DIR",       "NA")
                br.R1Type       = GD("R1TYPE",      "NA")
                br.R1Name       = G("R1NAME")
                br.R1State      = G("R1STATE")
                br.R1Country    = G("R1CTY")
                br.R2PlusMinus  = GD("R2PLUSMINUS", "NA")
                br.R2Dir        = GD("R2DIR",       "NA")
                br.R2Type       = GD("R2TYPE",      "NA")
                br.R2Name       = G("R2NAME")
                br.R2State      = G("R2STATE")
                br.R2Country    = G("R2CTY")
                br.R3PlusMinus  = GD("R3PLUSMINUS", "NA")
                br.R3Dir        = GD("R3DIR",       "NA")
                br.R3Type       = GD("R3TYPE",      "NA")
                br.R3Name       = G("R3NAME")
                br.R3State      = G("R3STATE")
                br.R3Country    = G("R3CTY")
                br.R4PlusMinus  = GD("R4PLUSMINUS", "NA")
                br.R4Dir        = GD("R4DIR",       "NA")
                br.R4Type       = GD("R4TYPE",      "NA")
                br.R4Name       = G("R4NAME")
                br.R4State      = G("R4STATE")
                br.R4Country    = G("R4CTY")
                br.R5PlusMinus  = GD("R5PLUSMINUS", "NA")
                br.R5Dir        = GD("R5DIR",       "NA")
                br.R5Type       = GD("R5TYPE",      "NA")
                br.R5Name       = G("R5NAME")
                br.R5State      = G("R5STATE")
                br.R5Country    = G("R5CTY")
                br.SrvDaysLo    = G("SRVDAYSLO")
                br.SrvDaysHi    = G("SRVDAYSHI")
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

            Await _session.RunOnSessionThreadAsync(Sub()
                For i As Integer = 0 To selectedRows.Count - 1
                    Dim row = selectedRows(i)
                    Dim idx = i

                    Application.Current.Dispatcher.InvokeAsync(Sub()
                        ProgressCurrent = idx + 1
                        ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", idx + 1, total, row.Action, row.Account)
                        row.Status        = OperationStatus.Running
                        row.StatusMessage = ""
                    End Sub)

                    If String.IsNullOrWhiteSpace(row.Action) Then
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                        End Sub)
                        skipped += 1
                        Continue For
                    End If

                    Try
                        ProcessRow(row)
                        ok += 1
                    Catch ex As Exception
                        err += 1
                    End Try
                Next
            End Sub)

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
                br.EffDate     = G("EFFDATE")
                br.CanDateItem = G("CANDATEITEM")
                br.ExcCls      = G("EXCCLS")
                br.ExcMaxW     = G("EXCMAXW")
                br.P1Type      = GD("P1TYPE",  "NA")
                br.P1Prod1     = G("P1PROD1")
                br.P1Prod2     = G("P1PROD2")
                br.P1ExcCls    = G("P1EXCCLS")
                br.P2Type      = GD("P2TYPE",  "NA")
                br.P2Prod1     = G("P2PROD1")
                br.P2Prod2     = G("P2PROD2")
                br.P2ExcCls    = G("P2EXCCLS")
                br.P3Type      = GD("P3TYPE",  "NA")
                br.P3Prod1     = G("P3PROD1")
                br.P3Prod2     = G("P3PROD2")
                br.P3ExcCls    = G("P3EXCCLS")
                br.P4Type      = GD("P4TYPE",  "NA")
                br.P4Prod1     = G("P4PROD1")
                br.P4Prod2     = G("P4PROD2")
                br.P4ExcCls    = G("P4EXCCLS")
                br.P5Type      = GD("P5TYPE",  "NA")
                br.P5Prod1     = G("P5PROD1")
                br.P5Prod2     = G("P5PROD2")
                br.P5ExcCls    = G("P5EXCCLS")
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

            Await _session.RunOnSessionThreadAsync(Sub()
                For i As Integer = 0 To selectedRows.Count - 1
                    Dim row = selectedRows(i)
                    Dim idx = i

                    Application.Current.Dispatcher.InvokeAsync(Sub()
                        ProgressCurrent = idx + 1
                        ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", idx + 1, total, row.Action, row.Account)
                        row.Status        = OperationStatus.Running
                        row.StatusMessage = ""
                    End Sub)

                    If String.IsNullOrWhiteSpace(row.Action) Then
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                        End Sub)
                        skipped += 1
                        Continue For
                    End If

                    Try
                        ProcessRow(row)
                        ok += 1
                    Catch ex As Exception
                        err += 1
                    End Try
                Next
            End Sub)

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
                br.Condition      = G("CONDITION")
                br.PrepdOrCollect = GD("PREPDORCOLLECT", "NA")
                br.EffDate        = G("EFFDATE")
                br.CanDateItem    = G("CANDATEITEM")
                br.Comments       = G("COMMENTS")
                br.Alternation    = GD("ALTERNATION",   "NA")
                br.ClassRates     = GD("CLASSRATES",    "NA")
                br.RateManually   = If(G("RATEMANUALLY").ToUpper() = "Y", "Y", "N")
                br.RT1Wgt  = G("RT1WGT")  : br.RT1Type  = GD("RT1TYPE",  "NA") : br.RT1Amt  = G("RT1AMT")
                br.RT2Wgt  = G("RT2WGT")  : br.RT2Type  = GD("RT2TYPE",  "NA") : br.RT2Amt  = G("RT2AMT")
                br.RT3Wgt  = G("RT3WGT")  : br.RT3Type  = GD("RT3TYPE",  "NA") : br.RT3Amt  = G("RT3AMT")
                br.RT4Wgt  = G("RT4WGT")  : br.RT4Type  = GD("RT4TYPE",  "NA") : br.RT4Amt  = G("RT4AMT")
                br.RT5Wgt  = G("RT5WGT")  : br.RT5Type  = GD("RT5TYPE",  "NA") : br.RT5Amt  = G("RT5AMT")
                br.RT6Wgt  = G("RT6WGT")  : br.RT6Type  = GD("RT6TYPE",  "NA") : br.RT6Amt  = G("RT6AMT")
                br.RT7Wgt  = G("RT7WGT")  : br.RT7Type  = GD("RT7TYPE",  "NA") : br.RT7Amt  = G("RT7AMT")
                br.RT8Wgt  = G("RT8WGT")  : br.RT8Type  = GD("RT8TYPE",  "NA") : br.RT8Amt  = G("RT8AMT")
                br.RT9Wgt  = G("RT9WGT")  : br.RT9Type  = GD("RT9TYPE",  "NA") : br.RT9Amt  = G("RT9AMT")
                br.RT10Wgt = G("RT10WGT") : br.RT10Type = GD("RT10TYPE", "NA") : br.RT10Amt = G("RT10AMT")
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

            Await _session.RunOnSessionThreadAsync(Sub()
                For i As Integer = 0 To selectedRows.Count - 1
                    Dim row = selectedRows(i)
                    Dim idx = i

                    Application.Current.Dispatcher.InvokeAsync(Sub()
                        ProgressCurrent = idx + 1
                        ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", idx + 1, total, row.Action, row.Account)
                        row.Status        = OperationStatus.Running
                        row.StatusMessage = ""
                    End Sub)

                    If String.IsNullOrWhiteSpace(row.Action) Then
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                        End Sub)
                        skipped += 1
                        Continue For
                    End If

                    Try
                        ProcessRow(row)
                        ok += 1
                    Catch ex As Exception
                        err += 1
                    End Try
                Next
            End Sub)

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
                br.AdjType       = GD("ADJTYPE",        "NA")
                br.Condition     = G("CONDITION")
                br.PrepdOrCollect = GD("PREPDORCOLLECT", "NA")
                br.EffDate       = G("EFFDATE")
                br.CanDateItem   = G("CANDATEITEM")
                br.Comments      = G("COMMENTS")
                br.AppRule       = GD("APPRULE",        "NA")
                br.RT1Wgt  = G("RT1WGT")  : br.RT1DiscAdjDir  = GD("RT1DIR",  "NA") : br.RT1DiscAdjUnits  = GD("RT1UNITS",  "NA") : br.RT1DiscAdjType  = GD("RT1TYPE",  "NA") : br.RT1Amt  = G("RT1AMT")
                br.RT2Wgt  = G("RT2WGT")  : br.RT2DiscAdjDir  = GD("RT2DIR",  "NA") : br.RT2DiscAdjUnits  = GD("RT2UNITS",  "NA") : br.RT2DiscAdjType  = GD("RT2TYPE",  "NA") : br.RT2Amt  = G("RT2AMT")
                br.RT3Wgt  = G("RT3WGT")  : br.RT3DiscAdjDir  = GD("RT3DIR",  "NA") : br.RT3DiscAdjUnits  = GD("RT3UNITS",  "NA") : br.RT3DiscAdjType  = GD("RT3TYPE",  "NA") : br.RT3Amt  = G("RT3AMT")
                br.RT4Wgt  = G("RT4WGT")  : br.RT4DiscAdjDir  = GD("RT4DIR",  "NA") : br.RT4DiscAdjUnits  = GD("RT4UNITS",  "NA") : br.RT4DiscAdjType  = GD("RT4TYPE",  "NA") : br.RT4Amt  = G("RT4AMT")
                br.RT5Wgt  = G("RT5WGT")  : br.RT5DiscAdjDir  = GD("RT5DIR",  "NA") : br.RT5DiscAdjUnits  = GD("RT5UNITS",  "NA") : br.RT5DiscAdjType  = GD("RT5TYPE",  "NA") : br.RT5Amt  = G("RT5AMT")
                br.RT6Wgt  = G("RT6WGT")  : br.RT6DiscAdjDir  = GD("RT6DIR",  "NA") : br.RT6DiscAdjUnits  = GD("RT6UNITS",  "NA") : br.RT6DiscAdjType  = GD("RT6TYPE",  "NA") : br.RT6Amt  = G("RT6AMT")
                br.RT7Wgt  = G("RT7WGT")  : br.RT7DiscAdjDir  = GD("RT7DIR",  "NA") : br.RT7DiscAdjUnits  = GD("RT7UNITS",  "NA") : br.RT7DiscAdjType  = GD("RT7TYPE",  "NA") : br.RT7Amt  = G("RT7AMT")
                br.RT8Wgt  = G("RT8WGT")  : br.RT8DiscAdjDir  = GD("RT8DIR",  "NA") : br.RT8DiscAdjUnits  = GD("RT8UNITS",  "NA") : br.RT8DiscAdjType  = GD("RT8TYPE",  "NA") : br.RT8Amt  = G("RT8AMT")
                br.RT9Wgt  = G("RT9WGT")  : br.RT9DiscAdjDir  = GD("RT9DIR",  "NA") : br.RT9DiscAdjUnits  = GD("RT9UNITS",  "NA") : br.RT9DiscAdjType  = GD("RT9TYPE",  "NA") : br.RT9Amt  = G("RT9AMT")
                br.RT10Wgt = G("RT10WGT") : br.RT10DiscAdjDir = GD("RT10DIR", "NA") : br.RT10DiscAdjUnits = GD("RT10UNITS", "NA") : br.RT10DiscAdjType = GD("RT10TYPE", "NA") : br.RT10Amt = G("RT10AMT")
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

            Await _session.RunOnSessionThreadAsync(Sub()
                For i As Integer = 0 To selectedRows.Count - 1
                    Dim row = selectedRows(i)
                    Dim idx = i

                    Application.Current.Dispatcher.InvokeAsync(Sub()
                        ProgressCurrent = idx + 1
                        ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", idx + 1, total, row.Action, row.Account)
                        row.Status        = OperationStatus.Running
                        row.StatusMessage = ""
                    End Sub)

                    If String.IsNullOrWhiteSpace(row.Action) Then
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                        End Sub)
                        skipped += 1
                        Continue For
                    End If

                    Try
                        ProcessRow(row)
                        ok += 1
                    Catch ex As Exception
                        err += 1
                    End Try
                Next
            End Sub)

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
                br.Release        = If(G("RELEASE").ToUpper() = "Y", "Y", "N")
                br.PrepdOrCollect = GD("PREPDORCOLLECT", "NA")
                br.EffDate        = G("EFFDATE")
                br.CanDateItem    = G("CANDATEITEM")
                br.Comments       = G("COMMENTS")
                br.S1Cond   = G("S1COND")  : br.S1Desc   = G("S1DESC")  : br.S1Type   = G("S1TYPE")  : br.S1Amount   = G("S1AMT")  : br.S1CondId   = G("S1CONDID")
                br.S2Cond   = G("S2COND")  : br.S2Desc   = G("S2DESC")  : br.S2Type   = G("S2TYPE")  : br.S2Amount   = G("S2AMT")  : br.S2CondId   = G("S2CONDID")
                br.S3Cond   = G("S3COND")  : br.S3Desc   = G("S3DESC")  : br.S3Type   = G("S3TYPE")  : br.S3Amount   = G("S3AMT")  : br.S3CondId   = G("S3CONDID")
                br.S4Cond   = G("S4COND")  : br.S4Desc   = G("S4DESC")  : br.S4Type   = G("S4TYPE")  : br.S4Amount   = G("S4AMT")  : br.S4CondId   = G("S4CONDID")
                br.S5Cond   = G("S5COND")  : br.S5Desc   = G("S5DESC")  : br.S5Type   = G("S5TYPE")  : br.S5Amount   = G("S5AMT")  : br.S5CondId   = G("S5CONDID")
                br.S6Cond   = G("S6COND")  : br.S6Desc   = G("S6DESC")  : br.S6Type   = G("S6TYPE")  : br.S6Amount   = G("S6AMT")  : br.S6CondId   = G("S6CONDID")
                br.S7Cond   = G("S7COND")  : br.S7Desc   = G("S7DESC")  : br.S7Type   = G("S7TYPE")  : br.S7Amount   = G("S7AMT")  : br.S7CondId   = G("S7CONDID")
                br.S8Cond   = G("S8COND")  : br.S8Desc   = G("S8DESC")  : br.S8Type   = G("S8TYPE")  : br.S8Amount   = G("S8AMT")  : br.S8CondId   = G("S8CONDID")
                br.S9Cond   = G("S9COND")  : br.S9Desc   = G("S9DESC")  : br.S9Type   = G("S9TYPE")  : br.S9Amount   = G("S9AMT")  : br.S9CondId   = G("S9CONDID")
                br.S10Cond  = G("S10COND") : br.S10Desc  = G("S10DESC") : br.S10Type  = G("S10TYPE") : br.S10Amount  = G("S10AMT") : br.S10CondId  = G("S10CONDID")
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
