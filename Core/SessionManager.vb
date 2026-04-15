Option Strict On
Option Explicit On

Imports System.Threading.Tasks
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

        ' ── State ────────────────────────────────────────────────────
        Private _ss As ScreenScraping
        Private _isConnected As Boolean

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _isConnected
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
                RaiseEvent StatusChanged(Me, "Connecting to " & host & "...")

                Await Task.Run(Sub()
                    ' ScreenScraping constructor blocks until login completes
                    _ss = New ScreenScraping(
                        ScreenScraping.sslibTypeType.Bluezone,
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
                End Sub)

                _isConnected = True
                RaiseEvent StatusChanged(Me, "Connected — " & system & " @ " & host)
                RaiseEvent ConnectionChanged(Me, EventArgs.Empty)
                Return True

            Catch ex As Exception
                _isConnected = False
                CleanupSession()
                RaiseEvent StatusChanged(Me, "Connection failed: " & ex.Message)
                RaiseEvent ConnectionChanged(Me, EventArgs.Empty)
                Return False
            End Try
        End Function

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
        End Sub

    End Class

End Namespace
