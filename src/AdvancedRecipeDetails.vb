Public Class AdvancedRecipeDetails
    Public ReadOnly Property Ingredients As List(Of RecipeIngredient)
    Public ReadOnly Property PreparationMethod As String

    Public Sub New(
        ingredients As IEnumerable(Of RecipeIngredient),
        preparationMethod As String
    )
        Me.Ingredients = If(
            ingredients,
            Enumerable.Empty(Of RecipeIngredient)()
        ).ToList()
        Me.PreparationMethod = If(preparationMethod, String.Empty).Trim()
    End Sub
End Class
