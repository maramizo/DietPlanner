Public Class RecommendedIntake
    Public Sub New()
        InitializeComponent()
        Dim currentSettings = New NutrientInfo().RecommendedDailyIntakes
        For Each setting As KeyValuePair(Of String, Double) In currentSettings
            RecommendedGridView.Rows.Add(setting.Key, setting.Value)
        Next
    End Sub

    Private Sub RecommendedGridView_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles RecommendedGridView.CellFormatting
        If e.RowIndex >= 0 AndAlso e.ColumnIndex = 1 Then
            If IsNumeric(e.Value) Then
                Dim nutrient = RecommendedGridView.Rows(e.RowIndex).Cells(0).Value.ToString()
                Dim nutrition = New Nutrition(nutrient, e.Value)
                e.Value = nutrition.FormattedAmount
                e.FormattingApplied = True
            End If
        End If
    End Sub

    Private Sub Cancel_Click(sender As Object, e As EventArgs) Handles Cancel.Click
        Close()
    End Sub

    Private Sub Save_Click(sender As Object, e As EventArgs) Handles Save.Click
        Dim currentSettings = New NutrientInfo()
        Dim newSettings As New Dictionary(Of String, Double)

        For Each row As DataGridViewRow In RecommendedGridView.Rows
            If row.Cells(0).Value IsNot Nothing Then
                Dim value As Double
                If row.Cells(1).Value Is Nothing Or row.Cells(1).Value.ToString() = "" Then
                    value = 0
                Else
                    If Not Double.TryParse(row.Cells(1).Value, value) Then
                        MessageBox.Show("Nutritional value must be a number")
                        Return
                    End If
                End If
                newSettings.Add(row.Cells(0).Value, value)
            End If
        Next

        currentSettings.RecommendedDailyIntakes = newSettings
        currentSettings.Save()
        Close()
    End Sub
End Class