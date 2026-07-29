Public Class WeekPlanner
    Private ReadOnly _allMeals As List(Of Meal)
    Private _currentPlan As WeeklyPlan
    Private _updatingPlannedMealTypes As Boolean

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
        InitializePlannedMealTypes()
        AddHandler PlannedMealTypesCheckedListBox.ItemCheck,
            AddressOf PlannedMealTypesCheckedListBox_ItemCheck
        _allMeals = MealRepository.LoadAll()
        PopulateRecipeChoices()
        UpdateGenerationModeCopy()
        LoadSavedPlan()
    End Sub

    Private Sub InitializePlannedMealTypes()
        _updatingPlannedMealTypes = True
        Try
            For index As Integer = 0 To PlannedMealTypesCheckedListBox.Items.Count - 1
                PlannedMealTypesCheckedListBox.SetItemChecked(index, True)
            Next
        Finally
            _updatingPlannedMealTypes = False
        End Try
        UpdatePlanColumns(WeekPlanGenerator.MealTypes)
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
        Dim plannedMealTypes = GetPlanMealTypes(savedPlan)
        If Not IsSavedPlanCompatible(savedPlan, plannedMealTypes) Then
            StatusLabel.Text =
                "Recipes or categories changed since the saved plan. Select recipes and generate it again."
            Return
        End If

        _currentPlan = savedPlan
        RestoreSavedGenerationMode(savedPlan)
        RestoreSavedMealTypes(plannedMealTypes)
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

    Private Function IsSavedPlanCompatible(
        plan As WeeklyPlan,
        plannedMealTypes As List(Of String)
    ) As Boolean
        If plannedMealTypes.Count = 0 Then Return False
        Dim expectedMealTypes As New HashSet(Of String)(
            plannedMealTypes,
            StringComparer.OrdinalIgnoreCase
        )

        For Each day In plan.Days
            If day Is Nothing OrElse day.Meals Is Nothing OrElse
                day.Meals.Count <> plannedMealTypes.Count Then
                Return False
            End If
            If day.Meals.Any(Function(meal) meal Is Nothing) OrElse
                day.Meals.Select(Function(meal) meal.MealType).
                    Distinct(StringComparer.OrdinalIgnoreCase).
                    Count() <> plannedMealTypes.Count OrElse
                day.Meals.Any(
                    Function(meal) Not expectedMealTypes.Contains(meal.MealType)
                ) Then
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

    Private Shared Function GetPlanMealTypes(
        plan As WeeklyPlan
    ) As List(Of String)
        Dim savedMealTypes = If(
            plan?.PlannedMealTypes,
            New List(Of String)
        )
        If savedMealTypes.Count = 0 AndAlso
            plan?.Days IsNot Nothing AndAlso
            plan.Days.Count > 0 AndAlso
            plan.Days(0)?.Meals IsNot Nothing Then
            savedMealTypes = plan.Days(0).Meals.
                Where(Function(meal) meal IsNot Nothing).
                Select(Function(meal) meal.MealType).
                ToList()
        End If

        Dim normalized = WeekPlanGenerator.MealTypes.Where(
            Function(optionName) savedMealTypes.Any(
                Function(value) String.Equals(
                    value,
                    optionName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        ).ToList()
        If normalized.Count = 0 Then
            Return New List(Of String)(WeekPlanGenerator.MealTypes)
        End If
        Return normalized
    End Function

    Private Sub RestoreSavedMealTypes(plannedMealTypes As List(Of String))
        Dim selected As New HashSet(Of String)(
            plannedMealTypes,
            StringComparer.OrdinalIgnoreCase
        )
        _updatingPlannedMealTypes = True
        Try
            For index As Integer = 0 To PlannedMealTypesCheckedListBox.Items.Count - 1
                PlannedMealTypesCheckedListBox.SetItemChecked(
                    index,
                    selected.Contains(
                        PlannedMealTypesCheckedListBox.Items(index).ToString()
                    )
                )
            Next
        Finally
            _updatingPlannedMealTypes = False
        End Try
        UpdatePlanColumns(plannedMealTypes)
    End Sub

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
        Dim plannedMealTypes = GetSelectedPlannedMealTypes()

        Try
            Dim nutrientInfo As New NutrientInfo()
            _currentPlan = WeekPlanGenerator.Generate(
                selectedMeals,
                _allMeals,
                generationMode,
                plannedMealTypes,
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

    Private Function GetSelectedPlannedMealTypes() As List(Of String)
        Dim selected As New HashSet(Of String)(
            PlannedMealTypesCheckedListBox.CheckedItems.
                Cast(Of Object)().
                Select(Function(item) item.ToString()),
            StringComparer.OrdinalIgnoreCase
        )
        Return WeekPlanGenerator.MealTypes.Where(
            Function(mealType) selected.Contains(mealType)
        ).ToList()
    End Function

    Private Sub PlannedMealTypesCheckedListBox_ItemCheck(
        sender As Object,
        e As ItemCheckEventArgs
    )
        If _updatingPlannedMealTypes OrElse IsDisposed Then Return
        BeginInvoke(
            New Action(
                Sub()
                    If IsDisposed Then Return
                    UpdatePlanColumns(GetSelectedPlannedMealTypes())
                    SetReadyStatus()
                End Sub
            )
        )
    End Sub

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
                "Choose meals and any must-have recipes." &
                Environment.NewLine &
                "Then generate or shuffle."
        Else
            StatusLabel.Text =
                "Cover every checked meal type." &
                Environment.NewLine &
                "Then generate or shuffle."
        End If
    End Sub

    Private Sub DisplayPlan(plan As WeeklyPlan)
        PlanDataGrid.Rows.Clear()
        SummaryDataGrid.Rows.Clear()
        Dim plannedMealTypes = GetPlanMealTypes(plan)
        UpdatePlanColumns(plannedMealTypes)

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
                GetMealName(mealsByType, "Brunch"),
                GetMealName(mealsByType, "Lunch"),
                GetMealName(mealsByType, "Dinner"),
                GetMealName(mealsByType, "Snack"),
                totalCalories.ToString("N0"),
                (coverage * 100).ToString("0") & "%"
            )
            Dim row = PlanDataGrid.Rows(rowIndex)
            row.Height = 48
            For Each mealType In plannedMealTypes
                If mealsByType.ContainsKey(mealType) Then
                    row.Cells(GetMealTypeColumn(mealType).Index).Tag =
                        mealsByType(mealType)
                End If
            Next
        Next

        PopulateWeeklySummary(plan)
        BalanceLabel.Text = CreateBalanceDescription(
            calorieTotals,
            nutrientCoverage
        )
    End Sub

    Private Sub UpdatePlanColumns(plannedMealTypes As IEnumerable(Of String))
        Dim visibleMealTypes As New HashSet(Of String)(
            If(plannedMealTypes, Enumerable.Empty(Of String)),
            StringComparer.OrdinalIgnoreCase
        )
        For Each mealType In WeekPlanGenerator.MealTypes
            GetMealTypeColumn(mealType).Visible =
                visibleMealTypes.Contains(mealType)
        Next
    End Sub

    Private Function GetMealTypeColumn(
        mealType As String
    ) As DataGridViewColumn
        Select Case mealType
            Case "Breakfast"
                Return BreakfastColumn
            Case "Brunch"
                Return BrunchColumn
            Case "Lunch"
                Return LunchColumn
            Case "Dinner"
                Return DinnerColumn
            Case "Snack"
                Return SnackColumn
            Case Else
                Throw New ArgumentOutOfRangeException(
                    NameOf(mealType),
                    mealType,
                    "Unknown meal type."
                )
        End Select
    End Function

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
