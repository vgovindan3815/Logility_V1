Option Strict On
Option Explicit On

Imports System.Windows
Imports FXF3A_Tool.ViewModels

Namespace Views

    Public Partial Class MainWindow

        Private ReadOnly _vm As MainViewModel

        Public Sub New()
            InitializeComponent()
            _vm = New MainViewModel()
            DataContext = _vm

            ' Wire the password dialog handler from LoginViewModel
            AddHandler _vm.LoginVM.RequestPassword, AddressOf OnRequestPassword

            ' Wire closing handler
            AddHandler Me.Closing, AddressOf MainWindow_Closing

            ' Restore window position from settings
            If My.MySettings.Default.WindowWidth > 0 Then
                Width  = My.MySettings.Default.WindowWidth
                Height = My.MySettings.Default.WindowHeight
                Left   = My.MySettings.Default.WindowLeft
                Top    = My.MySettings.Default.WindowTop
            End If
        End Sub

        ' ── Password dialog ──────────────────────────────────────────
        ' The ViewModel raises RequestPassword when it needs credentials.
        ' We handle it here in code-behind — the only code-behind logic allowed.
        Private Sub OnRequestPassword(prompt As String, callback As Action(Of String))
            Dim dlg As New PasswordDialog(prompt)
            dlg.Owner = Me
            Dim result = dlg.ShowDialog()
            If result = True Then
                callback(dlg.Password)
            Else
                callback("")
            End If
        End Sub

        ' ── Window closing — save geometry and disconnect ─────────────
        Private Sub MainWindow_Closing(sender As Object,
                e As ComponentModel.CancelEventArgs)
            My.MySettings.Default.WindowWidth  = Width
            My.MySettings.Default.WindowHeight = Height
            My.MySettings.Default.WindowLeft   = Left
            My.MySettings.Default.WindowTop    = Top
            My.MySettings.Default.Save()

            ' Disconnect if still connected
            Dim session = Core.SessionManager.Instance
            If session.IsConnected Then
                session.Disconnect()
            End If
        End Sub

    End Class

End Namespace
