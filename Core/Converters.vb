Option Strict On
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data

Namespace Core
    Public Class BoolToVisibilityConverter : Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type,
                parameter As Object, culture As CultureInfo) As Object _
                Implements IValueConverter.Convert
            Return If(CBool(value), Visibility.Visible, Visibility.Collapsed)
        End Function
        Public Function ConvertBack(value As Object, targetType As Type,
                parameter As Object, culture As CultureInfo) As Object _
                Implements IValueConverter.ConvertBack
            Return value IsNot Nothing AndAlso DirectCast(value, Visibility) = Visibility.Visible
        End Function
    End Class

    Public Class StringToVisibilityConverter : Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type,
                parameter As Object, culture As CultureInfo) As Object _
                Implements IValueConverter.Convert
            Return If(Not String.IsNullOrWhiteSpace(TryCast(value, String)),
                      Visibility.Visible, Visibility.Collapsed)
        End Function
        Public Function ConvertBack(value As Object, targetType As Type,
                parameter As Object, culture As CultureInfo) As Object _
                Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class BoolToStatusColorConverter : Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type,
                parameter As Object, culture As CultureInfo) As Object _
                Implements IValueConverter.Convert
            Return If(CBool(value), "#10B981", "#888888")
        End Function
        Public Function ConvertBack(value As Object, targetType As Type,
                parameter As Object, culture As CultureInfo) As Object _
                Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace