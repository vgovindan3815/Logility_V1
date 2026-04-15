Option Strict On
Imports System.Windows
Imports FXF3A_Tool.ViewModels

Namespace Views
    Public Partial Class LoginView
        Public Sub New()
            InitializeComponent()
        End Sub

        ''' <summary>
        ''' Connect button on the full login form.
        ''' Reads the two PasswordBox controls directly (PasswordBox doesn't support data binding)
        ''' and passes the values to LoginViewModel.ConnectWithPasswords.
        ''' Clears both boxes immediately after handing off.
        ''' </summary>
        Private Sub FormConnect_Click(sender As Object, e As RoutedEventArgs)
            Dim vm = TryCast(DataContext, LoginViewModel)
            If vm Is Nothing Then Return

            Dim pwdT As String = _pwdT.Password
            Dim pwdL As String = _pwdL.Password
            _pwdT.Clear()
            _pwdL.Clear()

            vm.ConnectWithPasswords(pwdT, pwdL)
        End Sub
    End Class
End Namespace
