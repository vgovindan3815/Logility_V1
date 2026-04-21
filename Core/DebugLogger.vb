Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text
Imports FXF3A_Tool.Models

Namespace Core

    ''' <summary>
    ''' Appends error records to a rolling debug log file next to the executable.
    ''' Never throws — logging failures are silently swallowed.
    ''' </summary>
    Friend NotInheritable Class DebugLogger

        Private Shared ReadOnly LogFileName As String = "debug_log.txt"

        Private Shared ReadOnly Property LogPath As String
            Get
                Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName)
            End Get
        End Property

        ''' <summary>Appends one error entry for a batch row to the debug log.</summary>
        Public Shared Sub LogError(row As BatchRowBase, ex As Exception)
            Try
                Dim sb As New StringBuilder()
                sb.AppendLine("--- " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & " ---")
                sb.AppendLine("Row    : " & row.Account & "  Authority=" & row.Authority & "  Number=" & row.Number & "  Item=" & row.Item)
                sb.AppendLine("Action : " & row.Action)
                sb.AppendLine("Type   : " & ex.GetType().FullName)
                sb.AppendLine("Message: " & ex.Message)
                If ex.StackTrace IsNot Nothing Then
                    sb.AppendLine("Stack  : " & ex.StackTrace)
                End If
                Dim inner As Exception = ex.InnerException
                Dim depth As Integer = 1
                Do While inner IsNot Nothing
                    sb.AppendLine(String.Format("Inner ({0}): {1}: {2}", depth, inner.GetType().Name, inner.Message))
                    inner = inner.InnerException
                    depth += 1
                Loop
                sb.AppendLine()
                File.AppendAllText(LogPath, sb.ToString(), Encoding.UTF8)
            Catch
                ' Never let logging crash the app
            End Try
        End Sub

        ''' <summary>Returns the full path of the log file.</summary>
        Public Shared Function GetLogPath() As String
            Return LogPath
        End Function

        ''' <summary>True if a non-empty log file exists.</summary>
        Public Shared Function LogExists() As Boolean
            Try
                Return File.Exists(LogPath) AndAlso New FileInfo(LogPath).Length > 0
            Catch
                Return False
            End Try
        End Function

    End Class

End Namespace
