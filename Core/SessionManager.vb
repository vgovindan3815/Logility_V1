Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Concurrent
Imports FedEx.PABST.SS.SSLib
Imports FedEx.PABST.SS.Screens

Namespace Core

    ''' <summary>
    ''' Singleton that owns the ScreenScraping session and all screen objects.
    ''' All ViewModels reference this to perform screen operations.
    ''' Connection runs on a background Task — never blocks the UI thread.
    ''' </summary>
    Public Class SessionManager

        ' ── Singleton ────────────────────────────────────────────────
        Private Shared _instance As SessionManager
        Public Shared ReadOnly Property Instance As SessionManager
            Get
                If _instance Is Nothing Then _instance = New SessionManager()
                Return _instance
            End Get
        End Property

        Private Sub New()
        End Sub

        ' ── Events ───────────────────────────────────────────────────
        ''' <summary>Fired when connected or disconnected.</summary>
        Public Event ConnectionChanged(sender As Object, e As EventArgs)

        ''' <summary>Fired with a status message string during connect/disconnect.</summary>
        Public Event StatusChanged(sender As Object, message As String)

        ' ── Dedicated session thread ─────────────────────────────────
        ' tn3270_dll.dll has thread affinity: every call after creation must come
        ' from the same OS thread.  We own a single long-lived Thread and funnel
        ' all screen-scraping work through it via _workQueue.
        Private _workQueue As BlockingCollection(Of Action)
        Private _sessionThread As Thread

        Private Sub EnsureSessionThread()
            If _sessionThread IsNot Nothing AndAlso _sessionThread.IsAlive Then Return
            _workQueue = New BlockingCollection(Of Action)()
            _sessionThread = New Thread(Sub()
                For Each work In _workQueue.GetConsumingEnumerable()
                    work()
                Next
            End Sub)
            _sessionThread.IsBackground = True
            _sessionThread.Name = "SessionThread"
            _sessionThread.Start()
        End Sub

        Private Sub StopSessionThread()
            Try
                If _workQueue IsNot Nothing Then _workQueue.CompleteAdding()
            Catch : End Try
            Try
                If _sessionThread IsNot Nothing Then
                    _sessionThread.Join(5000)
                End If
            Catch : End Try
            _sessionThread = Nothing
            _workQueue = Nothing
        End Sub

        ''' <summary>
        ''' Schedules <paramref name="action"/> on the dedicated session thread and
        ''' returns a Task that completes (or faults) when the action finishes.
        ''' Awaiting this from the UI thread keeps the UI responsive while ensuring
        ''' tn3270_dll.dll is always called from the same thread.
        ''' </summary>
        Public Function RunOnSessionThreadAsync(action As Action) As Task
            Dim tcs As New TaskCompletionSource(Of Boolean)()
            _workQueue.Add(Sub()
                Try
                    action()
                    tcs.SetResult(True)
                Catch ex As Exception
                    tcs.SetException(ex)
                End Try
            End Sub)
            Return tcs.Task
        End Function

        ' ── State ────────────────────────────────────────────────────
        Private _ss As ScreenScraping
        Private _isConnected As Boolean
        Private _lastConnectError As String = ""

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _isConnected
            End Get
        End Property

        ''' <summary>
        ''' The exception message from the most recent failed ConnectAsync call.
        ''' Empty string if the last connection attempt succeeded.
        ''' </summary>
        Public ReadOnly Property LastConnectError As String
            Get
                Return _lastConnectError
            End Get
        End Property

        ' ── Screen object accessors ──────────────────────────────────
        ' Only valid after ConnectAsync returns True.
        Private _fxf3a As FXF3A
        Private _fxf3b As FXF3B
        Private _fxf3c As FXF3C
        Private _fxf3d As FXF3D
        Private _fxf3e As FXF3E
        Private _fxf3f As FXF3F
        Private _fxf3g As FXF3G
        Private _fxf3j As FXF3J
        Private _fxf3k As FXF3K
        Private _fxf3m As FXF3M
        Private _fxf3n As FXF3N
        Private _fxf4m As FXF4M

        Public ReadOnly Property FXF3A As FXF3A
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3a
            End Get
        End Property

        Public ReadOnly Property FXF3B As FXF3B
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3b
            End Get
        End Property

        Public ReadOnly Property FXF3C As FXF3C
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3c
            End Get
        End Property

        Public ReadOnly Property FXF3D As FXF3D
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3d
            End Get
        End Property

        Public ReadOnly Property FXF3E As FXF3E
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3e
            End Get
        End Property

        Public ReadOnly Property FXF3F As FXF3F
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3f
            End Get
        End Property

        Public ReadOnly Property FXF3G As FXF3G
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3g
            End Get
        End Property

        Public ReadOnly Property FXF3J As FXF3J
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3j
            End Get
        End Property

        Public ReadOnly Property FXF3K As FXF3K
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3k
            End Get
        End Property

        Public ReadOnly Property FXF3M As FXF3M
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3m
            End Get
        End Property

        Public ReadOnly Property FXF3N As FXF3N
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf3n
            End Get
        End Property

        Public ReadOnly Property FXF4M As FXF4M
            Get
                If Not _isConnected Then Throw New InvalidOperationException("Not connected.")
                Return _fxf4m
            End Get
        End Property

        ' ── Connect ──────────────────────────────────────────────────
        ''' <summary>
        ''' Connects to the mainframe on a background thread.
        ''' Passwords are used and immediately cleared from memory.
        ''' Returns True on success, False on failure (exception message via StatusChanged).
        ''' </summary>
        Public Async Function ConnectAsync(host As String,
                                           xmlPath As String,
                                           system As String,
                                           uidT As String,
                                           uidL As String,
                                           pwdT As String,
                                           pwdL As String,
                                           timeoutMs As Integer) As Task(Of Boolean)
            Try
                _lastConnectError = ""
                RaiseEvent StatusChanged(Me, "Connecting to " & host & "...")

                ValidateLayoutPath(xmlPath)

                ' Start (or reuse) the dedicated session thread BEFORE posting work to it
                EnsureSessionThread()

                Await RunOnSessionThreadAsync(Sub()
                    ' Add tn3270 DLL directory to PATH for FedEx_Emu to find it
                    Dim dllDir As String = "C:\fedex"
                    Dim currentPath As String = Environment.GetEnvironmentVariable("PATH")
                    If Not currentPath.Contains(dllDir) Then
                        Environment.SetEnvironmentVariable("PATH", dllDir & ";" & currentPath)
                    End If

                    ' ScreenScraping constructor blocks until login completes
                    _ss = New ScreenScraping(
                        ScreenScraping.sslibTypeType.FedEx_Emu,
                        host,
                        xmlPath,
                        timeoutMs,
                        system,
                        uidT,
                        uidL,
                        pwdT,
                        pwdL,
                        ScreenScraping.connectionType.FREIGHT,
                        True)

                    ' Instantiate all screen objects — share the same session
                    _fxf3a = New FXF3A(_ss)
                    _fxf3b = New FXF3B(_ss)
                    _fxf3c = New FXF3C(_ss)
                    _fxf3d = New FXF3D(_ss)
                    _fxf3e = New FXF3E(_ss)
                    _fxf3f = New FXF3F(_ss)
                    _fxf3g = New FXF3G(_ss)
                    _fxf3j = New FXF3J(_ss)
                    _fxf3k = New FXF3K(_ss)
                    _fxf3m = New FXF3M(_ss)
                    _fxf3n = New FXF3N(_ss)
                    _fxf4m = New FXF4M(_ss)

                    ' Navigate the Logility session back to SELECT: so the first gotoScreen
                    ' call works correctly.  Using gotoLogility() via reflection — the same
                    ' approach the DLL itself uses — rather than the full Login() which
                    ' re-authenticates and costs ~55 seconds.
                    Try
                        Dim gotoLogilityMethod = _ss.LOGIN.GetType().GetMethod("gotoLogility")
                        If gotoLogilityMethod IsNot Nothing Then
                            gotoLogilityMethod.Invoke(_ss.LOGIN, Nothing)
                        End If
                    Catch
                        ' If navigation fails here the session is still valid;
                        ' gotoScreen() will recover on the first batch call.
                    End Try
                End Sub)

                _isConnected = True
                RaiseEvent StatusChanged(Me, "Connected — " & system & " @ " & host)
                RaiseEvent ConnectionChanged(Me, EventArgs.Empty)
                Return True

            Catch ex As Exception
                _lastConnectError = BuildConnectErrorMessage(ex, xmlPath)
                _isConnected = False
                CleanupSession()
                WriteConnectErrorLog(ex, xmlPath)
                RaiseEvent StatusChanged(Me, "Connection failed: " & _lastConnectError)
                RaiseEvent ConnectionChanged(Me, EventArgs.Empty)
                Return False
            End Try
        End Function

        Private Function BuildConnectErrorMessage(ex As Exception, xmlPath As String) As String
            Dim baseMessage As String = ex.Message

            If baseMessage IsNot Nothing AndAlso
               baseMessage.IndexOf("unexpected token", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
               baseMessage.IndexOf("expected token is ';'", StringComparison.OrdinalIgnoreCase) >= 0 Then

                Return String.Format(
                    "{0} Layout XML: {1}. This usually indicates a layout parsing mismatch or special-character encoding issue in the selected XML file. Compare this file with a known-good team copy.",
                    baseMessage,
                    xmlPath)
            End If

            Return String.Format("{0} Layout XML: {1}", baseMessage, xmlPath)
        End Function

        Private Shared Sub WriteConnectErrorLog(ex As Exception, xmlPath As String)
            Try
                Dim logPath As String = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "connect_error.txt")
                Dim sb As New System.Text.StringBuilder()
                sb.AppendLine("Timestamp: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                sb.AppendLine("Layout XML: " & xmlPath)
                sb.AppendLine("Type:      " & ex.GetType().FullName)
                sb.AppendLine("Message:   " & ex.Message)
                sb.AppendLine("Stack:")
                sb.AppendLine(ex.StackTrace)
                Dim inner As Exception = ex.InnerException
                Dim depth As Integer = 1
                Do While inner IsNot Nothing
                    sb.AppendLine(String.Format("InnerException ({0}):", depth))
                    sb.AppendLine("  Type:    " & inner.GetType().FullName)
                    sb.AppendLine("  Message: " & inner.Message)
                    sb.AppendLine("  Stack:")
                    sb.AppendLine(inner.StackTrace)
                    inner = inner.InnerException
                    depth += 1
                Loop
                System.IO.File.WriteAllText(logPath, sb.ToString(), System.Text.Encoding.UTF8)
            Catch
                ' Never let logging crash the app
            End Try
        End Sub

        ''' <summary>
        ''' Validates that the supplied layout path is present and points to an existing file.
        ''' Detailed content parsing remains inside the ScreenScraping library.
        ''' </summary>
        Private Sub ValidateLayoutPath(xmlPath As String)
            If String.IsNullOrWhiteSpace(xmlPath) Then
                Throw New InvalidDataException("Screen layout XML path is empty.")
            End If

            If Not File.Exists(xmlPath) Then
                Throw New FileNotFoundException("Screen layout XML file was not found.", xmlPath)
            End If
        End Sub

        ' ── Disconnect ───────────────────────────────────────────────
        Public Sub Disconnect()
            Try
                RaiseEvent StatusChanged(Me, "Disconnecting...")
                CleanupSession()
                _isConnected = False
                RaiseEvent StatusChanged(Me, "Disconnected")
                RaiseEvent ConnectionChanged(Me, EventArgs.Empty)
            Catch ex As Exception
                ' Always mark as disconnected even if cleanup throws
                _isConnected = False
                RaiseEvent ConnectionChanged(Me, EventArgs.Empty)
            End Try
        End Sub

        ' ── Internal cleanup ─────────────────────────────────────────
        Private Sub CleanupSession()
            ' Null out screen objects
            _fxf3a = Nothing
            _fxf3b = Nothing
            _fxf3c = Nothing
            _fxf3d = Nothing
            _fxf3e = Nothing
            _fxf3f = Nothing
            _fxf3g = Nothing
            _fxf3j = Nothing
            _fxf3k = Nothing
            _fxf3m = Nothing
            _fxf3n = Nothing
            _fxf4m = Nothing

            ' Close and kill Bluezone session
            If _ss IsNot Nothing Then
                Try
                    _ss.Close()
                Catch : End Try
                Try
                    If _ss.SSProcess IsNot Nothing Then
                        _ss.SSProcess.Kill()
                    End If
                Catch : End Try
                _ss = Nothing
            End If

            ' Stop the dedicated session thread so the next Connect starts fresh
            StopSessionThread()
        End Sub

    End Class

End Namespace
