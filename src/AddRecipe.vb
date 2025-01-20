Imports Newtonsoft.Json
Imports System.IO

Public Class AddRecipe

    Private Sub AddRecipe_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each nutritional As String In AllNutritionals
            NutritionalsDataGrid.Rows.Add(nutritional, "")
        Next
    End Sub

    Private Sub DataGridView_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles NutritionalsDataGrid.CellFormatting
        If e.RowIndex >= 0 AndAlso e.ColumnIndex = 1 Then ' Assuming column 1 contains the nutrient amount
            ' Check if the value is numeric (e.g., 31.0 or 31)
            If IsNumeric(e.Value) Then
                Dim nutrient = NutritionalsDataGrid.Rows(e.RowIndex).Cells(0).Value.ToString()
                Dim nutrition = New Nutrition(nutrient, e.Value)
                e.Value = nutrition.FormattedAmount
                Console.WriteLine(e.Value, nutrition.FormattedAmount, NutritionalsDataGrid.Rows(e.RowIndex).Cells(0))
            End If
        End If
    End Sub


    Private Sub SaveButton_Click(sender As Object, e As EventArgs) Handles SaveButton.Click
        Dim nutritionals As New Dictionary(Of String, Double)

        'Check that all data is entered
        If NameTextBox.Text = "" Or CaloriesTextBox.Text = "" Or RecipeTextBox.Text = "" Then
            MessageBox.Show("Please fill in all fields")
            Return
        End If

        'Store nutritionals
        For Each row As DataGridViewRow In NutritionalsDataGrid.Rows
            If row.Cells(0).Value IsNot Nothing Then
                If nutritionals.ContainsKey(row.Cells(0).Value) Then
                    MessageBox.Show("Nutritional already exists. Please remove duplicates.")
                    Return
                End If
                Dim value As Double
                If row.Cells(1).Value Is Nothing Or row.Cells(1).Value = "" Then
                    value = 0
                Else
                    If Not Double.TryParse(row.Cells(1).Value, value) Then
                        MessageBox.Show("Nutritional value must be a number")
                        Return
                    End If
                End If
                nutritionals.Add(row.Cells(0).Value, value)
            End If
        Next

        'Store meal
        Dim meal As New Meal(NameTextBox.Text, CaloriesTextBox.Text, nutritionals, RecipeTextBox.Text, PrepTimeTextBox.Text, CookTimeTextBox.Text)
        Dim json As String
        If Directory.Exists("./data") And File.Exists("./data/meals.json") Then
            json = File.ReadAllText("./data/meals.json")
        Else
            json = "[]"
        End If
        Dim meals As List(Of Meal) = JsonConvert.DeserializeObject(Of List(Of Meal))(json)
        meals.Add(meal)
        json = JsonConvert.SerializeObject(meals)
        If Not Directory.Exists("./data") Then
            Directory.CreateDirectory("./data")
        End If
        File.WriteAllText("./data/meals.json", json)
        Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Close()
    End Sub

    Private Async Sub ScrapeButton_Click(sender As Object, e As EventArgs) Handles ScrapeButton.Click
        Dim loader = New Loading("Scraping nutritionals...")
        loader.Show()
        Dim meal = Await API.ScrapeNutritionals(RecipeTextBox.Text)
        If meal Is Nothing Then
            MessageBox.Show("Could not scrape nutritionals")
            Return
        End If

        NameTextBox.Text = meal.Name
        CaloriesTextBox.Text = meal.Calory
        PrepTimeTextBox.Text = meal.PrepTime
        CookTimeTextBox.Text = meal.CookTime

        'Clear all grid rows then add the new ones
        NutritionalsDataGrid.Rows.Clear()
        For Each nutritional As KeyValuePair(Of String, Double) In meal.Nutritionals
            NutritionalsDataGrid.Rows.Add(nutritional.Key, nutritional.Value.ToString())
        Next
        loader.Close()
    End Sub
End Class