Public Class WeekPlanner
    Private ReadOnly _allMeals As List(Of Meal)
    Private _currentPlan As WeeklyPlan

    Private Class RecipeChoice
        Public ReadOnly Property Meal As Meal

        Public Sub New(meal As Meal)
            Me.Meal = meal
        End Sub

        Public Overrides Function ToString() As String
            Dim categories = If(
                Meal.MealTypes Is Nothing,
                String.Empty,
                String.Join(", ", Meal.MealTypes)
            )
            Return Meal.Name & If(categories = String.Empty, "", " — " & categories)
        End Function
    End Class

    Public Sub New()
        InitializeComponent()
        ApplyAppIcon(Me)
        _allMeals = MealRepository.LoadAll()
        PopulateRecipeChoices()
        UpdateGenerationModeCopy()
        LoadSavedPlan()
    End Sub

    Private Sub PopulateRecipeChoices()
        SelectedRecipesCheckedListBox.Items.Clear()
        For Each meal In _allMeals
            SelectedRecipesCheckedListBox.Items.Add(New RecipeChoice(meal), False)
        Next
    End Sub

    Private Sub LoadSavedPlan()
        Dim savedPlan = WeekPlanRepository.Load()
        If savedPlan Is Nothing OrElse savedPlan.Days Is Nothing OrElse
            savedPlan.Days.Count <> WeekPlanGenerator.DayNames.Length Then
            SetReadyStatus()
            Return
        End If
        If Not IsSavedPlanCompatible(savedPlan) Then
            StatusLabel.Text =
                "Recipes or categories changed since the saved plan. Select recipes and generate it again."
            Return
        End If

        _currentPlan = savedPlan
        RestoreSavedGenerationMode(savedPlan)
        RestoreSavedSelection(savedPlan)
        DisplayPlan(savedPlan)
        StatusLabel.Text =
            "Loaded the plan generated " &
            savedPlan.GeneratedAt.ToString("g") &
            "."
    End Sub

    Private Sub RestoreSavedGenerationMode(plan As WeeklyPlan)
        FullCatalogRadioButton.Checked = String.Equals(
            plan.GenerationMode,
            WeekPlanGenerationMode.FullCatalogWithGuarantees.ToString(),
            StringComparison.OrdinalIgnoreCase
        )
        SelectedOnlyRadioButton.Checked = Not FullCatalogRadioButton.Checked
        UpdateGenerationModeCopy()
    End Sub

    Private Function IsSavedPlanCompatible(plan As WeeklyPlan) As Boolean
        For Each day In plan.Days
            If day Is Nothing OrElse day.Meals Is Nothing OrElse
                day.Meals.Count <> WeekPlanGenerator.MealTypes.Length Then
                Return False
            End If
            If day.Meals.Any(Function(meal) meal Is Nothing) OrElse
                day.Meals.Select(Function(meal) meal.MealType).
                    Distinct(StringComparer.OrdinalIgnoreCase).
                    Count() <> WeekPlanGenerator.MealTypes.Length Then
                Return False
            End If

            For Each plannedMeal In day.Meals
                Dim currentMeal = FindCurrentMeal(plannedMeal)
                If currentMeal Is Nothing OrElse currentMeal.MealTypes Is Nothing OrElse
                    Not currentMeal.MealTypes.Any(
                        Function(mealType) String.Equals(
                            mealType,
                            plannedMeal.MealType,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ) Then
                    Return False
                End If
            Next
        Next

        Return True
    End Function

    Private Sub RestoreSavedSelection(plan As WeeklyPlan)
        Dim selectedUrls As New HashSet(Of String)(
            If(plan.SelectedRecipeUrls, New List(Of String)),
            StringComparer.OrdinalIgnoreCase
        )
        Dim selectedNames As New HashSet(Of String)(
            If(plan.SelectedRecipeNames, New List(Of String)),
            StringComparer.CurrentCultureIgnoreCase
        )

        For index As Integer = 0 To SelectedRecipesCheckedListBox.Items.Count - 1
            Dim choice = DirectCast(
                SelectedRecipesCheckedListBox.Items(index),
                RecipeChoice
            )
            Dim selected =
                (
                    Not String.IsNullOrWhiteSpace(choice.Meal.Recipe) AndAlso
                    selectedUrls.Contains(choice.Meal.Recipe)
                ) OrElse selectedNames.Contains(choice.Meal.Name)
            SelectedRecipesCheckedListBox.SetItemChecked(index, selected)
        Next
    End Sub

    Private Sub SelectAllButton_Click(sender As Object, e As EventArgs) Handles SelectAllButton.Click
        For index As Integer = 0 To SelectedRecipesCheckedListBox.Items.Count - 1
            SelectedRecipesCheckedListBox.SetItemChecked(index, True)
        Next
    End Sub

    Private Sub ClearSelectionButton_Click(
        sender As Object,
        e As EventArgs
    ) Handles ClearSelectionButton.Click
        For index As Integer = 0 To SelectedRecipesCheckedListBox.Items.Count - 1
            SelectedRecipesCheckedListBox.SetItemChecked(index, False)
        Next
    End Sub

    Private Sub GenerateButton_Click(sender As Object, e As EventArgs) Handles GenerateButton.Click
        Dim selectedMeals = SelectedRecipesCheckedListBox.CheckedItems.
            Cast(Of RecipeChoice)().
            Select(Function(choice) choice.Meal).
            ToList()
        Dim generationMode = GetGenerationMode()

        Try
            Dim nutrientInfo As New NutrientInfo()
            _currentPlan = WeekPlanGenerator.Generate(
                selectedMeals,
                _allMeals,
                generationMode,
                nutrientInfo.RecommendedDailyIntakes
            )
            WeekPlanRepository.Save(_currentPlan)
            DisplayPlan(_currentPlan)
            If generationMode =
                WeekPlanGenerationMode.FullCatalogWithGuarantees Then
                StatusLabel.Text =
                    "Shuffled from the full catalog with checked recipes guaranteed; saved at " &
                    _currentPlan.GeneratedAt.ToString("t") &
                    "."
            Else
                StatusLabel.Text =
                    "Shuffled using only checked recipes; saved at " &
                    _currentPlan.GeneratedAt.ToString("t") &
                    "."
            End If
        Catch ex As WeeklyPlanException
            MessageBox.Show(
                ex.Message,
                "Plan my week",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )
        Catch ex As Exception
            MessageBox.Show(
                "DietPlanner could not generate the week." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "Plan my week",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub

    Private Function GetGenerationMode() As WeekPlanGenerationMode
        If FullCatalogRadioButton.Checked Then
            Return WeekPlanGenerationMode.FullCatalogWithGuarantees
        End If
        Return WeekPlanGenerationMode.SelectedRecipesOnly
    End Function

    Private Sub GenerationMode_CheckedChanged(
        sender As Object,
        e As EventArgs
    ) Handles SelectedOnlyRadioButton.CheckedChanged,
        FullCatalogRadioButton.CheckedChanged
        If Not DirectCast(sender, RadioButton).Checked Then Return
        UpdateGenerationModeCopy()
        SetReadyStatus()
    End Sub

    Private Sub UpdateGenerationModeCopy()
        If FullCatalogRadioButton.Checked Then
            GenerationModeHelpLabel.Text =
                "Checked recipes are guaranteed; unchecked recipes can fill the remaining slots."
            SelectedRecipesLabel.Text =
                "Guaranteed recipes (optional; at least once)"
        Else
            GenerationModeHelpLabel.Text =
                "The plan uses only checked recipes; each one appears at least once."
            SelectedRecipesLabel.Text =
                "Recipes to use (each appears at least once)"
        End If
    End Sub

    Private Sub SetReadyStatus()
        If StatusLabel Is Nothing Then Return
        If FullCatalogRadioButton IsNot Nothing AndAlso
            FullCatalogRadioButton.Checked Then
            StatusLabel.Text =
                "Check any must-have recipes, then generate or shuffle a full-catalog plan."
        Else
            StatusLabel.Text =
                "Select recipes for every meal type, then generate or shuffle the week."
        End If
    End Sub

    Private Sub DisplayPlan(plan As WeeklyPlan)
        PlanDataGrid.Rows.Clear()
        SummaryDataGrid.Rows.Clear()

        Dim calorieTotals As New List(Of Double)
        Dim nutrientCoverage As New List(Of Double)

        For Each day In plan.Days
            Dim mealsByType = day.Meals.ToDictionary(
                Function(meal) meal.MealType,
                StringComparer.OrdinalIgnoreCase
            )
            Dim totalCalories = day.Meals.Sum(Function(meal) meal.Calories)
            Dim coverage = CalculateDailyCoverage(day, plan.TargetDailyIntakes)
            calorieTotals.Add(totalCalories)
            nutrientCoverage.Add(coverage)

            Dim rowIndex = PlanDataGrid.Rows.Add(
                day.Name,
                GetMealName(mealsByType, "Breakfast"),
                GetMealName(mealsByType, "Lunch"),
                GetMealName(mealsByType, "Brunch"),
                GetMealName(mealsByType, "Dinner"),
                GetMealName(mealsByType, "Snack"),
                totalCalories.ToString("N0"),
                (coverage * 100).ToString("0") & "%"
            )
            Dim row = PlanDataGrid.Rows(rowIndex)
            row.Height = 48
            For mealTypeIndex As Integer = 0 To WeekPlanGenerator.MealTypes.Length - 1
                Dim mealType = WeekPlanGenerator.MealTypes(mealTypeIndex)
                If mealsByType.ContainsKey(mealType) Then
                    row.Cells(mealTypeIndex + 1).Tag = mealsByType(mealType)
                End If
            Next
        Next

        PopulateWeeklySummary(plan)
        BalanceLabel.Text = CreateBalanceDescription(
            calorieTotals,
            nutrientCoverage
        )
    End Sub

    Private Shared Function GetMealName(
        mealsByType As Dictionary(Of String, PlannedMeal),
        mealType As String
    ) As String
        If mealsByType.ContainsKey(mealType) Then
            Return mealsByType(mealType).MealName
        End If
        Return "—"
    End Function

    Private Shared Function CalculateDailyCoverage(
        day As PlannedDay,
        targets As IDictionary(Of String, Double)
    ) As Double
        If targets Is Nothing OrElse targets.Count = 0 Then Return 0

        Dim ratios As New List(Of Double)
        For Each target In targets
            If target.Value <= 0 Then Continue For
            Dim total = day.Meals.Sum(
                Function(meal) GetPlannedNutrientAmount(meal, target.Key)
            )
            ratios.Add(total / target.Value)
        Next
        If ratios.Count = 0 Then Return 0
        Return ratios.Average()
    End Function

    Private Sub PopulateWeeklySummary(plan As WeeklyPlan)
        If plan.TargetDailyIntakes Is Nothing Then Return

        For Each target In plan.TargetDailyIntakes.OrderBy(
            Function(item) item.Key,
            StringComparer.CurrentCultureIgnoreCase
        )
            Dim weeklyTotal = plan.Days.Sum(
                Function(day) day.Meals.Sum(
                    Function(meal) GetPlannedNutrientAmount(meal, target.Key)
                )
            )
            Dim weeklyTarget = target.Value * WeekPlanGenerator.DayNames.Length
            Dim percent = If(
                weeklyTarget <= 0,
                0,
                weeklyTotal / weeklyTarget * 100
            )
            SummaryDataGrid.Rows.Add(
                target.Key,
                New Nutrition(target.Key, Math.Round(weeklyTotal, 2)).FormattedAmount(),
                New Nutrition(target.Key, Math.Round(weeklyTarget, 2)).FormattedAmount(),
                percent.ToString("0.0") & "%"
            )
        Next
    End Sub

    Private Shared Function GetPlannedNutrientAmount(
        meal As PlannedMeal,
        nutrientName As String
    ) As Double
        If meal.Nutritionals Is Nothing Then Return 0
        For Each item In meal.Nutritionals
            If String.Equals(
                item.Key,
                nutrientName,
                StringComparison.OrdinalIgnoreCase
            ) Then
                Return item.Value
            End If
        Next
        Return 0
    End Function

    Private Shared Function CreateBalanceDescription(
        calorieTotals As List(Of Double),
        nutrientCoverage As List(Of Double)
    ) As String
        If calorieTotals.Count = 0 Then Return "Generate a plan to see its balance."

        Dim averageCalories = calorieTotals.Average()
        Dim calorieDeviation = MaximumDeviationFraction(
            calorieTotals,
            averageCalories
        )
        Dim averageCoverage = If(
            nutrientCoverage.Count = 0,
            0,
            nutrientCoverage.Average()
        )
        Dim coverageDeviation = MaximumDeviationFraction(
            nutrientCoverage,
            averageCoverage
        )
        Dim withinGoal = calorieDeviation <= 0.15 AndAlso coverageDeviation <= 0.2
        Dim prefix = If(
            withinGoal,
            "Balanced within the day-to-day variance goal.",
            "Best balance available from the chosen recipe pool."
        )

        Return prefix &
            " Calories: " &
            calorieTotals.Min().ToString("N0") &
            "–" &
            calorieTotals.Max().ToString("N0") &
            "/day (max " &
            (calorieDeviation * 100).ToString("0.0") &
            "% from average). Nutrient coverage: " &
            (nutrientCoverage.Min() * 100).ToString("0") &
            "–" &
            (nutrientCoverage.Max() * 100).ToString("0") &
            "%."
    End Function

    Private Shared Function MaximumDeviationFraction(
        values As IEnumerable(Of Double),
        average As Double
    ) As Double
        If Math.Abs(average) < 0.000001 Then Return 0
        Return values.Max(
            Function(value) Math.Abs(value - average) / average
        )
    End Function

    Private Sub PlanDataGrid_CellDoubleClick(
        sender As Object,
        e As DataGridViewCellEventArgs
    ) Handles PlanDataGrid.CellDoubleClick
        If e.RowIndex < 0 OrElse e.ColumnIndex < 1 OrElse e.ColumnIndex > 5 Then Return
        Dim plannedMeal = TryCast(
            PlanDataGrid.Rows(e.RowIndex).Cells(e.ColumnIndex).Tag,
            PlannedMeal
        )
        If plannedMeal Is Nothing Then Return

        Dim meal = FindCurrentMeal(plannedMeal)
        If meal Is Nothing Then Return

        Using details As New RecipeView(meal)
            details.ShowDialog(Me)
        End Using
    End Sub

    Private Function FindCurrentMeal(plannedMeal As PlannedMeal) As Meal
        If plannedMeal Is Nothing Then Return Nothing
        Return _allMeals.FirstOrDefault(
            Function(candidate)
                Return (
                    Not String.IsNullOrWhiteSpace(plannedMeal.RecipeUrl) AndAlso
                    String.Equals(
                        candidate.Recipe,
                        plannedMeal.RecipeUrl,
                        StringComparison.OrdinalIgnoreCase
                    )
                ) OrElse String.Equals(
                    candidate.Name,
                    plannedMeal.MealName,
                    StringComparison.CurrentCultureIgnoreCase
                )
            End Function
        )
    End Function

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        Close()
    End Sub
End Class
