Option Strict On
Imports System.Windows.Controls
Namespace Views
    Public Partial Class FXF3B_View
        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub CommitPendingEdit()
            BatchDataGrid.CommitEdit(DataGridEditingUnit.Row, True)
        End Sub

        Private Sub RunBatch_Click(sender As Object, e As System.Windows.RoutedEventArgs)
            CommitPendingEdit()
        End Sub
    End Class
End Namespace
