Public Class RecipeCatalog
    Private ReadOnly _meals As List(Of Meal)
    Private _updatingCategoryCells As Boolean

    Public Sub New()
        InitializeComponent()
        ApplyAppIcon(Me)
        _meals = MealRepository.LoadAll()
        PopulateRecipes()
    End Sub

    Private Sub PopulateRecipes()
        CatalogDataGrid.Rows.Clear()
        For Each meal In _meals
            Dim rowIndex = CatalogDataGrid.Rows.Add(
                meal.Name,
                meal.Calory,
                HasMealType(meal, "Breakfast"),
                HasMealType(meal, "Brunch"),
                HasMealType(meal, "Lunch"),
                HasMealType(meal, "Dinner"),
                HasMealType(meal, "Snack"),
                GetAdvancedStatus(meal)
            )
            CatalogDataGrid.Rows(rowIndex).Tag = meal
        Next
    End Sub

    Private Shared Function HasMealType(meal As Meal, mealType As String) As Boolean
        Return meal.MealTypes IsNot Nothing AndAlso meal.MealTypes.Any(
            Function(value) String.Equals(
                value,
                mealType,
                StringComparison.OrdinalIgnoreCase
            )
        )
    End Function

    Private Shared Function GetAdvancedStatus(meal As Meal) As String
        If meal.IsAdvancedScrapeUnavailable() Then Return "Source unavailable"
        If meal.NeedsAdvancedScrape() Then Return "Pending"
        Return "Complete"
    End Function

    Private Sub CatalogDataGrid_CurrentCellDirtyStateChanged(
        sender As Object,
        e As EventArgs
    ) Handles CatalogDataGrid.CurrentCellDirtyStateChanged
        If CatalogDataGrid.IsCurrentCellDirty Then
            CatalogDataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub CatalogDataGrid_CellValueChanged(
        sender As Object,
        e As DataGridViewCellEventArgs
    ) Handles CatalogDataGrid.CellValueChanged
        If _updatingCategoryCells OrElse e.RowIndex < 0 Then Return
        If e.ColumnIndex <> BreakfastColumn.Index AndAlso
            e.ColumnIndex <> BrunchColumn.Index Then
            Return
        End If

        Dim row = CatalogDataGrid.Rows(e.RowIndex)
        Dim breakfastSelected = Convert.ToBoolean(
            If(row.Cells(BreakfastColumn.Index).Value, False)
        )
        Dim brunchSelected = Convert.ToBoolean(
            If(row.Cells(BrunchColumn.Index).Value, False)
        )
        If Not breakfastSelected OrElse brunchSelected Then Return

        _updatingCategoryCells = True
        Try
            row.Cells(BrunchColumn.Index).Value = True
        Finally
            _updatingCategoryCells = False
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As EventArgs) Handles SaveButton.Click
        CatalogDataGrid.EndEdit()

        For Each row As DataGridViewRow In CatalogDataGrid.Rows
            Dim meal = TryCast(row.Tag, Meal)
            If meal Is Nothing Then Continue For

            Dim mealTypes As New List(Of String)
            AddCheckedMealType(row, BreakfastColumn, "Breakfast", mealTypes)
            AddCheckedMealType(row, BrunchColumn, "Brunch", mealTypes)
            AddCheckedMealType(row, LunchColumn, "Lunch", mealTypes)
            AddCheckedMealType(row, DinnerColumn, "Dinner", mealTypes)
            AddCheckedMealType(row, SnackColumn, "Snack", mealTypes)

            If mealTypes.Count = 0 Then
                MessageBox.Show(
                    meal.Name & " must have at least one meal type.",
                    "Recipe categories",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                CatalogDataGrid.CurrentCell = row.Cells(BreakfastColumn.Index)
                Return
            End If

            meal.SetMealTypes(mealTypes)
        Next

        MealRepository.MergeAll(_meals)
        MealRepository.SaveCurrentCategoryVersion()
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Shared Sub AddCheckedMealType(
        row As DataGridViewRow,
        column As DataGridViewColumn,
        mealType As String,
        destination As List(Of String)
    )
        If Convert.ToBoolean(If(row.Cells(column.Index).Value, False)) Then
            destination.Add(mealType)
        End If
    End Sub

    Private Sub ViewDetailsButton_Click(
        sender As Object,
        e As EventArgs
    ) Handles ViewDetailsButton.Click
        ShowSelectedRecipe()
    End Sub

    Private Sub CatalogDataGrid_CellDoubleClick(
        sender As Object,
        e As DataGridViewCellEventArgs
    ) Handles CatalogDataGrid.CellDoubleClick
        If e.RowIndex >= 0 Then ShowSelectedRecipe()
    End Sub

    Private Sub ShowSelectedRecipe()
        If CatalogDataGrid.CurrentRow Is Nothing Then Return
        Dim meal = TryCast(CatalogDataGrid.CurrentRow.Tag, Meal)
        If meal Is Nothing Then Return

        Using details As New RecipeView(meal)
            details.ShowDialog(Me)
        End Using
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub
End Class
