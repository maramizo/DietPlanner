Imports Newtonsoft.Json
Imports System.IO
Imports System.Windows.Forms.Design

Public Class MainWindow
    Private Sub LoadData()
        Try
            Dim json As String = File.ReadAllText("./data/meals.json")
            Dim meals As List(Of Meal) = JsonConvert.DeserializeObject(Of List(Of Meal))(json)
            meals = meals.OrderBy(Of String)(Function(x) x.Name).ToList()

            'Next, we set ComboBox1 to ComboBox5 to display the names of the meals:
            Dim comboBoxes() As ComboBox = {ComboBox1, ComboBox2, ComboBox3, ComboBox4, ComboBox5}

            For Each comboBox As ComboBox In comboBoxes
                Dim bindingSource As New BindingSource()
                bindingSource.DataSource = meals
                comboBox.DataSource = bindingSource
                comboBox.DisplayMember = "name"
                AddHandler comboBox.SelectedIndexChanged, AddressOf ComboBox_SelectedIndexChanged
                comboBox.Text = "Select an option..."
                comboBox.DropDownStyle = ComboBoxStyle.DropDown
            Next

            Dim clearButtons() As Button = {ClearButton1, ClearButton2, ClearButton3, ClearButton4, ClearButton5}
            For Each clearButton As Button In clearButtons
                RemoveHandler clearButton.Click, AddressOf ClearButton_Click
                AddHandler clearButton.Click, AddressOf ClearButton_Click
            Next

            Dim ViewRecipeButtons() As Button = {ViewRecipe1, ViewRecipe2, ViewRecipe3, ViewRecipe4, ViewRecipe5}
            For Each viewRecipeButton As Button In ViewRecipeButtons
                RemoveHandler viewRecipeButton.Click, AddressOf ViewRecipe_Click
                AddHandler viewRecipeButton.Click, AddressOf ViewRecipe_Click
            Next

            Dim buttons() As Button = {Button1, Button2, Button3, Button4, Button5}

            For i As Integer = 0 To 4
                RemoveHandler buttons(i).Click, AddressOf Button_Click
                AddHandler buttons(i).Click, AddressOf Button_Click
            Next

            Dim viewRecipes() As Button = {ViewRecipe1, ViewRecipe2, ViewRecipe3, ViewRecipe4, ViewRecipe5}
            For i As Integer = 0 To 4
                viewRecipes(i).Enabled = False
            Next

        Catch ex As Exception
            If Not ex.Message.Contains("Could not find file") And Not ex.Message.Contains("Could not find a part") Then
                MessageBox.Show(ex.Message)
            End If
        End Try
    End Sub

    Private Sub ViewRecipe_Click(sender As Object, e As EventArgs) Handles ViewRecipe1.Click, ViewRecipe2.Click, ViewRecipe3.Click, ViewRecipe4.Click, ViewRecipe5.Click
        'This will open a URL in the default browser:
        Dim viewRecipe As Button = CType(sender, Button)
        Dim meal As Meal = CType(Controls.Find("ComboBox" & viewRecipe.Name.Substring(10), True)(0), ComboBox).SelectedItem
        Try
            Process.Start(meal.Recipe)
        Catch ex As Exception
            MessageBox.Show("Could not open the recipe. Link has been copied to clipboard.")
            Clipboard.SetText(meal.Recipe)
        End Try
    End Sub

    Private Sub ClearButton_Click(clearButton As Button, e As EventArgs) Handles ClearButton1.Click, ClearButton2.Click, ClearButton3.Click, ClearButton4.Click, ClearButton5.Click
        Dim comboBox As ComboBox = CType(Controls.Find("ComboBox" & clearButton.Name.Substring(11), True)(0), ComboBox)
        comboBox.Text = "Select an option..."
        CalculateTotalCalories()
        RecalculateEnabledViewButtons()
    End Sub

    Private Sub MainWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
        LoadData()
    End Sub

    Private Sub Button_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click, Button3.Click, Button4.Click, Button5.Click
        Dim button As Button = CType(sender, Button)
        Dim meal As Meal = CType(Controls.Find("ComboBox" & button.Name.Substring(6), True)(0), ComboBox).SelectedItem
        Dim recipeView As New RecipeView(meal)
        recipeView.Show()
    End Sub

    Private Sub AddButton_Click(sender As Object, e As EventArgs) Handles AddButton.Click
        Dim addMeal As New AddRecipe()
        AddHandler addMeal.FormClosing, AddressOf LoadData
        addMeal.Show()
    End Sub

    Private Sub ViewDailyButton_Click(sender As Object, e As EventArgs) Handles ViewDailyFactsButton.Click
        Dim currentMeals As New List(Of Meal)
        For Each comboBox In {ComboBox1, ComboBox2, ComboBox3, ComboBox4, ComboBox5}
            If comboBox.Text = "Select an option..." Or comboBox.Text = "" Then
                Continue For
            End If
            currentMeals.Add(CType(comboBox.SelectedItem, Meal))
        Next

        Dim dailyFacts As New DailyFacts(currentMeals)
        dailyFacts.Show()
    End Sub

    Private Sub CalculateTotalCalories()
        Dim totalCalories As Integer = 0
        For Each comboBox As ComboBox In {ComboBox1, ComboBox2, ComboBox3, ComboBox4, ComboBox5}
            If comboBox.Text = "Select an option..." Or comboBox.Text = "" Then
                Continue For
            End If
            Dim meal As Meal = CType(comboBox.SelectedItem, Meal)
            totalCalories += meal.Calory
        Next

        TotalCaloriesLabel.Text = totalCalories
    End Sub

    Private Sub RecalculateEnabledViewButtons()
        For i As Integer = 0 To 4
            Dim comboBox As ComboBox = CType(Controls.Find("ComboBox" & (i + 1), True)(0), ComboBox)
            Dim meal As Meal = comboBox.SelectedItem
            Dim viewRecipeUrl As Button = CType(Controls.Find("ViewRecipe" & (i + 1), True)(0), Button)
            Dim viewRecipe As Button = CType(Controls.Find("Button" & (i + 1), True)(0), Button)
            viewRecipe.Enabled = meal IsNot Nothing And comboBox.Text <> "Select an option..."
            viewRecipeUrl.Enabled = meal IsNot Nothing And comboBox.Text <> "Select an option..."
        Next
    End Sub

    Private Sub ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        ' Cast the sender to a ComboBox
        Dim selectedComboBox As ComboBox = CType(sender, ComboBox)
        Dim viewRecipe As Button = CType(Controls.Find("ViewRecipe" & selectedComboBox.Name.Substring(8), True)(0), Button)
        Dim viewRecipeUrl As Button = CType(Controls.Find("Button" & selectedComboBox.Name.Substring(8), True)(0), Button)

        ' Get the selected meal
        Dim selectedMeal As Meal = CType(selectedComboBox.SelectedItem, Meal)
        viewRecipe.Enabled = selectedMeal IsNot Nothing
        viewRecipeUrl.Enabled = selectedMeal IsNot Nothing

        CalculateTotalCalories()
    End Sub
End Class