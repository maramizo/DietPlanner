Public Class WeekPlanner
    Private ReadOnly _allMeals As List(Of Meal)
    Private _currentPlan As WeeklyPlan
    Private _updatingPlannedMealTypes As Boolean
    Private _constraintTabs As TabControl
    Private _ingredientChoicesCheckedListBox As CheckedListBox
    Private _selectAllIngredientsButton As Button
    Private _clearIngredientsButton As Button
    Private _resultsTabs As TabControl
    Private _ingredientResultsPage As TabPage
    Private _planIngredientsDataGrid As DataGridView
    Private _updatingPlanIngredientMeasurements As Boolean

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

    Private Class IngredientChoice
        Public ReadOnly Property Key As String
        Public ReadOnly Property DisplayName As String

        Public Sub New(key As String, displayName As String)
            Me.Key = key
            Me.DisplayName = displayName
        End Sub

        Public Overrides Function ToString() As String
            Return DisplayName
        End Function
    End Class

    Public Sub New()
        InitializeComponent()
        ConfigureIngredientPlannerUi()
        ApplyAppIcon(Me)
        InitializePlannedMealTypes()
        AddHandler PlannedMealTypesCheckedListBox.ItemCheck,
            AddressOf PlannedMealTypesCheckedListBox_ItemCheck
        _allMeals = MealRepository.LoadAll()
        PopulateRecipeChoices()
        PopulateIngredientChoices()
        UpdateGenerationModeCopy()
        LoadSavedPlan()
    End Sub

    Private Sub ConfigureIngredientPlannerUi()
        ClientSize = New Size(1280, 780)
        MinimumSize = New Size(1050, 700)
        GenerateButton.Location = New Point(20, 686)
        StatusLabel.Location = New Point(20, 724)
        StatusLabel.Size = New Size(290, 46)
        CloseButton.Location = New Point(1135, 732)

        _constraintTabs = New TabControl With {
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left,
            .Location = New Point(20, 308),
            .Name = "PlanningConstraintsTabControl",
            .Size = New Size(290, 368),
            .TabIndex = 2
        }
        Dim recipesPage As New TabPage("Recipes") With {
            .Name = "RecipeConstraintsTabPage"
        }
        Dim ingredientsPage As New TabPage("Ingredients") With {
            .Name = "IngredientConstraintsTabPage"
        }
        _constraintTabs.TabPages.AddRange({recipesPage, ingredientsPage})

        recipesPage.Padding = New Padding(6)
        Dim recipeLayout As New TableLayoutPanel With {
            .ColumnCount = 1,
            .Dock = DockStyle.Fill,
            .RowCount = 3
        }
        recipeLayout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100)
        )
        recipeLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
        recipeLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        recipeLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        SelectedRecipesLabel.AutoSize = False
        SelectedRecipesLabel.Dock = DockStyle.Fill
        SelectedRecipesLabel.Margin = New Padding(0, 0, 0, 4)
        SelectedRecipesCheckedListBox.Dock = DockStyle.Fill
        SelectedRecipesCheckedListBox.Margin = New Padding(0)
        Dim recipeButtons As New TableLayoutPanel With {
            .ColumnCount = 2,
            .Dock = DockStyle.Fill,
            .RowCount = 1
        }
        recipeButtons.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 50)
        )
        recipeButtons.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 50)
        )
        SelectAllButton.Dock = DockStyle.Fill
        SelectAllButton.Margin = New Padding(0, 6, 4, 0)
        ClearSelectionButton.Dock = DockStyle.Fill
        ClearSelectionButton.Margin = New Padding(4, 6, 0, 0)
        recipeButtons.Controls.Add(SelectAllButton, 0, 0)
        recipeButtons.Controls.Add(ClearSelectionButton, 1, 0)
        recipeLayout.Controls.Add(SelectedRecipesLabel, 0, 0)
        recipeLayout.Controls.Add(SelectedRecipesCheckedListBox, 0, 1)
        recipeLayout.Controls.Add(recipeButtons, 0, 2)
        recipesPage.Controls.Add(recipeLayout)

        Dim ingredientHelpLabel As New Label With {
            .Dock = DockStyle.Fill,
            .Name = "IngredientConstraintHelpLabel",
            .Text = "Allowed ingredients — uncheck anything you do not want used."
        }
        _ingredientChoicesCheckedListBox = New CheckedListBox With {
            .CheckOnClick = True,
            .Dock = DockStyle.Fill,
            .FormattingEnabled = True,
            .HorizontalScrollbar = True,
            .Name = "IngredientChoicesCheckedListBox"
        }
        _selectAllIngredientsButton = New Button With {
            .Dock = DockStyle.Fill,
            .Name = "SelectAllIngredientsButton",
            .Text = "Select All",
            .UseVisualStyleBackColor = True
        }
        _clearIngredientsButton = New Button With {
            .Dock = DockStyle.Fill,
            .Name = "ClearIngredientsButton",
            .Text = "Clear Selection",
            .UseVisualStyleBackColor = True
        }
        AddHandler _selectAllIngredientsButton.Click,
            AddressOf SelectAllIngredientsButton_Click
        AddHandler _clearIngredientsButton.Click,
            AddressOf ClearIngredientsButton_Click
        ingredientsPage.Padding = New Padding(6)
        Dim ingredientConstraintLayout As New TableLayoutPanel With {
            .ColumnCount = 1,
            .Dock = DockStyle.Fill,
            .RowCount = 3
        }
        ingredientConstraintLayout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100)
        )
        ingredientConstraintLayout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 42)
        )
        ingredientConstraintLayout.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100)
        )
        ingredientConstraintLayout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 34)
        )
        ingredientHelpLabel.Margin = New Padding(0, 0, 0, 4)
        _ingredientChoicesCheckedListBox.Margin = New Padding(0)
        Dim ingredientButtons As New TableLayoutPanel With {
            .ColumnCount = 2,
            .Dock = DockStyle.Fill,
            .RowCount = 1
        }
        ingredientButtons.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 50)
        )
        ingredientButtons.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 50)
        )
        _selectAllIngredientsButton.Margin = New Padding(0, 6, 4, 0)
        _clearIngredientsButton.Margin = New Padding(4, 6, 0, 0)
        ingredientButtons.Controls.Add(
            _selectAllIngredientsButton,
            0,
            0
        )
        ingredientButtons.Controls.Add(_clearIngredientsButton, 1, 0)
        ingredientConstraintLayout.Controls.Add(
            ingredientHelpLabel,
            0,
            0
        )
        ingredientConstraintLayout.Controls.Add(
            _ingredientChoicesCheckedListBox,
            0,
            1
        )
        ingredientConstraintLayout.Controls.Add(
            ingredientButtons,
            0,
            2
        )
        ingredientsPage.Controls.Add(ingredientConstraintLayout)
        Controls.Add(_constraintTabs)

        SummaryLabel.Visible = False
        _resultsTabs = New TabControl With {
            .Anchor =
                AnchorStyles.Top Or AnchorStyles.Bottom Or
                AnchorStyles.Left Or AnchorStyles.Right,
            .Location = New Point(335, 460),
            .Name = "PlanResultsTabControl",
            .Size = New Size(925, 260),
            .TabIndex = 11
        }
        Dim nutritionPage As New TabPage(
            "Weekly recommended-intake summary"
        ) With {
            .Name = "NutritionSummaryTabPage"
        }
        SummaryDataGrid.Dock = DockStyle.Fill
        SummaryDataGrid.TabIndex = 0
        nutritionPage.Controls.Add(SummaryDataGrid)

        _ingredientResultsPage = New TabPage("Ingredients used") With {
            .Name = "PlanIngredientsTabPage",
            .Padding = New Padding(6)
        }
        Dim ingredientsLayout As New TableLayoutPanel With {
            .ColumnCount = 1,
            .Dock = DockStyle.Fill,
            .RowCount = 2
        }
        ingredientsLayout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100)
        )
        ingredientsLayout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 30)
        )
        ingredientsLayout.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100)
        )
        Dim ingredientsSummaryLabel As New Label With {
            .AutoSize = True,
            .Dock = DockStyle.Fill,
            .Name = "PlanIngredientsHelpLabel",
            .Text = "Totals include enough whole recipe batches for every planned meal slot."
        }
        _planIngredientsDataGrid = New DataGridView With {
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            .Dock = DockStyle.Fill,
            .Name = "PlanIngredientsDataGrid",
            .ReadOnly = False,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.CellSelect
        }
        _planIngredientsDataGrid.Columns.Add(
            New DataGridViewTextBoxColumn With {
                .FillWeight = 55,
                .HeaderText = "Ingredient",
                .Name = "PlannedIngredientColumn",
                .ReadOnly = True
            }
        )
        _planIngredientsDataGrid.Columns.Add(
            New DataGridViewTextBoxColumn With {
                .FillWeight = 25,
                .HeaderText = "Amount for planned week",
                .Name = "PlannedIngredientAmountColumn",
                .ReadOnly = True
            }
        )
        _planIngredientsDataGrid.Columns.Add(
            New DataGridViewComboBoxColumn With {
                .DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                .FillWeight = 20,
                .FlatStyle = FlatStyle.Flat,
                .HeaderText = "Measurement",
                .Name = "PlannedIngredientMeasurementColumn"
            }
        )
        AddHandler _planIngredientsDataGrid.CurrentCellDirtyStateChanged,
            AddressOf PlanIngredientsDataGrid_CurrentCellDirtyStateChanged
        AddHandler _planIngredientsDataGrid.CellValueChanged,
            AddressOf PlanIngredientsDataGrid_CellValueChanged
        AddHandler _planIngredientsDataGrid.DataError,
            AddressOf PlanIngredientsDataGrid_DataError
        ingredientsLayout.Controls.Add(ingredientsSummaryLabel, 0, 0)
        ingredientsLayout.Controls.Add(_planIngredientsDataGrid, 0, 1)
        _ingredientResultsPage.Controls.Add(ingredientsLayout)
        _resultsTabs.TabPages.AddRange({
            nutritionPage,
            _ingredientResultsPage
        })
        Controls.Add(_resultsTabs)
        _resultsTabs.BringToFront()
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

    Private Sub PopulateIngredientChoices()
        _ingredientChoicesCheckedListBox.Items.Clear()
        Dim ingredientNames As New Dictionary(Of String, String)(
            StringComparer.OrdinalIgnoreCase
        )
        For Each meal In _allMeals
            For Each ingredient In If(
                meal.Ingredients,
                New List(Of RecipeIngredient)
            )
                If ingredient Is Nothing Then Continue For
                Dim key = IngredientMeasurementConverter.IngredientKey(
                    ingredient.Ingredient
                )
                If key = String.Empty Then Continue For
                If Not ingredientNames.ContainsKey(key) OrElse
                    ingredient.Ingredient.Length <
                    ingredientNames(key).Length Then
                    ingredientNames(key) = ingredient.Ingredient
                End If
            Next
        Next

        For Each ingredientName In ingredientNames.OrderBy(
            Function(item) item.Value,
            StringComparer.CurrentCultureIgnoreCase
        )
            _ingredientChoicesCheckedListBox.Items.Add(
                New IngredientChoice(
                    ingredientName.Key,
                    ingredientName.Value
                ),
                True
            )
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
        RestoreSavedIngredientSelection(savedPlan)
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

    Private Sub RestoreSavedIngredientSelection(plan As WeeklyPlan)
        If plan Is Nothing OrElse Not plan.IngredientFilterApplied Then
            SetAllIngredientChoices(True)
            Return
        End If

        Dim allowedKeys As New HashSet(Of String)(
            If(
                plan.AllowedIngredientNames,
                New List(Of String)
            ).Select(
                Function(name)
                    Return IngredientMeasurementConverter.IngredientKey(name)
                End Function
            ),
            StringComparer.OrdinalIgnoreCase
        )
        For index As Integer = 0 To _ingredientChoicesCheckedListBox.Items.Count - 1
            Dim choice = DirectCast(
                _ingredientChoicesCheckedListBox.Items(index),
                IngredientChoice
            )
            _ingredientChoicesCheckedListBox.SetItemChecked(
                index,
                allowedKeys.Contains(choice.Key)
            )
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

    Private Sub SelectAllIngredientsButton_Click(
        sender As Object,
        e As EventArgs
    )
        SetAllIngredientChoices(True)
        SetReadyStatus()
    End Sub

    Private Sub ClearIngredientsButton_Click(
        sender As Object,
        e As EventArgs
    )
        SetAllIngredientChoices(False)
        SetReadyStatus()
    End Sub

    Private Sub SetAllIngredientChoices(isChecked As Boolean)
        For index As Integer = 0 To _ingredientChoicesCheckedListBox.Items.Count - 1
            _ingredientChoicesCheckedListBox.SetItemChecked(
                index,
                isChecked
            )
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
            Dim availableMeals = GetIngredientEligibleMeals(selectedMeals)
            Dim nutrientInfo As New NutrientInfo()
            Dim displayMeasurements = New Dictionary(Of String, String)(
                If(
                    _currentPlan?.IngredientDisplayMeasurements,
                    New Dictionary(Of String, String)()
                ),
                StringComparer.OrdinalIgnoreCase
            )
            _currentPlan = WeekPlanGenerator.Generate(
                selectedMeals,
                availableMeals,
                generationMode,
                plannedMealTypes,
                nutrientInfo.RecommendedDailyIntakes
            )
            _currentPlan.IngredientDisplayMeasurements = displayMeasurements
            _currentPlan.IngredientFilterApplied =
                _ingredientChoicesCheckedListBox.CheckedItems.Count <
                _ingredientChoicesCheckedListBox.Items.Count
            _currentPlan.AllowedIngredientNames =
                _ingredientChoicesCheckedListBox.CheckedItems.
                    Cast(Of IngredientChoice)().
                    Select(Function(choice) choice.DisplayName).
                    ToList()
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

    Private Function GetIngredientEligibleMeals(
        guaranteedMeals As IEnumerable(Of Meal)
    ) As List(Of Meal)
        Dim filterApplied =
            _ingredientChoicesCheckedListBox.CheckedItems.Count <
            _ingredientChoicesCheckedListBox.Items.Count
        If Not filterApplied Then Return New List(Of Meal)(_allMeals)

        Dim allowedKeys As New HashSet(Of String)(
            _ingredientChoicesCheckedListBox.CheckedItems.
                Cast(Of IngredientChoice)().
                Select(Function(choice) choice.Key),
            StringComparer.OrdinalIgnoreCase
        )
        Dim eligibleMeals = _allMeals.Where(
            Function(meal) MealUsesOnlyAllowedIngredients(meal, allowedKeys)
        ).ToList()
        Dim invalidGuarantees = If(
            guaranteedMeals,
            Enumerable.Empty(Of Meal)
        ).Where(
            Function(meal) Not eligibleMeals.Contains(meal)
        ).Select(Function(meal) meal.Name).
            Distinct(StringComparer.CurrentCultureIgnoreCase).
            OrderBy(
                Function(name) name,
                StringComparer.CurrentCultureIgnoreCase
            ).
            ToList()
        If invalidGuarantees.Count > 0 Then
            Throw New WeeklyPlanException(
                "These checked recipes require an excluded ingredient: " &
                String.Join(", ", invalidGuarantees) &
                ". Re-enable their ingredients or uncheck the recipes."
            )
        End If
        If eligibleMeals.Count = 0 Then
            Throw New WeeklyPlanException(
                "No recipes use only the selected ingredients. Re-enable at least one ingredient."
            )
        End If
        Return eligibleMeals
    End Function

    Private Shared Function MealUsesOnlyAllowedIngredients(
        meal As Meal,
        allowedKeys As HashSet(Of String)
    ) As Boolean
        If meal Is Nothing OrElse meal.Ingredients Is Nothing Then Return False
        Dim ingredientKeys = meal.Ingredients.
            Where(Function(ingredient) ingredient IsNot Nothing).
            Select(
                Function(ingredient)
                    Return IngredientMeasurementConverter.IngredientKey(
                        ingredient.Ingredient
                    )
                End Function
            ).
            Where(Function(key) key <> String.Empty).
            Distinct(StringComparer.OrdinalIgnoreCase).
            ToList()
        Return ingredientKeys.Count > 0 AndAlso
            ingredientKeys.All(Function(key) allowedKeys.Contains(key))
    End Function

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
                "Guarantee checked recipes; fill the rest freely."
            SelectedRecipesLabel.Text =
                "Guaranteed recipes (optional; at least once)"
        Else
            GenerationModeHelpLabel.Text =
                "Use checked recipes only; include each once."
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
        _planIngredientsDataGrid.Rows.Clear()
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
        PopulatePlanIngredients(plan)
        _resultsTabs.SelectedTab = _ingredientResultsPage
        BalanceLabel.Text = CreateBalanceDescription(
            calorieTotals,
            nutrientCoverage
        )
    End Sub

    Private Sub PopulatePlanIngredients(plan As WeeklyPlan)
        If plan Is Nothing OrElse plan.Days Is Nothing Then Return

        Dim entries As New List(Of IngredientAmountEntry)
        Dim plannedMeals = plan.Days.
            Where(Function(day) day IsNot Nothing AndAlso day.Meals IsNot Nothing).
            SelectMany(Function(day) day.Meals).
            Where(Function(meal) meal IsNot Nothing).
            ToList()
        For Each recipeGroup In plannedMeals.GroupBy(
            Function(plannedMeal)
                If Not String.IsNullOrWhiteSpace(plannedMeal.RecipeUrl) Then
                    Return "url|" & plannedMeal.RecipeUrl.Trim()
                End If
                Return "name|" & If(plannedMeal.MealName, String.Empty).Trim()
            End Function,
            StringComparer.OrdinalIgnoreCase
        )
            Dim plannedMeal = recipeGroup.First()
            Dim currentMeal = FindCurrentMeal(plannedMeal)
            Dim ingredients = If(
                plannedMeal.Ingredients IsNot Nothing AndAlso
                plannedMeal.Ingredients.Count > 0,
                plannedMeal.Ingredients,
                If(
                    currentMeal?.Ingredients,
                    New List(Of RecipeIngredient)
                )
            )
            Dim servingsPerBatch = plannedMeal.RecipeServings
            If servingsPerBatch < 1 AndAlso currentMeal IsNot Nothing Then
                servingsPerBatch = currentMeal.Servings
            End If
            servingsPerBatch = Math.Max(1, servingsPerBatch)
            Dim batches = Math.Ceiling(
                recipeGroup.Count() / CDbl(servingsPerBatch)
            )
            For Each ingredient In ingredients
                entries.Add(New IngredientAmountEntry(ingredient, batches))
            Next
        Next

        Dim measurementSystem =
            IngredientMeasurementConverter.NormalizeSystem(
                AppSettingsRepository.Load().IngredientMeasurementSystem
            )
        If plan.IngredientDisplayMeasurements Is Nothing Then
            plan.IngredientDisplayMeasurements =
                New Dictionary(Of String, String)(
                    StringComparer.OrdinalIgnoreCase
                )
        End If

        _updatingPlanIngredientMeasurements = True
        Try
            _planIngredientsDataGrid.Rows.Clear()
            For Each total In IngredientMeasurementConverter.Aggregate(
                entries,
                measurementSystem
            )
                Dim displayMeasurement = total.DefaultMeasurement
                Dim savedMeasurement As String = Nothing
                If plan.IngredientDisplayMeasurements.TryGetValue(
                    total.Key,
                    savedMeasurement
                ) Then
                    savedMeasurement =
                        IngredientMeasurementConverter.NormalizeUnit(
                            savedMeasurement
                        )
                    If total.CompatibleMeasurements.Contains(
                        savedMeasurement,
                        StringComparer.OrdinalIgnoreCase
                    ) Then
                        displayMeasurement = savedMeasurement
                    End If
                End If

                Dim rowIndex = _planIngredientsDataGrid.Rows.Add(
                    total.Ingredient,
                    total.FormatAmount(displayMeasurement),
                    Nothing
                )
                Dim row = _planIngredientsDataGrid.Rows(rowIndex)
                row.Tag = total
                Dim measurementCell = DirectCast(
                    row.Cells("PlannedIngredientMeasurementColumn"),
                    DataGridViewComboBoxCell
                )
                If total.CompatibleMeasurements.Count = 0 Then
                    measurementCell.ReadOnly = True
                Else
                    measurementCell.Items.AddRange(
                        total.CompatibleMeasurements.
                            Cast(Of Object)().
                            ToArray()
                    )
                    measurementCell.Value = displayMeasurement
                End If
            Next
        Finally
            _updatingPlanIngredientMeasurements = False
        End Try
    End Sub

    Private Sub PlanIngredientsDataGrid_CurrentCellDirtyStateChanged(
        sender As Object,
        e As EventArgs
    )
        If _planIngredientsDataGrid.IsCurrentCellDirty AndAlso
            TypeOf _planIngredientsDataGrid.CurrentCell Is
                DataGridViewComboBoxCell Then
            _planIngredientsDataGrid.CommitEdit(
                DataGridViewDataErrorContexts.Commit
            )
        End If
    End Sub

    Private Sub PlanIngredientsDataGrid_CellValueChanged(
        sender As Object,
        e As DataGridViewCellEventArgs
    )
        If _updatingPlanIngredientMeasurements OrElse e.RowIndex < 0 OrElse
            e.ColumnIndex < 0 OrElse
            _planIngredientsDataGrid.Columns(e.ColumnIndex).Name <>
                "PlannedIngredientMeasurementColumn" Then
            Return
        End If

        Dim row = _planIngredientsDataGrid.Rows(e.RowIndex)
        Dim total = TryCast(row.Tag, ConsolidatedIngredient)
        If total Is Nothing Then Return
        Dim measurement = IngredientMeasurementConverter.NormalizeUnit(
            Convert.ToString(
                row.Cells("PlannedIngredientMeasurementColumn").Value
            )
        )
        If Not total.CompatibleMeasurements.Contains(
            measurement,
            StringComparer.OrdinalIgnoreCase
        ) Then
            Return
        End If

        row.Cells("PlannedIngredientAmountColumn").Value =
            total.FormatAmount(measurement)
        If _currentPlan Is Nothing Then Return
        If _currentPlan.IngredientDisplayMeasurements Is Nothing Then
            _currentPlan.IngredientDisplayMeasurements =
                New Dictionary(Of String, String)(
                    StringComparer.OrdinalIgnoreCase
                )
        End If
        _currentPlan.IngredientDisplayMeasurements(total.Key) = measurement
        WeekPlanRepository.Save(_currentPlan)
    End Sub

    Private Sub PlanIngredientsDataGrid_DataError(
        sender As Object,
        e As DataGridViewDataErrorEventArgs
    )
        e.ThrowException = False
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
