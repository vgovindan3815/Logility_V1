Option Strict On
Option Explicit On

Imports System.Collections.Generic

Namespace Core

    ''' <summary>
    ''' Utility for parsing CSV files (RFC 4180).
    ''' Handles quoted fields, embedded commas, and escaped quotes.
    ''' </summary>
    Public NotInheritable Class CsvHelper

        ''' <summary>
        ''' Split a single CSV line into trimmed field values.
        ''' Respects double-quoted fields that may contain commas or embedded quotes.
        ''' </summary>
        Public Shared Function SplitLine(line As String) As String()
            Dim fields As New List(Of String)
            Dim sb As New System.Text.StringBuilder()
            Dim inQuote As Boolean = False
            Dim i As Integer = 0
            While i < line.Length
                Dim c As Char = line(i)
                If inQuote Then
                    If c = """"c Then
                        If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                            sb.Append(""""c)   ' Escaped "" -> "
                            i += 1
                        Else
                            inQuote = False    ' End of quoted field
                        End If
                    Else
                        sb.Append(c)
                    End If
                Else
                    If c = """"c Then
                        inQuote = True
                    ElseIf c = ","c Then
                        fields.Add(sb.ToString().Trim())
                        sb.Clear()
                    Else
                        sb.Append(c)
                    End If
                End If
                i += 1
            End While
            fields.Add(sb.ToString().Trim())
            Return fields.ToArray()
        End Function

        ''' <summary>
        ''' Build a column-name-to-index map from the header row.
        ''' Keys are case-insensitive trimmed column names.
        ''' </summary>
        Public Shared Function BuildHeaderMap(headerLine As String) As Dictionary(Of String, Integer)
            Dim map As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim fields = SplitLine(headerLine)
            For i As Integer = 0 To fields.Length - 1
                Dim name = fields(i).Trim()
                If Not String.IsNullOrEmpty(name) AndAlso Not map.ContainsKey(name) Then
                    map.Add(name, i)
                End If
            Next
            Return map
        End Function

        ''' <summary>
        ''' Get a field value by column name.
        ''' Returns empty string if column not found or index out of range.
        ''' </summary>
        Public Shared Function GetField(fields() As String,
                                        map As Dictionary(Of String, Integer),
                                        columnName As String) As String
            Dim idx As Integer
            If Not map.TryGetValue(columnName, idx) Then Return ""
            If idx >= fields.Length Then Return ""
            Return fields(idx)
        End Function

        ''' <summary>
        ''' Get a field value, returning defaultValue if empty.
        ''' </summary>
        Public Shared Function GetFieldOrDefault(fields() As String,
                                                 map As Dictionary(Of String, Integer),
                                                 columnName As String,
                                                 defaultValue As String) As String
            Dim v = GetField(fields, map, columnName)
            Return If(String.IsNullOrEmpty(v), defaultValue, v)
        End Function

        ''' <summary>
        ''' Parse a boolean field: true/yes/1 -> True, anything else -> False.
        ''' </summary>
        Public Shared Function GetBool(fields() As String,
                                       map As Dictionary(Of String, Integer),
                                       columnName As String) As Boolean
            Dim v = GetField(fields, map, columnName).ToLower()
            Return v = "true" OrElse v = "yes" OrElse v = "1"
        End Function

    End Class

End Namespace
