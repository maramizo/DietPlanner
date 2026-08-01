Public Class RecipeView
    Private ReadOnly _meal As Meal
    Private _updatingIngredientMeasurements As Boolean

    Public Sub New(meal As Meal)
        _meal = meal
        InitializeComponent()
        ApplyAppIcon(Me)
        NotifyIcon1.Icon = DirectCast(Icon.Clone(), Icon)
        Label1.Text = meal.Name
        For Each nutritional As KeyValuePair(Of String, String) In meal.ViewNutritionals()
            DataGridView1.Rows.Add(nutritional.Key, nutritional.Value)
        Next
        DataGridView1.AllowUserToAddRows = False
        PrepTime.Text = meal.PrepTime & " minutes"
        CookTime.Text = meal.CookTime & " minutes"
        TotalTime.Text = meal.TotalTime & " minutes"
        ServingsValueLabel.Text = If(
            meal.Servings > 0,
            meal.Servings.ToString(),
            "Unknown"
        )
        CaloriesPerServingValueLabel.Text = If(
            meal.Servings > 0,
            meal.Calory & " calories",
            "Unknown"
        )
        BatchCaloriesValueLabel.Text = If(
            meal.Servings > 0,
            (CLng(meal.Calory) * meal.Servings).ToString("N0") & " calories",
            "Unknown"
        )
        MealTypesLabel.Text = If(
            meal.MealTypes Is Nothing OrElse meal.MealTypes.Count = 0,
            "Not categorized",
            String.Join(", ", meal.MealTypes)
        )
        AdvancedDetailsStatusLabel.Text = If(
            meal.IsAdvancedScrapeUnavailable(),
            "Source unavailable",
            If(meal.NeedsAdvancedScrape(), "Pending", "Complete")
        )
        RefreshIngredientMeasurements()
        PreparationMethodTextBox.Text = If(
            String.IsNullOrWhiteSpace(meal.PreparationMethod),
            If(
                meal.IsAdvancedScrapeUnavailable(),
                "Source unavailable. Automatic extraction will not be retried.",
                "Not available"
            ),
            meal.PreparationMethod
        )
        NotesTextBox.Text = If(
            String.IsNullOrWhiteSpace(meal.Notes),
            If(
                meal.IsAdvancedScrapeUnavailable(),
                "Source unavailable. Automatic extraction will not be retried.",
                "No storage, freezing, or recipe variation notes were provided."
            ),
            meal.Notes
        )
    End Sub

    Public Sub RefreshIngredientMeasurements()
        _updatingIngredientMeasurements = True
        Try
            IngredientsDataGrid.Rows.Clear()
            If _meal?.Ingredients Is Nothing Then Return

            Dim measurementSystem =
                IngredientMeasurementConverter.NormalizeSystem(
                    AppSettingsRepository.Load().IngredientMeasurementSystem
                )
            For Each ingredient In _meal.Ingredients
                Dim rowIndex = IngredientsDataGrid.Rows.Add(
                    ingredient.Ingredient,
                    ingredient.Details,
                    String.Empty,
                    Nothing
                )
                Dim row = IngredientsDataGrid.Rows(rowIndex)
                row.Tag = ingredient
                Dim measurementCell = DirectCast(
                    row.Cells(IngredientMeasurementColumn.Index),
                    DataGridViewComboBoxCell
                )
                Dim compatibleMeasurements =
                    IngredientMeasurementConverter.GetCompatibleMeasurements(
                        ingredient.Measurement
                    )
                If compatibleMeasurements.Count = 0 Then
                    row.Cells(IngredientAmountColumn.Index).Value =
                        ingredient.Amount
                    measurementCell.ReadOnly = True
                    Continue For
                End If

                measurementCell.Items.AddRange(
                    compatibleMeasurements.Cast(Of Object)().ToArray()
                )
                Dim displayMeasurement =
                    IngredientMeasurementConverter.GetDefaultDisplayMeasurement(
                        ingredient,
                        measurementSystem
                    )
                measurementCell.Value = displayMeasurement
                row.Cells(IngredientAmountColumn.Index).Value =
                    IngredientMeasurementConverter.FormatAmountValue(
                        ingredient,
                        displayMeasurement
                    )
            Next
        Finally
            _updatingIngredientMeasurements = False
        End Try
    End Sub

    Private Sub IngredientsDataGrid_CurrentCellDirtyStateChanged(
        sender As Object,
        e As EventArgs
    ) Handles IngredientsDataGrid.CurrentCellDirtyStateChanged
        If IngredientsDataGrid.IsCurrentCellDirty AndAlso
            TypeOf IngredientsDataGrid.CurrentCell Is DataGridViewComboBoxCell Then
            IngredientsDataGrid.CommitEdit(
                DataGridViewDataErrorContexts.Commit
            )
        End If
    End Sub

    Private Sub IngredientsDataGrid_CellValueChanged(
        sender As Object,
        e As DataGridViewCellEventArgs
    ) Handles IngredientsDataGrid.CellValueChanged
        If _updatingIngredientMeasurements OrElse e.RowIndex < 0 OrElse
            e.ColumnIndex <> IngredientMeasurementColumn.Index Then
            Return
        End If

        Dim row = IngredientsDataGrid.Rows(e.RowIndex)
        Dim ingredient = TryCast(row.Tag, RecipeIngredient)
        If ingredient Is Nothing Then Return
        Dim measurement = Convert.ToString(
            row.Cells(IngredientMeasurementColumn.Index).Value
        )
        row.Cells(IngredientAmountColumn.Index).Value =
            IngredientMeasurementConverter.FormatAmountValue(
                ingredient,
                measurement
            )
    End Sub

    Private Sub IngredientsDataGrid_DataError(
        sender As Object,
        e As DataGridViewDataErrorEventArgs
    ) Handles IngredientsDataGrid.DataError
        e.ThrowException = False
    End Sub

    Private Sub RecipeView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        NotifyIcon1.BalloonTipText = "This is a recipe view"
        NotifyIcon1.BalloonTipTitle = "Recipe View"
        NotifyIcon1.ShowBalloonTip(1000)
    End Sub
End Class
