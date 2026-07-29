Public Class AdvancedRecipeDetails
    Public ReadOnly Property Ingredients As List(Of RecipeIngredient)
    Public ReadOnly Property PreparationMethod As String
    Public ReadOnly Property Notes As String
    Public ReadOnly Property Servings As Integer
    Public ReadOnly Property CaloriesPerServing As Integer

    Public Sub New(
        ingredients As IEnumerable(Of RecipeIngredient),
        preparationMethod As String,
        notes As String,
        servings As Integer,
        caloriesPerServing As Integer
    )
        Me.Ingredients = If(
            ingredients,
            Enumerable.Empty(Of RecipeIngredient)()
        ).ToList()
        Me.PreparationMethod = If(preparationMethod, String.Empty).Trim()
        Me.Notes = If(notes, String.Empty).Trim()
        Me.Servings = servings
        Me.CaloriesPerServing = caloriesPerServing
    End Sub
End Class
