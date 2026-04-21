Option Strict On
Option Explicit On

Imports System.Threading.Tasks
Imports System.Windows
Imports FXF3A_Tool.Core

Namespace ViewModels

    ''' <summary>
    ''' ViewModel for the login page and connection bar.
    ''' Persists non-sensitive settings. Passwords are NEVER stored.
    ''' </summary>
    Public Class LoginViewModel
        Inherits BaseViewModel

        Private ReadOnly _session As SessionManager

        Public Sub New(session As SessionManager)
            _session = session
            LoadSettings()

            _connectCommand    = New RelayCommand(AddressOf ExecuteConnect,
                                                  Function() Not _session.IsConnected AndAlso Not _isBusy)
            _disconnectCommand = New RelayCommand(AddressOf ExecuteDisconnect,
                                                  Function() _session.IsConnected AndAlso Not _isBusy)
            _browseRsfCommand  = New RelayCommand(AddressOf ExecuteBrowseRsf)

            AddHandler _session.ConnectionChanged, AddressOf OnConnectionChanged
        End Sub

        ' ── Connection fields (persisted — no passwords) ─────────────
        Private Const DefaultHost As String = "c0040811.test.cloud.fedex.com:9000"
        Private Const DefaultSystemCode As String = "FDXF"
        Private Const DefaultUid As String = "1647111"
        Private Shared ReadOnly DefaultRsfPath As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScreenLayouts.xml")
        Private Const DefaultTimeout As Integer = 30000

        Private _host As String = DefaultHost
        Public Property Host As String
            Get
                Return _host
            End Get
            Set(v As String)
                SetField(_host, v)
                _connectCommand.RaiseCanExecuteChanged()
                NotifyPropertyChanged("CanConnect")
            End Set
        End Property

        Private _systemCode As String = DefaultSystemCode
        Public Property SystemCode As String
            Get
                Return _systemCode
            End Get
            Set(v As String)
                SetField(_systemCode, v)
            End Set
        End Property

        Private _uidT As String = DefaultUid
        Public Property UidT As String
            Get
                Return _uidT
            End Get
            Set(v As String)
                SetField(_uidT, v)
            End Set
        End Property

        Private _uidL As String = DefaultUid
        Public Property UidL As String
            Get
                Return _uidL
            End Get
            Set(v As String)
                SetField(_uidL, v)
            End Set
        End Property

        Private _rsfPath As String = DefaultRsfPath
        Public Property RsfPath As String
            Get
                Return _rsfPath
            End Get
            Set(v As String)
                SetField(_rsfPath, v)
                NotifyPropertyChanged("CanConnect")
            End Set
        End Property

        Private _timeout As Integer = DefaultTimeout
        Public Property Timeout As Integer
            Get
                Return _timeout
            End Get
            Set(v As Integer)
                SetField(_timeout, v)
            End Set
        End Property

        ' ── UI state ─────────────────────────────────────────────────
        Private _isBusy As Boolean = False
        Public Property IsBusy As Boolean
            Get
                Return _isBusy
            End Get
            Set(v As Boolean)
                SetField(_isBusy, v)
                _connectCommand.RaiseCanExecuteChanged()
                _disconnectCommand.RaiseCanExecuteChanged()
                NotifyPropertyChanged("CanConnect")
            End Set
        End Property

        Private _errorBanner As String = ""
        Public Property ErrorBanner As String
            Get
                Return _errorBanner
            End Get
            Set(v As String)
                SetField(_errorBanner, v)
            End Set
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _session.IsConnected
            End Get
        End Property

        ''' <summary>
        ''' True when the form Connect button should be enabled.
        ''' </summary>
        Public ReadOnly Property CanConnect As Boolean
            Get
                Return Not _session.IsConnected AndAlso Not _isBusy AndAlso
                       Not String.IsNullOrWhiteSpace(_host) AndAlso
                       Not String.IsNullOrWhiteSpace(_rsfPath)
            End Get
        End Property

        ' ── Commands ─────────────────────────────────────────────────
        Private ReadOnly _connectCommand    As RelayCommand
        Private ReadOnly _disconnectCommand As RelayCommand
        Private ReadOnly _browseRsfCommand  As RelayCommand

        Public ReadOnly Property ConnectCommand As RelayCommand
            Get
                Return _connectCommand
            End Get
        End Property
        Public ReadOnly Property DisconnectCommand As RelayCommand
            Get
                Return _disconnectCommand
            End Get
        End Property
        Public ReadOnly Property BrowseRsfCommand As RelayCommand
            Get
                Return _browseRsfCommand
            End Get
        End Property

        ' ── Browse RSF path ──────────────────────────────────────────
        Private Sub ExecuteBrowseRsf()
            Dim dlg As New Microsoft.Win32.OpenFileDialog()
            dlg.Title  = "Select Screen Layout XML File"
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
            If Not String.IsNullOrWhiteSpace(_rsfPath) Then
                Try
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_rsfPath)
                Catch
                End Try
            End If
            If dlg.ShowDialog() = True Then
                RsfPath = dlg.FileName
            End If
        End Sub

        ' ── Connect — top-bar command (collects passwords via dialog) ─
        Private Sub ExecuteConnect()
            ErrorBanner = ""
            If String.IsNullOrWhiteSpace(_host) Then
                ErrorBanner = "Host is required."
                Return
            End If
            If String.IsNullOrWhiteSpace(_rsfPath) Then
                ErrorBanner = "Screen layout XML path is required."
                Return
            End If

            ' Set busy immediately so the form Connect button is also disabled
            ' while password dialogs are open — prevents a second session opening.
            IsBusy = True

            Dim pwdT As String = ""
            Dim pwdL As String = ""

            RaiseEvent RequestPassword("Terminal Password for user: " & _uidT,
                                       Sub(pwd) pwdT = pwd)
            If String.IsNullOrEmpty(pwdT) Then
                IsBusy = False
                Return
            End If

            RaiseEvent RequestPassword("Logility Password for user: " & _uidL,
                                       Sub(pwd) pwdL = pwd)
            If String.IsNullOrEmpty(pwdL) Then
                IsBusy = False
                Return
            End If

            DoConnectAsync(pwdT, pwdL)
        End Sub

        ''' <summary>
        ''' Called from LoginView code-behind with passwords read from the form PasswordBoxes.
        ''' </summary>
        Public Sub ConnectWithPasswords(pwdT As String, pwdL As String)
            ErrorBanner = ""
            If String.IsNullOrWhiteSpace(_host) Then
                ErrorBanner = "Host is required."
                Return
            End If
            If String.IsNullOrWhiteSpace(_rsfPath) Then
                ErrorBanner = "Screen layout XML path is required."
                Return
            End If
            If String.IsNullOrEmpty(pwdT) Then
                ErrorBanner = "Terminal password is required."
                Return
            End If
            DoConnectAsync(pwdT, pwdL)
        End Sub

        ''' <summary>
        ''' Raised when the ViewModel needs a password (top-bar connect path).
        ''' The View handles this by showing a PasswordDialog window.
        ''' callback receives the password string (empty = cancelled).
        ''' </summary>
        Public Event RequestPassword(prompt As String, callback As Action(Of String))

        ' ── Shared connect logic ──────────────────────────────────────
        Private Async Sub DoConnectAsync(pwdT As String, pwdL As String)
            IsBusy = True
            Try
                Dim ok = Await _session.ConnectAsync(
                    _host, _rsfPath, _systemCode, _uidT, _uidL,
                    pwdT, pwdL, _timeout)

                If ok Then
                    ErrorBanner = ""
                    SaveSettings()
                Else
                    Dim detail As String = _session.LastConnectError
                    ErrorBanner = If(String.IsNullOrWhiteSpace(detail),
                        "Connection failed. Check credentials and host.",
                        "Connection failed: " & detail)
                End If
            Catch ex As Exception
                ' AggregateException or any unexpected failure escaping ConnectAsync
                ErrorBanner = "Connection error: " & ex.Message
            Finally
                ' Overwrite passwords before discarding
                pwdT = New String("X"c, 10) : pwdT = ""
                pwdL = New String("X"c, 10) : pwdL = ""
                IsBusy = False
            End Try
        End Sub

        ' ── Disconnect ───────────────────────────────────────────────
        Private Sub ExecuteDisconnect()
            _session.Disconnect()
        End Sub

        ' ── Session event handler ─────────────────────────────────────
        Private Sub OnConnectionChanged(sender As Object, e As EventArgs)
            Application.Current.Dispatcher.InvokeAsync(Sub()
                NotifyPropertyChanged("IsConnected")
                NotifyPropertyChanged("CanConnect")
                _connectCommand.RaiseCanExecuteChanged()
                _disconnectCommand.RaiseCanExecuteChanged()
            End Sub)
        End Sub

        ' ── Settings persistence ──────────────────────────────────────
        Private Sub SaveSettings()
            My.MySettings.Default.LastHost    = _host
            My.MySettings.Default.LastSystem  = _systemCode
            My.MySettings.Default.LastUidT    = _uidT
            My.MySettings.Default.LastUidL    = _uidL
            My.MySettings.Default.LastRsfPath = _rsfPath
            My.MySettings.Default.LastTimeout = _timeout
            My.MySettings.Default.Save()
        End Sub

        Private Sub LoadSettings()
            _host = If(String.IsNullOrWhiteSpace(My.MySettings.Default.LastHost),
                       DefaultHost,
                       My.MySettings.Default.LastHost)

            _systemCode = If(String.IsNullOrWhiteSpace(My.MySettings.Default.LastSystem),
                             DefaultSystemCode,
                             My.MySettings.Default.LastSystem)

            _uidT = If(String.IsNullOrWhiteSpace(My.MySettings.Default.LastUidT),
                       DefaultUid,
                       My.MySettings.Default.LastUidT)

            _uidL = If(String.IsNullOrWhiteSpace(My.MySettings.Default.LastUidL),
                       DefaultUid,
                       My.MySettings.Default.LastUidL)

            Dim savedPath As String = My.MySettings.Default.LastRsfPath
            _rsfPath = If(String.IsNullOrWhiteSpace(savedPath) OrElse
                          String.Equals(savedPath, "C:\FXF\fxf3270.rsf", StringComparison.OrdinalIgnoreCase) OrElse
                          savedPath.EndsWith(".rsf", StringComparison.OrdinalIgnoreCase),
                          DefaultRsfPath,
                          savedPath)

            _timeout = If(My.MySettings.Default.LastTimeout > 0,
                          My.MySettings.Default.LastTimeout,
                          DefaultTimeout)
        End Sub

    End Class

End Namespace
