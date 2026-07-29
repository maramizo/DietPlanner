Imports System.Globalization

Public Class RecipeIngredient
    Public Property Ingredient As String
    Public Property Amount As String

    <Newtonsoft.Json.JsonConstructor>
    Public Sub New(ingredient As String, amount As String)
        Me.Ingredient = NormalizeIngredientName(ingredient)
        Me.Amount = If(amount, String.Empty).Trim()
    End Sub

    Private Shared Function NormalizeIngredientName(value As String) As String
        Dim normalized = If(value, String.Empty).Trim()
        If normalized = String.Empty Then Return normalized

        Dim letters = normalized.Where(Function(character) Char.IsLetter(character)).ToList()
        Dim isUniformCase =
            letters.Count > 0 AndAlso
            (
                letters.All(Function(character) Char.IsLower(character)) OrElse
                letters.All(Function(character) Char.IsUpper(character))
            )
        If isUniformCase Then
            Return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                normalized.ToLower(CultureInfo.CurrentCulture)
            )
        End If

        If Char.IsLower(normalized(0)) Then
            normalized =
                Char.ToUpper(normalized(0), CultureInfo.CurrentCulture) &
                normalized.Substring(1)
        End If
        Return normalized
    End Function
End Class
