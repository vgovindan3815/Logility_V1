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

End Class
