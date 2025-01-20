Public Class Meal
    Public Property Name As String
    Public Property Calory As Integer
    Public Property Nutritionals As Dictionary(Of String, Double)
    Public Property Recipe As String
    Public Property PrepTime As Integer
    Public Property CookTime As Integer
    Public Property TotalTime As Integer

    Public Sub New(name As String, calory As Integer, nutritionals As Dictionary(Of String, Double), recipe As String, Optional prepTime As Integer = 0, Optional cookTime As Integer = 0)
        Me.Name = name
        Me.Calory = calory
        Me.Nutritionals = ParseNutritionals(nutritionals)
        Me.Recipe = recipe
        Me.PrepTime = prepTime
        Me.CookTime = cookTime
        Me.TotalTime = prepTime + cookTime
    End Sub

    Public Function ParseNutritionals(nutritionals As Dictionary(Of String, Double))
        Dim returnedNutritionals As New Dictionary(Of String, Double)

        For Each unparsedNutritional As KeyValuePair(Of String, Double) In nutritionals
            Dim nutritional = New Nutrition(unparsedNutritional.Key, unparsedNutritional.Value)
            returnedNutritionals.Add(nutritional.Name, unparsedNutritional.Value)
        Next

        Return returnedNutritionals
    End Function

    Public Function ViewNutritionals()
        'Not all nutritionals are alike, some nutritionals show in mg, some in g:
        Dim returnedNutritionals As New Dictionary(Of String, String)

        For Each unparsedNutritional As KeyValuePair(Of String, Double) In Nutritionals
            Dim nutritional = New Nutrition(unparsedNutritional.Key, unparsedNutritional.Value)
            returnedNutritionals.Add(nutritional.Name, nutritional.FormattedAmount)
        Next

        Return returnedNutritionals
    End Function

End Class