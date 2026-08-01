Public Class AddRecipe

    Private _ingredientsWereScraped As Boolean

    Public Sub New()
        InitializeComponent()
        ApplyAppIcon(Me)
    End Sub

    Private Sub AddRecipe_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each nutritional As String In AllNutritionals
            NutritionalsDataGrid.Rows.Add(nutritional, "")
        Next
        IngredientUnitColumn.Items.Clear()
        IngredientUnitColumn.Items.AddRange(
            IngredientMeasurementConverter.SupportedUnits.
                Cast(Of Object)().
                ToArray()
        )
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
        IngredientsDataGrid.EndEdit()
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

        Dim prepTime As Integer
        If Not TryParseOptionalMinutes(PrepTimeTextBox.Text, prepTime) Then
            MessageBox.Show("Prep time must be a non-negative whole number of minutes.")
            Return
        End If
        Dim cookTime As Integer
        If Not TryParseOptionalMinutes(CookTimeTextBox.Text, cookTime) Then
            MessageBox.Show("Cook time must be a non-negative whole number of minutes.")
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
            Dim ingredientDetails = Convert.ToString(row.Cells(1).Value).Trim()
            Dim minimumText = Convert.ToString(row.Cells(2).Value).Trim()
            Dim maximumText = Convert.ToString(row.Cells(3).Value).Trim()
            Dim unitText = Convert.ToString(row.Cells(4).Value).Trim()
            If ingredientName = "" AndAlso
                ingredientDetails = "" AndAlso
                minimumText = "" AndAlso
                maximumText = "" AndAlso
                unitText = "" Then
                Continue For
            End If
            If ingredientName = "" Then
                MessageBox.Show("Each ingredient measurement needs an ingredient name.")
                Return
            End If

            Dim unit = IngredientMeasurementConverter.NormalizeUnit(unitText)
            If unitText <> "" AndAlso unit = "" Then
                MessageBox.Show(
                    "'" &
                    unitText &
                    "' is not a supported ingredient measurement."
                )
                Return
            End If
            If unit = "" Then
                unit = If(minimumText <> "", "piece", "none")
            End If

            Dim minimum As Double? = Nothing
            Dim maximum As Double? = Nothing
            If unit <> "to taste" AndAlso unit <> "none" Then
                Dim parsedMinimum As Double
                Dim parsedMaximum As Double
                If minimumText = "" OrElse
                    Not IngredientMeasurementConverter.TryParseQuantity(
                        minimumText,
                        parsedMinimum
                    ) Then
                    MessageBox.Show(
                        "Each measured ingredient needs a non-negative minimum amount."
                    )
                    Return
                End If
                If maximumText = "" Then maximumText = minimumText
                If Not IngredientMeasurementConverter.TryParseQuantity(
                    maximumText,
                    parsedMaximum
                ) OrElse parsedMaximum < parsedMinimum Then
                    MessageBox.Show(
                        "The maximum ingredient amount must be at least the minimum amount."
                    )
                    Return
                End If
                minimum = parsedMinimum
                maximum = parsedMaximum
            ElseIf minimumText <> "" OrElse maximumText <> "" Then
                MessageBox.Show(
                    "Leave both amount fields empty for 'to taste' or 'none'."
                )
                Return
            End If

            Dim sourceAmount As String
            Dim originalMeasurement = unitText
            If unit = "to taste" Then
                sourceAmount = "to taste"
            ElseIf unit = "none" Then
                sourceAmount = "as needed"
            ElseIf Math.Abs(minimum.Value - maximum.Value) < 0.000000001 Then
                sourceAmount = minimumText
            Else
                sourceAmount = minimumText & "-" & maximumText
            End If

            Dim scrapedIngredient = TryCast(row.Tag, RecipeIngredient)
            If scrapedIngredient IsNot Nothing AndAlso
                String.Equals(
                    scrapedIngredient.Measurement,
                    unit,
                    StringComparison.OrdinalIgnoreCase
                ) Then
                originalMeasurement = scrapedIngredient.OriginalMeasurement
                If AmountsMatch(
                    scrapedIngredient.MinAmount,
                    minimum
                ) AndAlso AmountsMatch(
                    scrapedIngredient.MaxAmount,
                    maximum
                ) Then
                    sourceAmount = scrapedIngredient.Amount
                End If
            End If
            ingredients.Add(
                New RecipeIngredient(
                    ingredientName,
                    amount:=sourceAmount,
                    details:=ingredientDetails,
                    minAmount:=minimum,
                    maxAmount:=maximum,
                    measurement:=unit,
                    originalMeasurement:=originalMeasurement
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
            prepTime,
            cookTime,
            mealTypes,
            ingredients,
            PreparationMethodTextBox.Text,
            NotesTextBox.Text,
            advancedScrapeVersion:=Meal.CurrentAdvancedScrapeVersion,
            ingredientDataVersion:=If(
                _ingredientsWereScraped,
                Meal.CurrentIngredientDataVersion,
                Math.Max(0, Meal.CurrentIngredientDataVersion - 1)
            )
        )
        Dim addResult = MealRepository.AddIfMissing(meal)
        If Not addResult.Added Then
            MessageBox.Show(
                "That recipe is already in DietPlanner as '" &
                addResult.Meal.Name &
                "'.",
                "Recipe already added",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )
            Return
        End If
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
                Dim ingredientRowIndex = IngredientsDataGrid.Rows.Add(
                    ingredient.Ingredient,
                    ingredient.Details,
                    If(
                        ingredient.MinAmount.HasValue,
                        ingredient.MinAmount.Value.ToString(
                            Globalization.CultureInfo.CurrentCulture
                        ),
                        String.Empty
                    ),
                    If(
                        ingredient.MaxAmount.HasValue,
                        ingredient.MaxAmount.Value.ToString(
                            Globalization.CultureInfo.CurrentCulture
                        ),
                        String.Empty
                    ),
                    ingredient.Measurement
                )
                IngredientsDataGrid.Rows(ingredientRowIndex).Tag = ingredient
            Next
            _ingredientsWereScraped = True
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

    Private Shared Function AmountsMatch(
        first As Double?,
        second As Double?
    ) As Boolean
        If first.HasValue <> second.HasValue Then Return False
        If Not first.HasValue Then Return True
        Return Math.Abs(first.Value - second.Value) < 0.000000001
    End Function

    Private Shared Function TryParseOptionalMinutes(
        value As String,
        ByRef minutes As Integer
    ) As Boolean
        minutes = 0
        Dim text = If(value, String.Empty).Trim()
        Return text = String.Empty OrElse
            (Integer.TryParse(text, minutes) AndAlso minutes >= 0)
    End Function
End Class
