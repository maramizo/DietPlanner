Public Class DailyFacts

    Private ReadOnly _meals As List(Of Meal)

    Public Sub New(meals As List(Of Meal))
        InitializeComponent()
        ApplyAppIcon(Me)
        _meals = If(meals, New List(Of Meal)).ToList()
        RefreshFacts()
    End Sub

    Private Sub RefreshFacts()
        NutritionalsDataGrid.Rows.Clear()
        Dim totalCalories As Integer = 0
        Dim totalCookTime As Integer = 0
        Dim totals As New Dictionary(Of String, Double)
        Dim nutrientInfo As New NutrientInfo()

        For Each meal As Meal In _meals
            totalCookTime += meal.TotalTime
            totalCalories += meal.Calory
            For Each unparsedNutritional As KeyValuePair(Of String, Double) In meal.Nutritionals
                If totals.ContainsKey(unparsedNutritional.Key) Then
                    totals(unparsedNutritional.Key) += unparsedNutritional.Value
                Else
                    totals.Add(unparsedNutritional.Key, unparsedNutritional.Value)
                End If
            Next
        Next

        For Each unparsedNutritional As KeyValuePair(Of String, Double) In totals
            Dim nutritional = New Nutrition(unparsedNutritional.Key, Math.Round(unparsedNutritional.Value, 2))
            Dim recommendedAmount As Decimal
            Dim found As Boolean = False
            Try
                recommendedAmount = nutrientInfo.RecommendedDailyIntakes(nutritional.Name)
                found = True
            Catch ex As KeyNotFoundException
                recommendedAmount = Math.Max(nutritional.Amount, 1)
            End Try
            Dim pctRecommended As Double = 0
            If recommendedAmount > 0 Then
                pctRecommended = nutritional.Amount / recommendedAmount * 100
            Else
                found = False
            End If
            If Not found Then
                Dim formattedRecommendedAmount = "—"
                NutritionalsDataGrid.Rows.Add(nutritional.Name, nutritional.FormattedAmount, "—", formattedRecommendedAmount)
            Else
                Dim formattedRecommendedAmount = New Nutrition(nutritional.Name, recommendedAmount).FormattedAmount
                pctRecommended = Math.Round(pctRecommended, 2)
                NutritionalsDataGrid.Rows.Add(nutritional.Name, nutritional.FormattedAmount, pctRecommended & "%", formattedRecommendedAmount)
            End If
            'If the nutritional is over the recommended amount, color it red
            If pctRecommended > 120 Then
                NutritionalsDataGrid.Rows(NutritionalsDataGrid.Rows.Count - 1).Cells(2).Style.BackColor = Color.Red
            ElseIf pctRecommended < 50 And found Then
                NutritionalsDataGrid.Rows(NutritionalsDataGrid.Rows.Count - 1).Cells(2).Style.BackColor = Color.Yellow
            ElseIf found Then
                NutritionalsDataGrid.Rows(NutritionalsDataGrid.Rows.Count - 1).Cells(2).Style.BackColor = Color.Green
            End If
        Next

        CaloriesLabel.Text = totalCalories.ToString() + " calories"
        CookTimeLabel.Text = totalCookTime.ToString() + " minutes"
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Close()
    End Sub

    Private Sub ChangeButton_Click(sender As Object, e As EventArgs) Handles ChangeButton.Click
        Dim changeIntakes = New RecommendedIntake()
        If changeIntakes.ShowDialog(Me) = DialogResult.OK Then
            RefreshFacts()
        End If
    End Sub
End Class
