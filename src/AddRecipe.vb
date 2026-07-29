Public Class AddRecipe

    Public Sub New()
        InitializeComponent()
        ApplyAppIcon(Me)
    End Sub

    Private Sub AddRecipe_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each nutritional As String In AllNutritionals
            NutritionalsDataGrid.Rows.Add(nutritional, "")
        Next
    End Sub

    Private Sub DataGridView_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles NutritionalsDataGrid.CellFormatting
        If e.RowIndex >= 0 AndAlso e.ColumnIndex = 1 Then
            Dim amount As Double
            If Global.DietPlanner.Nutrition.TryParseAmount(e.Value, amount) Then
                Dim nutrient = NutritionalsDataGrid.Rows(e.RowIndex).Cells(0).Value.ToString()
                Dim nutrition = New Nutrition(nutrient, amount)
                e.Value = nutrition.FormattedAmount
                e.FormattingApplied = True
            End If
        End If
    End Sub

    Private Sub NutritionalsDataGrid_CellParsing(
        sender As Object,
        e As DataGridViewCellParsingEventArgs
    ) Handles NutritionalsDataGrid.CellParsing
        If e.RowIndex < 0 OrElse e.ColumnIndex <> 1 Then Return

        Dim amount As Double
        If Global.DietPlanner.Nutrition.TryParseAmount(e.Value, amount) Then
            e.Value = amount
            e.ParsingApplied = True
        End If
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As EventArgs) Handles SaveButton.Click
        NutritionalsDataGrid.EndEdit()
        Dim nutritionals As New Dictionary(Of String, Double)

        'Check that all data is entered
        If NameTextBox.Text = "" Or
            CaloriesTextBox.Text = "" Or
            ServingsTextBox.Text = "" Or
            RecipeTextBox.Text = "" Then
            MessageBox.Show("Please fill in all fields")
            Return
        End If

        Dim caloriesPerServing As Integer
        If Not Integer.TryParse(CaloriesTextBox.Text, caloriesPerServing) OrElse
            caloriesPerServing < 0 Then
            MessageBox.Show("Calories per serving must be a non-negative whole number.")
            Return
        End If

        Dim servings As Integer
        If Not Integer.TryParse(ServingsTextBox.Text, servings) OrElse servings < 1 Then
            MessageBox.Show("Servings must be a positive whole number.")
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
                If row.Cells(1).Value Is Nothing OrElse
                    String.IsNullOrWhiteSpace(Convert.ToString(row.Cells(1).Value)) Then
                    value = 0
                Else
                    If Not Global.DietPlanner.Nutrition.TryParseAmount(
                        row.Cells(1).Value,
                        value
                    ) Then
                        MessageBox.Show("Nutritional value must be a number")
                        Return
                    End If
                End If
                nutritionals.Add(row.Cells(0).Value, value)
            End If
        Next

        Dim mealTypes As New List(Of String)
        For Each selectedMealType In MealTypeCheckedListBox.CheckedItems
            mealTypes.Add(selectedMealType.ToString())
        Next
        If mealTypes.Count = 0 Then
            MessageBox.Show("Please select at least one meal type.")
            Return
        End If

        Dim ingredients As New List(Of RecipeIngredient)
        For Each row As DataGridViewRow In IngredientsDataGrid.Rows
            If row.IsNewRow Then Continue For

            Dim ingredientName = Convert.ToString(row.Cells(0).Value).Trim()
            Dim quantityText = Convert.ToString(row.Cells(1).Value).Trim()
            Dim unitText = Convert.ToString(row.Cells(2).Value).Trim()
            If ingredientName = "" AndAlso
                quantityText = "" AndAlso
                unitText = "" Then
                Continue For
            End If
            If ingredientName = "" Then
                MessageBox.Show("Each ingredient measurement needs an ingredient name.")
                Return
            End If

            Dim quantity As Double = 0
            If quantityText <> "" AndAlso
                Not IngredientMeasurementConverter.TryParseQuantity(
                    quantityText,
                    quantity
                ) Then
                MessageBox.Show(
                    "Ingredient quantities must be non-negative numbers."
                )
                Return
            End If

            Dim unit = IngredientMeasurementConverter.NormalizeUnit(unitText)
            If unitText <> "" AndAlso unit = "" Then
                MessageBox.Show(
                    "'" &
                    unitText &
                    "' is not a supported ingredient unit." &
                    Environment.NewLine &
                    "Use a standard unit such as cup, tablespoon, gram, ounce, piece, or to taste."
                )
                Return
            End If
            If unit = "" Then
                unit = If(quantity > 0, "piece", "none")
            End If
            ingredients.Add(
                New RecipeIngredient(
                    ingredientName,
                    quantity:=quantity,
                    unit:=unit
                )
            )
        Next

        'Store meal
        Dim meal As New Meal(
            NameTextBox.Text,
            caloriesPerServing,
            nutritionals,
            RecipeTextBox.Text,
            servings,
            PrepTimeTextBox.Text,
            CookTimeTextBox.Text,
            mealTypes,
            ingredients,
            PreparationMethodTextBox.Text,
            NotesTextBox.Text,
            advancedScrapeVersion:=Meal.CurrentAdvancedScrapeVersion,
            ingredientDataVersion:=Meal.CurrentIngredientDataVersion
        )
        Dim meals = MealRepository.LoadAll()
        meals.Add(meal)
        MealRepository.SaveAll(meals)
        Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Close()
    End Sub

    Private Async Sub ScrapeButton_Click(sender As Object, e As EventArgs) Handles ScrapeButton.Click
        Dim loader = New Loading("Extracting recipe details with Codex...")
        loader.Show()
        ScrapeButton.Enabled = False

        Try
            Dim meal = Await API.ScrapeNutritionals(RecipeTextBox.Text)

            NameTextBox.Text = meal.Name
            CaloriesTextBox.Text = meal.Calory
            ServingsTextBox.Text = meal.Servings
            PrepTimeTextBox.Text = meal.PrepTime
            CookTimeTextBox.Text = meal.CookTime
            For index As Integer = 0 To MealTypeCheckedListBox.Items.Count - 1
                Dim displayedMealType = MealTypeCheckedListBox.Items(index).ToString()
                MealTypeCheckedListBox.SetItemChecked(
                    index,
                    meal.MealTypes.Any(
                        Function(value) String.Equals(
                            value,
                            displayedMealType,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
            Next

            'Clear all grid rows then add the new ones
            NutritionalsDataGrid.Rows.Clear()
            For Each nutritional As KeyValuePair(Of String, Double) In meal.Nutritionals
                NutritionalsDataGrid.Rows.Add(nutritional.Key, nutritional.Value.ToString())
            Next

            IngredientsDataGrid.Rows.Clear()
            For Each ingredient In meal.Ingredients
                IngredientsDataGrid.Rows.Add(
                    ingredient.Ingredient,
                    If(
                        ingredient.Quantity.HasValue,
                        ingredient.Quantity.Value.ToString(
                            Globalization.CultureInfo.CurrentCulture
                        ),
                        String.Empty
                    ),
                    ingredient.Unit
                )
            Next
            PreparationMethodTextBox.Text = meal.PreparationMethod
            NotesTextBox.Text = meal.Notes
        Catch ex As Exception
            MessageBox.Show(
                "Could not extract recipe details." & Environment.NewLine & Environment.NewLine & ex.Message,
                "DietPlanner",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        Finally
            ScrapeButton.Enabled = True
            loader.Close()
        End Try
    End Sub
End Class
