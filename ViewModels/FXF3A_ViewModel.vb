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

Namespace ViewModels

    Public Class FXF3A_ViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session

            _runBatchCommand   = New RelayCommand(AddressOf ExecuteRunBatch,
                                                  Function() BatchRows.Count > 0 AndAlso Not _isBusy)
            AddHandler BatchRows.CollectionChanged, Sub(s, e) _runBatchCommand.RaiseCanExecuteChanged()
            _addRowCommand     = New RelayCommand(Sub() BatchRows.Add(New FXF3A_BatchRow()))
            _clearBatchCommand = New RelayCommand(Sub() BatchRows.Clear(),
                                                  Function() Not _isBusy)
            _clearResultsCommand = New RelayCommand(Sub() Results.Clear())
            _exportResultsCommand = New RelayCommand(AddressOf ExecuteExportResults,
                                                     Function() Results.Count > 0)
            _loadAccountCommand = New RelayCommand(AddressOf ExecuteLoadAccount,
                                                   Function() _session.IsConnected AndAlso
                                                              Not String.IsNullOrWhiteSpace(_quickAccount) AndAlso
                                                              Not _isBusy)
            _loadCsvCommand     = New RelayCommand(AddressOf ExecuteLoadCsv,
                                                   Function() Not _isBusy)
            _selectAllCommand = New RelayCommand(Sub()
                For Each r As FXF3A_BatchRow In BatchRows
                    r.IsSelected = True
                Next
            End Sub)
        End Sub

        ' ── Quick-load bar ───────────────────────────────────────────
        Private _quickCarrier  As String = "FXFM"
        Private _quickCustType As String = "CC"
        Private _quickAccount  As String = ""

        Public Property QuickCarrier  As String
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
        Public Property QuickAccount  As String
            Get
                Return _quickAccount 
            End Get
            Set(v As String)
                SetField(_quickAccount, v)
                _loadAccountCommand.RaiseCanExecuteChanged()
            End Set
        End Property

        ' ── Batch rows ───────────────────────────────────────────────
        Public Property BatchRows As New ObservableCollection(Of FXF3A_BatchRow)

        ' ── Results ──────────────────────────────────────────────────
        Public Property Results As New ObservableCollection(Of FXF3A_BatchRow)

        ' ── Progress ─────────────────────────────────────────────────
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

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _runBatchCommand      As RelayCommand
        Private ReadOnly _addRowCommand        As RelayCommand
        Private ReadOnly _clearBatchCommand    As RelayCommand
        Private ReadOnly _clearResultsCommand  As RelayCommand
        Private ReadOnly _exportResultsCommand As RelayCommand
        Private ReadOnly _loadAccountCommand   As RelayCommand
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
        Public ReadOnly Property LoadAccountCommand   As RelayCommand
            Get
                Return _loadAccountCommand
            End Get
        End Property
        Public ReadOnly Property LoadCsvCommand       As RelayCommand
            Get
                Return _loadCsvCommand
            End Get
        End Property
        Public ReadOnly Property SelectAllCommand     As RelayCommand
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
                    Return _session.FXF3A.getItems(
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
                        Dim row As New FXF3A_BatchRow
                        row.Action    = "GET"
                        row.Carrier   = carrier
                        row.CustType  = custType
                        row.Account   = acct
                        row.Authority = hdr.auhority
                        row.Number    = hdr.number
                        row.Item      = hdr.item
                        If hdr.discTable IsNot Nothing AndAlso hdr.discTable.Count > 0 Then
                            row.Disc1    = hdr.discTable(0).disc.ToString()
                            row.EffDate1 = If(hdr.discTable(0).effectiveDate =
                                              FedEx.PABST.SS.SSLib.ScreenScraping.NULL_DATE,
                                              "",
                                              hdr.discTable(0).effectiveDate.ToString("MM/dd/yy"))
                        End If
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

            Dim selectedRows As New List(Of FXF3A_BatchRow)
            For Each r As FXF3A_BatchRow In BatchRows
                If r.IsSelected Then selectedRows.Add(r)
            Next
            If selectedRows.Count = 0 Then
                BannerMessage = "No rows selected. Use the checkboxes to select rows to process."
                IsBusy = False
                Return
            End If
            Dim total = selectedRows.Count
            Dim ok = 0, err = 0, skipped = 0
            ProgressTotal   = total
            ProgressCurrent = 0

            For i As Integer = 0 To selectedRows.Count - 1
                Dim row = selectedRows(i)
                ProgressCurrent = i + 1
                ProgressText    = String.Format("Row {0}/{1}  — {2} {3}", i + 1, total, row.Action, row.Account)

                If String.IsNullOrWhiteSpace(row.Action) Then
                    row.Status = OperationStatus.Skipped
                    skipped += 1
                    Continue For
                End If

                row.Status = OperationStatus.Running
                row.StatusMessage = ""

                Try
                    Await Task.Run(Sub() ProcessRow(row))
                    ok += 1
                Catch ex As Exception
                    ' ProcessRow sets row.Status/StatusMessage internally
                    err += 1
                End Try

                ' Yield to UI
                Await Task.Delay(10)
            Next

            ProgressText  = String.Format("Complete — {0} OK, {1} errors, {2} skipped", ok, err, skipped)
            BannerMessage = ProgressText
            IsBusy = False
        End Sub

        ' ── Process single row (runs on background thread) ────────────
        Private Sub ProcessRow(row As FXF3A_BatchRow)
            Try
                Dim carrier  = ParseCarrier(row.Carrier)
                Dim custType = ParseCustType(row.CustType)

                Select Case row.Action.ToUpper()

                    Case "GET"
                        If row.HasItemKey Then
                            ' Single item GET
                            Dim it = _session.FXF3A.getItem(
                                carrier, custType, row.Account,
                                row.Authority, row.Number, row.Item)
                            Dim resultRow As New FXF3A_BatchRow
                            resultRow.Carrier  = row.Carrier
                            resultRow.CustType = row.CustType
                            resultRow.FromItemClass(it)
                            Application.Current.Dispatcher.InvokeAsync(Sub()
                                Results.Add(resultRow)
                            End Sub)
                        Else
                            ' All items for account
                            Dim items = _session.FXF3A.getItems(carrier, custType, row.Account, True)
                            If items IsNot Nothing Then
                                For j As Integer = 0 To items.Count - 1
                                    ' For list results, do getItem on each to get full detail
                                    Dim hdr = items(j)
                                    Dim fullItem = _session.FXF3A.getItem(
                                        carrier, custType, row.Account,
                                        hdr.auhority, hdr.number, hdr.item)
                                    Dim resultRow As New FXF3A_BatchRow
                                    resultRow.Carrier  = row.Carrier
                                    resultRow.CustType = row.CustType
                                    resultRow.FromItemClass(fullItem)
                                    Application.Current.Dispatcher.InvokeAsync(Sub()
                                        Results.Add(resultRow)
                                    End Sub)
                                Next
                            End If
                        End If
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "ADD"
                        _session.FXF3A.addItem(
                            carrier, custType, row.Account,
                            row.ToItemClass(), (row.Release = "Y"))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CHANGE"
                        _session.FXF3A.changeItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item,
                            row.ToItemClass(), (row.Release = "Y"))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "CANCEL"
                        _session.FXF3A.cancelItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item,
                            row.GetCancelDate(), (row.Release = "Y"))
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "DELETE"
                        _session.FXF3A.deleteItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case "RELEASE"
                        _session.FXF3A.releaseItem(
                            carrier, custType, row.Account,
                            row.Authority, row.Number, row.Item)
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Success
                        End Sub)

                    Case Else
                        Application.Current.Dispatcher.InvokeAsync(Sub()
                            row.Status = OperationStatus.Skipped
                            row.StatusMessage = "Unknown action: " & row.Action
                        End Sub)
                End Select

            Catch ex As AccountNotFoundException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status = OperationStatus.Error
                    row.StatusMessage = "Account not found: " & ex.Message
                End Sub)
                Throw
            Catch ex As NoDiscountRecordsException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status = OperationStatus.Warning
                    row.StatusMessage = "No discount records"
                End Sub)
                ' Warning — not rethrown, doesn't count as error
            Catch ex As NumericValueException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status = OperationStatus.Error
                    row.StatusMessage = "Invalid cancel date: " & ex.Message
                End Sub)
                Throw
            Catch ex As GenericScreenScraperException
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status = OperationStatus.Error
                    row.StatusMessage = ex.Message
                    ' ScreenDump stored for tooltip — truncate for display
                    If Not String.IsNullOrWhiteSpace(ex.ScreenDump) Then
                        row.StatusMessage &= " [screen dump available]"
                    End If
                End Sub)
                Throw
            Catch ex As Exception
                Application.Current.Dispatcher.InvokeAsync(Sub()
                    row.Status = OperationStatus.Error
                    row.StatusMessage = ex.Message
                End Sub)
                Throw
            End Try
        End Sub

        ' ── Export Results to CSV ─────────────────────────────────────
        Private Sub ExecuteExportResults()
            Dim dlg As New Microsoft.Win32.SaveFileDialog()
            dlg.Filter   = "CSV files (*.csv)|*.csv"
            dlg.FileName = "FXF3A_Results_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
            If dlg.ShowDialog() <> True Then Return

            Using sw As New StreamWriter(dlg.FileName, False, Encoding.UTF8)
                ' Header
                sw.WriteLine("Timestamp,Carrier,CustType,Account,Authority,Number,Item," &
                             "Released,Disc1,EffDate1,CancelDate1,Currency,Inter,TypeHaul," &
                             "GeoDir1,GeoType1,GeoName1,LastMaintDate,OperatorId,Status")
                ' Rows
                For Each r In Results
                    sw.WriteLine(String.Join(",", {
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Carrier, r.CustType, Q(r.Account),
                        Q(r.Authority), Q(r.Number), Q(r.Item),
                        r.StatusIcon,
                        r.Disc1, r.EffDate1, r.CanDate1, r.Currency,
                        r.Inter, r.TypeHaul,
                        r.GeoDir1, r.GeoType1, Q(r.GeoName1),
                        r.LastMaintDate, r.OperatorId, r.Status.ToString()
                    }))
                Next
            End Using

            BannerMessage = "Exported " & Results.Count & " rows to " & dlg.FileName
        End Sub

        ' ── Import from CSV ──────────────────────────────────────────
        ''' <summary>
        ''' Load batch rows from a CSV file. Does NOT require a mainframe connection.
        ''' Expected columns (header row, case-insensitive):
        '''   Action, Carrier, CustType, Account, Authority, Number, Item,
        '''   Release, Disc1, EffDate1, CanDate1, Disc2, EffDate2, CanDate2,
        '''   Currency, Inter, TypeHaul, Matrix, GeoDir1, GeoType1, GeoName1
        ''' Any missing columns are filled with defaults.
        ''' </summary>
        Private Sub ExecuteLoadCsv()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Import FXF3A Batch from CSV"
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim lines As String()
            Try
                lines = File.ReadAllLines(dlg.FileName)
            Catch ex As Exception
                BannerMessage = "Cannot read file: " & ex.Message
                Return
            End Try

            If lines.Length < 2 Then
                BannerMessage = "CSV has no data rows (needs a header row + at least one data row)."
                Return
            End If

            Dim hdr = Core.CsvHelper.BuildHeaderMap(lines(0))
            Dim count = 0, errors = 0

            For i As Integer = 1 To lines.Length - 1
                Dim line = lines(i).Trim()
                If String.IsNullOrEmpty(line) Then Continue For
                Try
                    Dim f = Core.CsvHelper.SplitLine(line)
                    Dim G = Function(col As String) Core.CsvHelper.GetField(f, hdr, col)
                    Dim GD = Function(col As String, def As String) Core.CsvHelper.GetFieldOrDefault(f, hdr, col, def)

                    Dim row As New FXF3A_BatchRow()
                    row.Action    = G("Action")
                    row.Carrier   = GD("Carrier",  "FXFM")
                    row.CustType  = GD("CustType", "CC")
                    row.Account   = G("Account")
                    row.Authority = G("Authority")
                    row.Number    = G("Number")
                    row.Item      = G("Item")
                    row.Release   = If(Core.CsvHelper.GetBool(f, hdr, "Release"), "Y", "N")
                    row.Disc1     = G("Disc1")
                    row.EffDate1  = G("EffDate1")
                    row.CanDate1  = G("CanDate1")
                    row.Disc2     = G("Disc2")
                    row.EffDate2  = G("EffDate2")
                    row.CanDate2  = G("CanDate2")
                    row.Disc3     = G("Disc3")
                    row.EffDate3  = G("EffDate3")
                    row.CanDate3  = G("CanDate3")
                    row.Currency  = G("Currency")
                    row.Inter     = GD("Inter",    "NA")
                    row.TypeHaul  = GD("TypeHaul", "NA")
                    row.Matrix    = G("Matrix")
                    row.GeoDir1   = GD("GeoDir1",  "NA")
                    row.GeoType1  = GD("GeoType1", "NA")
                    row.GeoName1  = G("GeoName1")
                    BatchRows.Add(row)
                    count += 1
                Catch
                    errors += 1
                End Try
            Next

            BannerMessage = If(errors > 0,
                String.Format("{0} rows imported, {1} skipped (parse error) from {2}.", count, errors, IO.Path.GetFileName(dlg.FileName)),
                String.Format("{0} rows imported from {1}.", count, IO.Path.GetFileName(dlg.FileName)))
        End Sub

        ' ── Load rows from Excel import ──────────────────────────────
        ''' <summary>
        ''' Populate BatchRows from a pre-parsed list of dictionaries produced by
        ''' ExcelLoader.LoadSheet. Keys are normalised (upper-case, no underscores/?)
        ''' so CUST_TYPE -> CUSTTYPE, RELEASE? -> RELEASE, etc.
        ''' </summary>
        Public Sub LoadRows(rows As List(Of Dictionary(Of String, String)))
            BatchRows.Clear()
            For Each rowDict As Dictionary(Of String, String) In rows
                Dim G As Func(Of String, String) = _
                    Function(col As String) If(rowDict.ContainsKey(col), rowDict(col), "")
                Dim GD As Func(Of String, String, String) = _
                    Function(col As String, def As String) _
                        If(rowDict.ContainsKey(col) AndAlso Not String.IsNullOrEmpty(rowDict(col)), rowDict(col), def)
                Dim br As New FXF3A_BatchRow()
                br.Action    = G("ACTION")
                br.Carrier   = GD("CARRIER",  "FXFM")
                br.CustType  = GD("CUSTTYPE", "CC")
                br.Account   = G("ACCOUNT")
                br.Authority = G("AUTHORITY")
                br.Number    = G("NUMBER")
                br.Item      = G("ITEM")
                br.Release   = If(G("RELEASE").ToUpper() = "TRUE" OrElse G("RELEASE").ToUpper() = "Y", "Y", "N")
                br.Disc1     = G("DISC1")
                br.EffDate1  = G("EFFDATE1")
                br.CanDate1  = G("CANDATE1")
                br.Disc2     = G("DISC2")
                br.EffDate2  = G("EFFDATE2")
                br.CanDate2  = G("CANDATE2")
                br.Disc3     = G("DISC3")
                br.EffDate3  = G("EFFDATE3")
                br.CanDate3  = G("CANDATE3")
                br.Currency  = G("CURRENCY")
                br.Inter     = GD("INTER",    "NA")
                br.TypeHaul  = GD("TYPEHAUL", "NA")
                br.Matrix    = G("MATRIX")
                br.GeoDir1   = GD("GEODIR1",  "NA")
                br.GeoType1  = GD("GEOTYPE1", "NA")
                br.GeoName1  = G("GEONAME1")
                br.PrepdIn    = If(G("PREPDIN").ToUpper()    = "Y", "Y", "N")
                br.PrepdOut   = If(G("PREPDOUT").ToUpper()   = "Y", "Y", "N")
                br.CollIn     = If(G("COLLIN").ToUpper()     = "Y", "Y", "N")
                br.CollOut    = If(G("COLLOUT").ToUpper()    = "Y", "Y", "N")
                br.ThirdParty = If(G("3RDPTY").ToUpper()     = "Y", "Y", "N")
                br.ApplyArbs  = If(G("APPLYARB").ToUpper()   = "Y", "Y", "N")
                br.IncExempt  = If(G("INCEXM").ToUpper()     = "Y", "Y", "N")
                br.Fak        = If(G("FAK").ToUpper()        = "Y", "Y", "N")
                br.N50   = If(G("N50").ToUpper()   = "Y", "Y", "N")
                br.N55   = If(G("N55").ToUpper()   = "Y", "Y", "N")
                br.N60   = If(G("N60").ToUpper()   = "Y", "Y", "N")
                br.N65   = If(G("N65").ToUpper()   = "Y", "Y", "N")
                br.N70   = If(G("N70").ToUpper()   = "Y", "Y", "N")
                br.N77_5 = If(G("N77").ToUpper()   = "Y", "Y", "N")
                br.N85   = If(G("N85").ToUpper()   = "Y", "Y", "N")
                br.N92_5 = If(G("N92").ToUpper()   = "Y", "Y", "N")
                br.N100  = If(G("N100").ToUpper()  = "Y", "Y", "N")
                br.N110  = If(G("N110").ToUpper()  = "Y", "Y", "N")
                br.N125  = If(G("N125").ToUpper()  = "Y", "Y", "N")
                br.N150  = If(G("N150").ToUpper()  = "Y", "Y", "N")
                br.N175  = If(G("N175").ToUpper()  = "Y", "Y", "N")
                br.N200  = If(G("N200").ToUpper()  = "Y", "Y", "N")
                br.N250  = If(G("N250").ToUpper()  = "Y", "Y", "N")
                br.N300  = If(G("N300").ToUpper()  = "Y", "Y", "N")
                br.N400  = If(G("N400").ToUpper()  = "Y", "Y", "N")
                br.N500  = If(G("N500").ToUpper()  = "Y", "Y", "N")
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

        ''' <summary>Quote a CSV field if it contains a comma.</summary>
        Private Shared Function Q(s As String) As String
            If s Is Nothing Then Return ""
            If s.Contains(",") Then Return """" & s.Replace("""", """""") & """"
            Return s
        End Function

    End Class

End Namespace
