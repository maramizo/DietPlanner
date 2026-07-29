Public Class MainWindow
    Private Const EmptySelectionText As String = "Select an option..."
    Private Shared ReadOnly SlotMealTypes As String() = {
        "Breakfast",
        "Brunch",
        "Lunch",
        "Dinner",
        "Snack"
    }

    Private Class AdvancedMigrationResult
        Public Property ChangedCount As Integer
        Public ReadOnly Property UnavailableMeals As New List(Of String)
        Public ReadOnly Property RetryableFailures As New List(Of AdvancedMigrationFailure)
    End Class

    Private Class AdvancedMigrationFailure
        Public Property MealName As String
        Public Property [Error] As Exception
    End Class

    Private Class AdvancedMigrationItemResult
        Public Property Meal As Meal
        Public Property Details As AdvancedRecipeDetails
        Public Property UnavailableNote As String
        Public Property RetryableFailure As Exception
    End Class

    Private Class CategoryMigrationResult
        Public Property UpdatedCount As Integer
        Public Property Succeeded As Boolean
        Public Property Failure As Exception
    End Class

    Public Sub New()
        InitializeComponent()
        ApplyAppIcon(Me)
    End Sub

    Private Async Function LoadDataAsync() As Task
        Dim meals = MealRepository.LoadAll()
        Dim advancedCandidateCount = meals.
            Where(Function(meal) meal.NeedsAdvancedScrape()).
            Count()
        Dim needsCategoryUpgrade =
            MealRepository.LoadCategoryVersion() < MealRepository.CurrentCategoryVersion
        Dim hasUncategorizedMeals = meals.Any(
            Function(meal) meal.MealTypes Is Nothing OrElse meal.MealTypes.Count = 0
        )
        If needsCategoryUpgrade AndAlso meals.Count = 0 Then
            MealRepository.SaveCurrentCategoryVersion()
        End If
        Dim needsCategoryMigration =
            meals.Count > 0 AndAlso (needsCategoryUpgrade OrElse hasUncategorizedMeals)

        Dim advancedMigration As AdvancedMigrationResult = Nothing
        Dim categoryMigration As CategoryMigrationResult = Nothing
        If advancedCandidateCount > 0 OrElse needsCategoryMigration Then
            Dim loaderMessage As String
            If advancedCandidateCount > 0 AndAlso needsCategoryMigration Then
                loaderMessage =
                    "Updating recipe details and meal categories in parallel..."
            ElseIf advancedCandidateCount > 0 Then
                loaderMessage =
                    "Updating serving data, ingredients, directions, and notes for " &
                    advancedCandidateCount &
                    " existing recipes in parallel..."
            ElseIf needsCategoryUpgrade Then
                loaderMessage = "Updating recipe meal categories..."
            Else
                loaderMessage = "Categorizing existing recipes..."
            End If

            Dim loader As New Loading(loaderMessage)
            loader.Show(Me)
            Try
                Dim compatibilityTasks As New List(Of Task)
                Dim advancedTask As Task(Of AdvancedMigrationResult) = Nothing
                Dim categoryTask As Task(Of CategoryMigrationResult) = Nothing

                If advancedCandidateCount > 0 Then
                    advancedTask = MigrateAdvancedDetailsAsync(meals)
                    compatibilityTasks.Add(advancedTask)
                End If
                If needsCategoryMigration Then
                    categoryTask = MigrateCategoriesAsync(meals, needsCategoryUpgrade)
                    compatibilityTasks.Add(categoryTask)
                End If

                Await Task.WhenAll(compatibilityTasks)
                If advancedTask IsNot Nothing Then advancedMigration = Await advancedTask
                If categoryTask IsNot Nothing Then categoryMigration = Await categoryTask

                Dim recipesChanged =
                    advancedMigration IsNot Nothing AndAlso
                    advancedMigration.ChangedCount > 0
                Dim categoriesChanged =
                    categoryMigration IsNot Nothing AndAlso
                    categoryMigration.UpdatedCount > 0
                Dim preserveNormalizedCategories =
                    categoryMigration IsNot Nothing AndAlso
                    Not categoryMigration.Succeeded
                If recipesChanged OrElse
                    categoriesChanged OrElse
                    preserveNormalizedCategories Then
                    MealRepository.SaveAll(meals)
                End If
                If needsCategoryUpgrade AndAlso
                    categoryMigration IsNot Nothing AndAlso
                    categoryMigration.Succeeded Then
                    MealRepository.SaveCurrentCategoryVersion()
                End If
            Finally
                loader.Close()
            End Try

            If advancedMigration IsNot Nothing Then
                ShowAdvancedMigrationResult(advancedMigration)
            End If
            If categoryMigration IsNot Nothing Then
                ShowCategoryMigrationResult(categoryMigration)
            End If
        End If

        BindMealSelectors(meals)
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Function

    Private Async Function MigrateAdvancedDetailsAsync(
        meals As List(Of Meal)
    ) As Task(Of AdvancedMigrationResult)
        Dim result As New AdvancedMigrationResult()
        Dim migrationTasks = meals.
            Where(Function(candidate) candidate.NeedsAdvancedScrape()).
            Select(Function(meal) MigrateAdvancedMealAsync(meal)).
            ToArray()
        Dim itemResults = Await Task.WhenAll(migrationTasks)

        For Each itemResult In itemResults
            If itemResult.Details IsNot Nothing Then
                itemResult.Meal.ApplyAdvancedDetails(itemResult.Details)
                result.ChangedCount += 1
            ElseIf itemResult.UnavailableNote IsNot Nothing Then
                itemResult.Meal.MarkAdvancedScrapeUnavailable(
                    itemResult.UnavailableNote
                )
                result.UnavailableMeals.Add(itemResult.Meal.Name)
                result.ChangedCount += 1
            ElseIf itemResult.RetryableFailure IsNot Nothing Then
                result.RetryableFailures.Add(
                    New AdvancedMigrationFailure With {
                        .MealName = itemResult.Meal.Name,
                        .Error = itemResult.RetryableFailure
                    }
                )
            End If
        Next

        Return result
    End Function

    Private Async Function MigrateAdvancedMealAsync(
        meal As Meal
    ) As Task(Of AdvancedMigrationItemResult)
        Dim result As New AdvancedMigrationItemResult With {.Meal = meal}
        Try
            result.Details = Await API.ScrapeAdvancedDetails(meal.Recipe)
        Catch ex As RecipeSourceUnavailableException
            result.UnavailableNote = ex.Message
        Catch ex As Exception
            result.RetryableFailure = ex
        End Try
        Return result
    End Function

    Private Async Function MigrateCategoriesAsync(
        meals As List(Of Meal),
        recategorizeAll As Boolean
    ) As Task(Of CategoryMigrationResult)
        Dim result As New CategoryMigrationResult()
        Try
            result.UpdatedCount = If(
                recategorizeAll,
                Await API.RecategorizeMealsAsync(meals),
                Await API.CategorizeUncategorizedMealsAsync(meals)
            )
            result.Succeeded = True
        Catch ex As Exception
            result.Failure = ex
        End Try
        Return result
    End Function

    Private Sub ShowAdvancedMigrationResult(result As AdvancedMigrationResult)
        Dim messageParts As New List(Of String)

        If result.UnavailableMeals.Count > 0 Then
            messageParts.Add(
                "These recipe sources could not provide serving data, ingredients, directions, and notes and were marked " &
                "'Source unavailable', so DietPlanner will not retry them automatically:" &
                Environment.NewLine &
                String.Join(
                    Environment.NewLine,
                    result.UnavailableMeals.Select(Function(name) "• " & name)
                )
            )
        End If

        If result.RetryableFailures.Count > 0 Then
            messageParts.Add(
                "These recipes hit temporary errors and remain pending for the next startup:" &
                Environment.NewLine &
                String.Join(
                    Environment.NewLine,
                    result.RetryableFailures.Select(
                        Function(failure)
                            Return "• " & failure.MealName & ": " & failure.Error.Message
                        End Function
                    )
                )
            )
        End If

        If messageParts.Count = 0 Then Return
        MessageBox.Show(
            String.Join(Environment.NewLine & Environment.NewLine, messageParts),
            "Recipe detail migration",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
    End Sub

    Private Sub ShowCategoryMigrationResult(result As CategoryMigrationResult)
        If result.Succeeded OrElse result.Failure Is Nothing Then Return

        MessageBox.Show(
            "Existing recipes were loaded, but DietPlanner could not update all of their categories. " &
            "Breakfast recipes were still made available for Brunch, and the broader category update " &
            "will be retried next time." &
            Environment.NewLine & Environment.NewLine & result.Failure.Message,
            "DietPlanner",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
    End Sub

    Private Sub BindMealSelectors(meals As List(Of Meal))
        Dim comboBoxes() As ComboBox = {
            ComboBox1,
            ComboBox2,
            ComboBox3,
            ComboBox4,
            ComboBox5
        }

        For index As Integer = 0 To comboBoxes.Length - 1
            Dim comboBox = comboBoxes(index)
            Dim mealType = SlotMealTypes(index)
            RemoveHandler comboBox.SelectedIndexChanged, AddressOf ComboBox_SelectedIndexChanged
            comboBox.DataSource = Nothing

            Dim matchingMeals = meals.Where(
                Function(meal) meal.SupportsMealType(mealType)
            ).ToList()
            Dim bindingSource As New BindingSource With {
                .DataSource = matchingMeals
            }
            comboBox.DataSource = bindingSource
            comboBox.DisplayMember = NameOf(Meal.Name)
            comboBox.DropDownStyle = ComboBoxStyle.DropDown
            ClearSelection(comboBox)
            AddHandler comboBox.SelectedIndexChanged, AddressOf ComboBox_SelectedIndexChanged
        Next
    End Sub

    Private Sub ClearSelection(comboBox As ComboBox)
        comboBox.SelectedIndex = -1
        comboBox.Text = EmptySelectionText
    End Sub

    Private Async Sub MainWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
        Try
            Await LoadDataAsync()
        Catch ex As Exception
            MessageBox.Show(
                "Could not load saved recipes." & Environment.NewLine & Environment.NewLine & ex.Message,
                "DietPlanner",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try

#If Not DEBUG Then
        If Await AutoUpdater.TryInstallLatestReleaseAsync(Me) Then
            Close()
        End If
#End If
    End Sub

    Private Sub ViewRecipe_Click(
        sender As Object,
        e As EventArgs
    ) Handles ViewRecipe1.Click, ViewRecipe2.Click, ViewRecipe3.Click, ViewRecipe4.Click, ViewRecipe5.Click
        Dim viewRecipe = DirectCast(sender, Button)
        Dim comboBox = DirectCast(
            Controls.Find("ComboBox" & viewRecipe.Name.Substring(10), True)(0),
            ComboBox
        )
        Dim meal = TryCast(comboBox.SelectedItem, Meal)
        If meal Is Nothing Then Return

        Try
            Process.Start(
                New ProcessStartInfo(meal.Recipe) With {
                    .UseShellExecute = True
                }
            )
        Catch ex As Exception
            MessageBox.Show("Could not open the recipe. The link has been copied to the clipboard.")
            Clipboard.SetText(meal.Recipe)
        End Try
    End Sub

    Private Sub ClearButton_Click(
        clearButton As Button,
        e As EventArgs
    ) Handles ClearButton1.Click, ClearButton2.Click, ClearButton3.Click, ClearButton4.Click, ClearButton5.Click
        Dim comboBox = DirectCast(
            Controls.Find("ComboBox" & clearButton.Name.Substring(11), True)(0),
            ComboBox
        )
        ClearSelection(comboBox)
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Sub

    Private Sub ClearAllButton_Click(sender As Object, e As EventArgs) Handles ClearAllButton.Click
        For Each comboBox As ComboBox In {
            ComboBox1,
            ComboBox2,
            ComboBox3,
            ComboBox4,
            ComboBox5
        }
            ClearSelection(comboBox)
        Next
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Sub

    Private Sub Button_Click(
        sender As Object,
        e As EventArgs
    ) Handles Button1.Click, Button2.Click, Button3.Click, Button4.Click, Button5.Click
        Dim button = DirectCast(sender, Button)
        Dim comboBox = DirectCast(
            Controls.Find("ComboBox" & button.Name.Substring(6), True)(0),
            ComboBox
        )
        Dim meal = TryCast(comboBox.SelectedItem, Meal)
        If meal Is Nothing Then Return

        Dim recipeView As New RecipeView(meal)
        recipeView.Show()
    End Sub

    Private Sub AddButton_Click(sender As Object, e As EventArgs) Handles AddButton.Click
        Dim addMeal As New AddRecipe()
        AddHandler addMeal.FormClosed, AddressOf AddMeal_FormClosed
        addMeal.Show()
    End Sub

    Private Async Sub ViewAllRecipesButton_Click(
        sender As Object,
        e As EventArgs
    ) Handles ViewAllRecipesButton.Click
        Using catalog As New RecipeCatalog()
            If catalog.ShowDialog(Me) <> DialogResult.OK Then Return
        End Using

        Try
            Await LoadDataAsync()
        Catch ex As Exception
            MessageBox.Show(
                "The categories were saved, but DietPlanner could not refresh the main window." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "DietPlanner",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
        End Try
    End Sub

    Private Sub PlanWeekButton_Click(sender As Object, e As EventArgs) Handles PlanWeekButton.Click
        Using planner As New WeekPlanner()
            planner.ShowDialog(Me)
        End Using
    End Sub

    Private Sub SettingsButton_Click(
        sender As Object,
        e As EventArgs
    ) Handles SettingsButton.Click
        Using settings As New SettingsForm()
            settings.ShowDialog(Me)
        End Using
    End Sub

    Private Async Sub AddMeal_FormClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Await LoadDataAsync()
        Catch ex As Exception
            MessageBox.Show(
                "Could not reload saved recipes." & Environment.NewLine & Environment.NewLine & ex.Message,
                "DietPlanner",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub

    Private Sub ViewDailyButton_Click(
        sender As Object,
        e As EventArgs
    ) Handles ViewDailyFactsButton.Click
        Dim currentMeals As New List(Of Meal)
        For Each comboBox As ComboBox In {
            ComboBox1,
            ComboBox2,
            ComboBox3,
            ComboBox4,
            ComboBox5
        }
            Dim meal = TryCast(comboBox.SelectedItem, Meal)
            If meal IsNot Nothing Then currentMeals.Add(meal)
        Next

        Dim dailyFacts As New DailyFacts(currentMeals)
        dailyFacts.Show()
    End Sub

    Private Sub CalculateTotalCalories()
        Dim totalCalories As Integer = 0
        For Each comboBox As ComboBox In {
            ComboBox1,
            ComboBox2,
            ComboBox3,
            ComboBox4,
            ComboBox5
        }
            Dim meal = TryCast(comboBox.SelectedItem, Meal)
            If meal IsNot Nothing Then totalCalories += meal.Calory
        Next

        TotalCaloriesLabel.Text = totalCalories.ToString()
    End Sub

    Private Sub RecalculateEnabledViewButtons()
        For index As Integer = 1 To 5
            Dim comboBox = DirectCast(
                Controls.Find("ComboBox" & index, True)(0),
                ComboBox
            )
            Dim hasMeal = TryCast(comboBox.SelectedItem, Meal) IsNot Nothing
            For Each buttonPrefix In {"Button", "ClearButton", "ViewRecipe"}
                Dim actionButton = DirectCast(
                    Controls.Find(buttonPrefix & index, True)(0),
                    Button
                )
                actionButton.Enabled = hasMeal
                actionButton.Visible = hasMeal
            Next
        Next
    End Sub

    Private Sub ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Sub
End Class
