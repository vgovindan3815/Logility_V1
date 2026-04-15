Option Strict On
Option Explicit On

Imports System.Windows.Input

Namespace Core

    ''' <summary>
    ''' Parameterless ICommand implementation.
    ''' Use for buttons that don't need to pass data to the ViewModel.
    ''' </summary>
    Public Class RelayCommand
        Implements ICommand

        Private ReadOnly _execute As Action
        Private ReadOnly _canExecute As Func(Of Boolean)

        Public Sub New(execute As Action,
                       Optional canExecute As Func(Of Boolean) = Nothing)
            If execute Is Nothing Then Throw New ArgumentNullException("execute")
            _execute = execute
            _canExecute = canExecute
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean _
                Implements ICommand.CanExecute
            Return _canExecute Is Nothing OrElse _canExecute()
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _execute()
        End Sub

        Public Event CanExecuteChanged As EventHandler _
                Implements ICommand.CanExecuteChanged

        ''' <summary>
        ''' Call this from the ViewModel when the CanExecute condition may have changed.
        ''' Typically called after a property that affects CanExecute is set.
        ''' </summary>
        Public Sub RaiseCanExecuteChanged()
            RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
        End Sub

    End Class

    ''' <summary>
    ''' Typed ICommand implementation.
    ''' Use for commands that receive a parameter from the View binding
    ''' (e.g. DataGrid selected item, ComboBox selection).
    ''' </summary>
    Public Class RelayCommand(Of T)
        Implements ICommand

        Private ReadOnly _execute As Action(Of T)
        Private ReadOnly _canExecute As Func(Of T, Boolean)

        Public Sub New(execute As Action(Of T),
                       Optional canExecute As Func(Of T, Boolean) = Nothing)
            If execute Is Nothing Then Throw New ArgumentNullException("execute")
            _execute = execute
            _canExecute = canExecute
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean _
                Implements ICommand.CanExecute
            If _canExecute Is Nothing Then Return True
            If TypeOf parameter Is T Then
                Return _canExecute(DirectCast(parameter, T))
            End If
            Return True
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If TypeOf parameter Is T Then
                _execute(DirectCast(parameter, T))
            End If
        End Sub

        Public Event CanExecuteChanged As EventHandler _
                Implements ICommand.CanExecuteChanged

        Public Sub RaiseCanExecuteChanged()
            RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
        End Sub

    End Class

End Namespace
