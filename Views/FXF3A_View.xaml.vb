Option Strict On
Imports System.Windows.Controls
Namespace Views
    Public Partial Class FXF3A_View
        Public Sub New()
            InitializeComponent()
        End Sub

        ''' <summary>
        ''' Force-commit any in-progress DataGrid cell edit before the batch runs.
        ''' Wired to the Run Batch button via EventTrigger in code-behind.
        ''' </summary>
        Public Sub CommitPendingEdit()
            BatchDataGrid.CommitEdit(DataGridEditingUnit.Row, True)
        End Sub

        Private Sub RunBatch_Click(sender As Object, e As System.Windows.RoutedEventArgs)
            CommitPendingEdit()
        End Sub
    End Class
End Namespace
