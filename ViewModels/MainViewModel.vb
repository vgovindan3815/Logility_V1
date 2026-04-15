Option Strict On
Option Explicit On

Imports System.Windows
Imports FXF3A_Tool.Core
Imports Microsoft.Win32

Namespace ViewModels

    ''' <summary>
    ''' Shell ViewModel. Owns:
    '''   - Current content view (swaps when nav item clicked)
    '''   - Connection bar state (bound to connection bar in MainWindow)
    '''   - Session status badge text and colour
    ''' </summary>
    Public Class MainViewModel
        Inherits BaseViewModel

        ' ── Dependencies ─────────────────────────────────────────────
        Private ReadOnly _session As SessionManager

        ' ── Child ViewModels (created once, reused) ──────────────────
        Private _LoginVM  As LoginViewModel
        Private _FXF3A_VM As FXF3A_ViewModel
        Private _FXF3B_VM As FXF3B_ViewModel
        Private _FXF3C_VM As FXF3C_ViewModel
        Private _FXF3D_VM As FXF3D_ViewModel
        Private _FXF3E_VM As FXF3E_ViewModel
        Private _FXF3F_VM As FXF3F_ViewModel
        Private _FXF3G_VM As FXF3G_ViewModel
        Private _FXF3J_VM As FXF3J_ViewModel
        Private _FXF3K_VM As FXF3K_ViewModel
        Private _FXF3M_VM As FXF3M_ViewModel
        Private _FXF3N_VM As FXF3N_ViewModel
        Private _FXF4M_VM As FXF4M_ViewModel

        Public ReadOnly Property LoginVM    As LoginViewModel
            Get
                Return _LoginVM
            End Get
        End Property
        Public ReadOnly Property FXF3A_VM   As FXF3A_ViewModel
            Get
                Return _FXF3A_VM
            End Get
        End Property
        Public ReadOnly Property FXF3B_VM   As FXF3B_ViewModel
            Get
                Return _FXF3B_VM
            End Get
        End Property
        Public ReadOnly Property FXF3C_VM   As FXF3C_ViewModel
            Get
                Return _FXF3C_VM
            End Get
        End Property
        Public ReadOnly Property FXF3D_VM   As FXF3D_ViewModel
            Get
                Return _FXF3D_VM
            End Get
        End Property
        Public ReadOnly Property FXF3E_VM   As FXF3E_ViewModel
            Get
                Return _FXF3E_VM
            End Get
        End Property
        Public ReadOnly Property FXF3F_VM   As FXF3F_ViewModel
            Get
                Return _FXF3F_VM
            End Get
        End Property
        Public ReadOnly Property FXF3G_VM   As FXF3G_ViewModel
            Get
                Return _FXF3G_VM
            End Get
        End Property
        Public ReadOnly Property FXF3J_VM   As FXF3J_ViewModel
            Get
                Return _FXF3J_VM
            End Get
        End Property
        Public ReadOnly Property FXF3K_VM   As FXF3K_ViewModel
            Get
                Return _FXF3K_VM
            End Get
        End Property
        Public ReadOnly Property FXF3M_VM   As FXF3M_ViewModel
            Get
                Return _FXF3M_VM
            End Get
        End Property
        Public ReadOnly Property FXF3N_VM   As FXF3N_ViewModel
            Get
                Return _FXF3N_VM
            End Get
        End Property
        Public ReadOnly Property FXF4M_VM   As FXF4M_ViewModel
            Get
                Return _FXF4M_VM
            End Get
        End Property

        ' ── Constructor ──────────────────────────────────────────────
        Public Sub New()
            _session = SessionManager.Instance

            ' Instantiate all child ViewModels
            _LoginVM  = New LoginViewModel(_session)
            _FXF3A_VM = New FXF3A_ViewModel(_session)
            _FXF3B_VM = New FXF3B_ViewModel(_session)
            _FXF3C_VM = New FXF3C_ViewModel(_session)
            _FXF3D_VM = New FXF3D_ViewModel(_session)
            _FXF3E_VM = New FXF3E_ViewModel(_session)
            _FXF3F_VM = New FXF3F_ViewModel(_session)
            _FXF3G_VM = New FXF3G_ViewModel(_session)
            _FXF3J_VM = New FXF3J_ViewModel(_session)
            _FXF3K_VM = New FXF3K_ViewModel(_session)
            _FXF3M_VM = New FXF3M_ViewModel(_session)
            _FXF3N_VM = New FXF3N_ViewModel(_session)
            _FXF4M_VM = New FXF4M_ViewModel(_session)

            ' Start on the Login/Welcome view
            _currentView = _LoginVM

            ' Subscribe to session events
            AddHandler _session.ConnectionChanged, AddressOf OnConnectionChanged
            AddHandler _session.StatusChanged,     AddressOf OnStatusChanged

            ' Excel load command
            _loadAllFromExcelCommand = New RelayCommand(AddressOf ExecuteLoadAllFromExcel)

            ' Navigation commands
            _navLoginCommand  = New RelayCommand(Sub() NavigateTo(_LoginVM))
            _navFXF3ACommand  = New RelayCommand(Sub() NavigateTo(_FXF3A_VM))
            _navFXF3BCommand  = New RelayCommand(Sub() NavigateTo(_FXF3B_VM))
            _navFXF3CCommand  = New RelayCommand(Sub() NavigateTo(_FXF3C_VM))
            _navFXF3DCommand  = New RelayCommand(Sub() NavigateTo(_FXF3D_VM))
            _navFXF3ECommand  = New RelayCommand(Sub() NavigateTo(_FXF3E_VM))
            _navFXF3FCommand  = New RelayCommand(Sub() NavigateTo(_FXF3F_VM))
            _navFXF3GCommand  = New RelayCommand(Sub() NavigateTo(_FXF3G_VM))
            _navFXF3JCommand  = New RelayCommand(Sub() NavigateTo(_FXF3J_VM))
            _navFXF3KCommand  = New RelayCommand(Sub() NavigateTo(_FXF3K_VM))
            _navFXF3MCommand  = New RelayCommand(Sub() NavigateTo(_FXF3M_VM))
            _navFXF3NCommand  = New RelayCommand(Sub() NavigateTo(_FXF3N_VM))
            _navFXF4MCommand  = New RelayCommand(Sub() NavigateTo(_FXF4M_VM))
        End Sub

        ' ── Current view ─────────────────────────────────────────────
        Private _currentView As BaseViewModel
        Public Property CurrentView As BaseViewModel
            Get
                Return _currentView
            End Get
            Set(v As BaseViewModel)
                SetField(_currentView, v)
                NotifyPropertyChanged("IsLoginActive")
                NotifyPropertyChanged("IsFXF3AActive")
                NotifyPropertyChanged("IsFXF3BActive")
                NotifyPropertyChanged("IsFXF3CActive")
                NotifyPropertyChanged("IsFXF3DActive")
                NotifyPropertyChanged("IsFXF3EActive")
                NotifyPropertyChanged("IsFXF3FActive")
                NotifyPropertyChanged("IsFXF3GActive")
                NotifyPropertyChanged("IsFXF3JActive")
                NotifyPropertyChanged("IsFXF3KActive")
                NotifyPropertyChanged("IsFXF3MActive")
                NotifyPropertyChanged("IsFXF3NActive")
                NotifyPropertyChanged("IsFXF4MActive")
            End Set
        End Property

        Private Sub NavigateTo(vm As BaseViewModel)
            CurrentView = vm
        End Sub

        ' ── Nav active flags (for nav rail highlight binding) ─────────
        Public ReadOnly Property IsLoginActive  As Boolean
            Get
                Return TypeOf _currentView Is LoginViewModel 
            End Get
        End Property
        Public ReadOnly Property IsFXF3AActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3A_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3BActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3B_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3CActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3C_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3DActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3D_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3EActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3E_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3FActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3F_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3GActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3G_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3JActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3J_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3KActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3K_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3MActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3M_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF3NActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF3N_ViewModel
            End Get
        End Property
        Public ReadOnly Property IsFXF4MActive  As Boolean
            Get
                Return TypeOf _currentView Is FXF4M_ViewModel
            End Get
        End Property

        ' ── Excel load command ───────────────────────────────────────
        Private ReadOnly _loadAllFromExcelCommand As RelayCommand

        Public ReadOnly Property LoadAllFromExcelCommand As RelayCommand
            Get
                Return _loadAllFromExcelCommand
            End Get
        End Property

        Private _loadStatus As String = ""
        Public Property LoadStatus As String
            Get
                Return _loadStatus
            End Get
            Set(v As String)
                SetField(_loadStatus, v)
            End Set
        End Property

        Private Sub ExecuteLoadAllFromExcel()
            Dim dlg As New OpenFileDialog()
            dlg.Title  = "Select data workbook"
            dlg.Filter = "Excel files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All files (*.*)|*.*"
            If dlg.ShowDialog() <> True Then Return

            Dim path As String = dlg.FileName
            Dim counts As New System.Text.StringBuilder()
            Dim total As Integer = 0

            Dim screens() As String = { _
                "FXF3A_Batch", "FXF3B_Batch", "FXF3C_Batch", "FXF3D_Batch", _
                "FXF3E_Batch", "FXF3F_Batch", "FXF3G_Batch", "FXF3J_Batch", _
                "FXF3K_Batch", "FXF3M_Batch", "FXF3N_Batch", "FXF4M_Batch" _
            }

            Try
                Dim rows3A  = ExcelLoader.LoadSheet(path, "FXF3A_Batch",  1)
                Dim rows3B  = ExcelLoader.LoadSheet(path, "FXF3B_Batch",  1)
                Dim rows3C  = ExcelLoader.LoadSheet(path, "FXF3C_Batch",  1)
                Dim rows3D  = ExcelLoader.LoadSheet(path, "FXF3D_Batch",  1)
                Dim rows3E  = ExcelLoader.LoadSheet(path, "FXF3E_Batch",  1)
                Dim rows3F  = ExcelLoader.LoadSheet(path, "FXF3F_Batch",  1)
                Dim rows3G  = ExcelLoader.LoadSheet(path, "FXF3G_Batch",  1)
                Dim rows3J  = ExcelLoader.LoadSheet(path, "FXF3J_Batch",  1)
                Dim rows3K  = ExcelLoader.LoadSheet(path, "FXF3K_Batch",  1)
                Dim rows3M  = ExcelLoader.LoadSheet(path, "FXF3M_Batch",  1)
                Dim rows3N  = ExcelLoader.LoadSheet(path, "FXF3N_Batch",  1)
                Dim rows4M  = ExcelLoader.LoadSheet(path, "FXF4M_Batch",  1)

                _FXF3A_VM.LoadRows(rows3A)
                _FXF3B_VM.LoadRows(rows3B)
                _FXF3C_VM.LoadRows(rows3C)
                _FXF3D_VM.LoadRows(rows3D)
                _FXF3E_VM.LoadRows(rows3E)
                _FXF3F_VM.LoadRows(rows3F)
                _FXF3G_VM.LoadRows(rows3G)
                _FXF3J_VM.LoadRows(rows3J)
                _FXF3K_VM.LoadRows(rows3K)
                _FXF3M_VM.LoadRows(rows3M)
                _FXF3N_VM.LoadRows(rows3N)
                _FXF4M_VM.LoadRows(rows4M)

                total = rows3A.Count + rows3B.Count + rows3C.Count + rows3D.Count + _
                        rows3E.Count + rows3F.Count + rows3G.Count + rows3J.Count + _
                        rows3K.Count + rows3M.Count + rows3N.Count + rows4M.Count

                LoadStatus = String.Format( _
                    "Loaded {0} rows  (3A={1} 3B={2} 3C={3} 3D={4} 3E={5} 3F={6} 3G={7} 3J={8} 3K={9} 3M={10} 3N={11} 4M={12})", _
                    total, rows3A.Count, rows3B.Count, rows3C.Count, rows3D.Count, _
                    rows3E.Count, rows3F.Count, rows3G.Count, rows3J.Count, _
                    rows3K.Count, rows3M.Count, rows3N.Count, rows4M.Count)

            Catch ex As Exception
                LoadStatus = "Load failed: " & ex.Message
                Try
                    Dim logPath As String = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(
                            System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "load_error.txt")
                    System.IO.File.WriteAllText(logPath,
                        "Message: " & ex.Message & System.Environment.NewLine &
                        "Type: " & ex.GetType().FullName & System.Environment.NewLine &
                        "Stack: " & ex.StackTrace & System.Environment.NewLine &
                        If(ex.InnerException IsNot Nothing,
                           "Inner: " & ex.InnerException.Message & System.Environment.NewLine &
                           "InnerStack: " & ex.InnerException.StackTrace, ""))
                Catch
                End Try
            End Try
        End Sub

        ' ── Navigation commands ───────────────────────────────────────
        Private ReadOnly _navLoginCommand  As RelayCommand
        Private ReadOnly _navFXF3ACommand  As RelayCommand
        Private ReadOnly _navFXF3BCommand  As RelayCommand
        Private ReadOnly _navFXF3CCommand  As RelayCommand
        Private ReadOnly _navFXF3DCommand  As RelayCommand
        Private ReadOnly _navFXF3ECommand  As RelayCommand
        Private ReadOnly _navFXF3FCommand  As RelayCommand
        Private ReadOnly _navFXF3GCommand  As RelayCommand
        Private ReadOnly _navFXF3JCommand  As RelayCommand
        Private ReadOnly _navFXF3KCommand  As RelayCommand
        Private ReadOnly _navFXF3MCommand  As RelayCommand
        Private ReadOnly _navFXF3NCommand  As RelayCommand
        Private ReadOnly _navFXF4MCommand  As RelayCommand

        Public ReadOnly Property NavLoginCommand  As RelayCommand
            Get
                Return _navLoginCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3ACommand  As RelayCommand
            Get
                Return _navFXF3ACommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3BCommand  As RelayCommand
            Get
                Return _navFXF3BCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3CCommand  As RelayCommand
            Get
                Return _navFXF3CCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3DCommand  As RelayCommand
            Get
                Return _navFXF3DCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3ECommand  As RelayCommand
            Get
                Return _navFXF3ECommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3FCommand  As RelayCommand
            Get
                Return _navFXF3FCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3GCommand  As RelayCommand
            Get
                Return _navFXF3GCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3JCommand  As RelayCommand
            Get
                Return _navFXF3JCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3KCommand  As RelayCommand
            Get
                Return _navFXF3KCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3MCommand  As RelayCommand
            Get
                Return _navFXF3MCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF3NCommand  As RelayCommand
            Get
                Return _navFXF3NCommand 
            End Get
        End Property
        Public ReadOnly Property NavFXF4MCommand  As RelayCommand
            Get
                Return _navFXF4MCommand 
            End Get
        End Property

        ' ── Session status (bound to connection bar badge) ────────────
        Private _sessionStatus As String = "Disconnected"
        Public Property SessionStatus As String
            Get
                Return _sessionStatus
            End Get
            Set(v As String)
                SetField(_sessionStatus, v)
            End Set
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _session.IsConnected
            End Get
        End Property

        ' ── Session event handlers ────────────────────────────────────
        Private Sub OnConnectionChanged(sender As Object, e As EventArgs)
            Application.Current.Dispatcher.InvokeAsync(Sub()
                NotifyPropertyChanged("IsConnected")
                ' Auto-navigate to FXF3A after connect; stay on current screen after disconnect
                If _session.IsConnected Then
                    NavigateTo(_FXF3A_VM)
                End If
            End Sub)
        End Sub

        Private Sub OnStatusChanged(sender As Object, message As String)
            Application.Current.Dispatcher.InvokeAsync(Sub()
                SessionStatus = message
            End Sub)
        End Sub

    End Class

End Namespace
