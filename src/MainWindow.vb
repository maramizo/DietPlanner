Imports Newtonsoft.Json
Imports System.IO

Public Class MainWindow
    Private Const EmptySelectionText As String = "Select an option..."
    Private Shared ReadOnly SlotMealTypes As String() = {
        "Breakfast",
        "Lunch",
        "Brunch",
        "Dinner",
        "Snack"
    }

    Private Class AdvancedMigrationResult
        Public Property ChangedCount As Integer
        Public ReadOnly Property UnavailableMeals As New List(Of String)
        Public Property RetryableFailure As Exception
    End Class

    Public Sub New()
        InitializeComponent()
        ApplyAppIcon(Me)
    End Sub

    Private Async Function LoadDataAsync() As Task
        Dim meals = LoadMeals()

        If meals.Any(Function(meal) meal.NeedsAdvancedScrape()) Then
            Dim loader As New Loading("Adding ingredients and directions to existing recipes...")
            loader.Show(Me)
            Try
                Dim migration = Await MigrateAdvancedDetailsAsync(meals, loader)
                If migration.ChangedCount > 0 Then SaveMeals(meals)
                ShowAdvancedMigrationResult(migration)
            Finally
                loader.Close()
            End Try
        End If

        If meals.Any(
            Function(meal) meal.MealTypes Is Nothing OrElse meal.MealTypes.Count = 0
        ) Then
            Dim loader As New Loading("Categorizing existing recipes...")
            loader.Show(Me)
            Try
                Dim updatedCount = Await API.CategorizeUncategorizedMealsAsync(meals)
                If updatedCount > 0 Then SaveMeals(meals)
            Catch ex As Exception
                MessageBox.Show(
                    "Existing recipes were loaded, but DietPlanner could not categorize all of them." &
                    Environment.NewLine & Environment.NewLine & ex.Message,
                    "DietPlanner",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
            Finally
                loader.Close()
            End Try
        End If

        BindMealSelectors(meals)
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Function

    Private Async Function MigrateAdvancedDetailsAsync(
        meals As List(Of Meal),
        loader As Loading
    ) As Task(Of AdvancedMigrationResult)
        Dim result As New AdvancedMigrationResult()

        For Each meal In meals.Where(Function(candidate) candidate.NeedsAdvancedScrape())
            loader.UpdateMessage("Adding ingredients and directions to " & meal.Name & "...")
            Try
                Dim details = Await API.ScrapeAdvancedDetails(meal.Recipe)
                meal.ApplyAdvancedDetails(details)
                result.ChangedCount += 1
            Catch ex As RecipeSourceUnavailableException
                meal.MarkAdvancedScrapeUnavailable(ex.Message)
                result.UnavailableMeals.Add(meal.Name)
                result.ChangedCount += 1
            Catch ex As Exception
                result.RetryableFailure = ex
                Exit For
            End Try
        Next

        Return result
    End Function

    Private Sub ShowAdvancedMigrationResult(result As AdvancedMigrationResult)
        Dim messageParts As New List(Of String)

        If result.UnavailableMeals.Count > 0 Then
            messageParts.Add(
                "These recipe sources could not provide ingredients and directions and were marked " &
                "'Source unavailable', so DietPlanner will not retry them automatically:" &
                Environment.NewLine &
                String.Join(
                    Environment.NewLine,
                    result.UnavailableMeals.Select(Function(name) "• " & name)
                )
            )
        End If

        If result.RetryableFailure IsNot Nothing Then
            messageParts.Add(
                "The remaining advanced-detail migration is still pending and will be retried on " &
                "the next startup." &
                Environment.NewLine &
                Environment.NewLine &
                result.RetryableFailure.Message
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

    Private Function LoadMeals() As List(Of Meal)
        If Not File.Exists("./data/meals.json") Then Return New List(Of Meal)

        Dim json = File.ReadAllText("./data/meals.json")
        Dim meals = JsonConvert.DeserializeObject(Of List(Of Meal))(json)
        If meals Is Nothing Then meals = New List(Of Meal)
        Return meals.OrderBy(Function(meal) meal.Name).ToList()
    End Function

    Private Sub SaveMeals(meals As List(Of Meal))
        If Not Directory.Exists("./data") Then Directory.CreateDirectory("./data")
        File.WriteAllText(
            "./data/meals.json",
            JsonConvert.SerializeObject(meals, Formatting.Indented)
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
            DirectCast(Controls.Find("ViewRecipe" & index, True)(0), Button).Enabled = hasMeal
            DirectCast(Controls.Find("Button" & index, True)(0), Button).Enabled = hasMeal
        Next
    End Sub

    Private Sub ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Sub
End Class
