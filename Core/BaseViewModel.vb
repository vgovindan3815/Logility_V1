Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Namespace Core

    ''' <summary>
    ''' Base class for all ViewModels.
    ''' Implements INotifyPropertyChanged with a SetField helper
    ''' that only raises PropertyChanged when the value actually changes.
    ''' </summary>
    Public MustInherit Class BaseViewModel
        Implements INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler _
            Implements INotifyPropertyChanged.PropertyChanged

        ''' <summary>Raise PropertyChanged for the calling property name.</summary>
        Protected Sub OnPropertyChanged(<CallerMemberName> Optional name As String = "")
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
        End Sub

        ''' <summary>
        ''' Set a backing field and raise PropertyChanged only if the value changed.
        ''' Returns True if the value was changed.
        ''' </summary>
        Protected Function SetField(Of T)(ByRef field As T, value As T,
                <CallerMemberName> Optional name As String = "") As Boolean
            If EqualityComparer(Of T).Default.Equals(field, value) Then Return False
            field = value
            OnPropertyChanged(name)
            Return True
        End Function

        ''' <summary>
        ''' Raise PropertyChanged for a specific property name.
        ''' Use when you need to notify about a computed property
        ''' after setting a backing field for a different property.
        ''' </summary>
        Protected Sub NotifyPropertyChanged(name As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
        End Sub

    End Class

End Namespace
