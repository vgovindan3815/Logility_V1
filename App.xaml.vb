Option Strict On
Option Explicit On

Imports System.Windows

''' <summary>
''' Code-behind for App.xaml.
''' Inherits declared here so the first-pass compiler sees the Application type
''' correctly and Application.Current resolves to System.Windows.Application.Current
''' in all ViewModels. The generated App.g.vb also declares Inherits with the same
''' base type, which VB.NET permits when both parts specify the same base.
''' </summary>
Public Partial Class Application
    Inherits System.Windows.Application

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)
        AddHandler AppDomain.CurrentDomain.UnhandledException,
                   AddressOf OnUnhandledException
        AddHandler Current.DispatcherUnhandledException,
                   AddressOf OnDispatcherUnhandledException
    End Sub

    ''' <summary>
    ''' Catches unhandled exceptions on background threads (e.g. Bluezone internal threads).
    ''' The process may still terminate after this handler returns (IsTerminating=True),
    ''' but the user sees the error message before that happens.
    ''' </summary>
    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim msg As String = "Unknown error"
        If TypeOf e.ExceptionObject Is Exception Then
            msg = DirectCast(e.ExceptionObject, Exception).Message
        End If

        Current.Dispatcher.Invoke(Sub()
            Dim shown As Boolean = TrySetLoginError("Connection error: " & msg)
            If Not shown Then
                MessageBox.Show(
                    "Connection error: " & msg & vbCrLf & vbCrLf &
                    "Check credentials, host, and Bluezone availability.",
                    "FXF3A Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error)
            End If
        End Sub)
    End Sub

    ''' <summary>
    ''' Catches unhandled exceptions on the UI/Dispatcher thread.
    ''' Marks the exception as handled so WPF does not shut down the application.
    ''' </summary>
    Private Sub OnDispatcherUnhandledException(sender As Object,
            e As Threading.DispatcherUnhandledExceptionEventArgs)
        e.Handled = True
        TrySetLoginError("Connection error: " & e.Exception.Message)
    End Sub

    ''' <summary>
    ''' Pushes an error message into LoginVM.ErrorBanner and resets IsBusy.
    ''' Returns True if the LoginVM was reachable, False if not yet initialised.
    ''' </summary>
    Private Function TrySetLoginError(msg As String) As Boolean
        If Current.MainWindow Is Nothing Then Return False
        Dim mainVm = TryCast(Current.MainWindow.DataContext,
                             Logility_Freight.ViewModels.MainViewModel)
        If mainVm Is Nothing OrElse mainVm.LoginVM Is Nothing Then Return False
        mainVm.LoginVM.IsBusy    = False
        mainVm.LoginVM.ErrorBanner = msg
        Return True
    End Function

End Class
