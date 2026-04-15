Option Strict On
Option Explicit On

Imports System.Windows

Namespace Views

    ''' <summary>
    ''' Modal password input dialog.
    ''' Password is read from the PasswordBox and cleared after OK.
    ''' Caller reads the Password property, then it is cleared.
    ''' </summary>
    Public Partial Class PasswordDialog

        Private _password As String = ""

        ''' <summary>The entered password. Empty string if cancelled.</summary>
        Public ReadOnly Property Password As String
            Get
                Return _password
            End Get
        End Property

        Public Sub New(prompt As String)
            InitializeComponent()
            PromptText.Text = prompt
            PwdBox.Focus()
            AddHandler Me.Closed, AddressOf PasswordDialog_Closed
        End Sub

        Private Sub OkButton_Click(sender As Object, e As RoutedEventArgs)
            _password = PwdBox.Password
            PwdBox.Clear()
            DialogResult = True
        End Sub

        Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs)
            PwdBox.Clear()
            _password = ""
            DialogResult = False
        End Sub

        Private Sub PasswordDialog_Closed(sender As Object, e As EventArgs)
            ' Ensure password is cleared if window is closed via X
            PwdBox.Clear()
        End Sub

    End Class

End Namespace
