Option Strict On
Option Explicit On

Imports System.Data
Imports System.Data.OleDb

Namespace Core

    ''' <summary>
    ''' Reads a named sheet from an .xlsx/.xlsm workbook using the built-in
    ''' Microsoft.ACE.OLEDB.16.0 provider (no external DLLs required).
    '''
    ''' Sheet structure expected:
    '''   Row 1  (index 0): title / description row — ignored
    '''   Row 2  (index 1 = headerRowIndex): column headers
    '''   Row 3+ (index 2+): data rows
    '''
    ''' Column normalisation: upper-case, underscores stripped, "?" stripped.
    '''   e.g.  CUST_TYPE -> CUSTTYPE,  RELEASE? -> RELEASE
    '''
    ''' Reading stops at the first data row whose ACTION column is blank.
    ''' </summary>
    Public Class ExcelLoader

        Public Shared Function LoadSheet(
                path As String,
                sheetName As String,
                headerRowIndex As Integer) As List(Of Dictionary(Of String, String))

            Dim result As New List(Of Dictionary(Of String, String))

            ' HDR=NO  — we manage the header row ourselves (it is not in row 1)
            ' IMEX=1  — treat all columns as text to avoid type-guessing
            Dim connStr As String = String.Format(
                "Provider=Microsoft.ACE.OLEDB.16.0;" &
                "Data Source={0};" &
                "Extended Properties='Excel 12.0 Xml;HDR=NO;IMEX=1'",
                path)

            Using conn As New OleDbConnection(connStr)
                conn.Open()

                ' Sheet name must be suffixed with $ for OleDb Excel queries
                Dim sql As String = String.Format("SELECT * FROM [{0}$]", sheetName)
                Using da As New OleDbDataAdapter(sql, conn)
                    Dim dt As New DataTable()
                    da.Fill(dt)

                    ' Need at least headerRow + 1 data row
                    If dt.Rows.Count <= headerRowIndex Then Return result

                    ' Build normalised-name -> column-index map from the header row
                    Dim colMap As New Dictionary(Of String, Integer)
                    Dim headerRow As DataRow = dt.Rows(headerRowIndex)
                    For c As Integer = 0 To dt.Columns.Count - 1
                        Dim raw As String = If(IsDBNull(headerRow(c)), "", headerRow(c).ToString())
                        Dim key As String = NormKey(raw)
                        If Not String.IsNullOrEmpty(key) AndAlso Not colMap.ContainsKey(key) Then
                            colMap(key) = c
                        End If
                    Next

                    ' Locate the ACTION column (stop-sentinel)
                    Dim actionColIdx As Integer = -1
                    If colMap.ContainsKey("ACTION") Then actionColIdx = colMap("ACTION")

                    ' Read data rows that follow the header row
                    For r As Integer = headerRowIndex + 1 To dt.Rows.Count - 1
                        Dim dataRow As DataRow = dt.Rows(r)

                        ' Stop at first blank ACTION cell
                        If actionColIdx >= 0 Then
                            Dim actionVal As String = CellStr(dataRow, actionColIdx)
                            If String.IsNullOrWhiteSpace(actionVal) Then Exit For
                        End If

                        Dim dict As New Dictionary(Of String, String)
                        For Each kvp As KeyValuePair(Of String, Integer) In colMap
                            dict(kvp.Key) = CellStr(dataRow, kvp.Value)
                        Next
                        result.Add(dict)
                    Next
                End Using
            End Using

            Return result
        End Function

        Private Shared Function CellStr(row As DataRow, col As Integer) As String
            If col >= row.Table.Columns.Count Then Return ""
            If IsDBNull(row(col)) Then Return ""
            Return row(col).ToString().Trim()
        End Function

        Private Shared Function NormKey(s As String) As String
            If s Is Nothing Then Return ""
            Return s.ToUpper().Replace("_", "").Replace("?", "").Trim()
        End Function

    End Class

End Namespace
