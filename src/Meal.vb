Public Class Meal
    Public Const AdvancedScrapePending As String = "Pending"
    Public Const AdvancedScrapeComplete As String = "Complete"
    Public Const AdvancedScrapeUnavailable As String = "Unavailable"

    Private Shared ReadOnly MealTypeOrder As String() = {
        "Breakfast",
        "Lunch",
        "Brunch",
        "Dinner",
        "Snack"
    }

    Public Property Name As String
    Public Property Calory As Integer
    Public Property Nutritionals As Dictionary(Of String, Double)
    Public Property Recipe As String
    Public Property PrepTime As Integer
    Public Property CookTime As Integer
    Public Property TotalTime As Integer
    Public Property MealTypes As List(Of String)
    Public Property Ingredients As List(Of RecipeIngredient)
    Public Property PreparationMethod As String
    Public Property AdvancedScrapeStatus As String
    Public Property AdvancedScrapeNote As String

    <Newtonsoft.Json.JsonConstructor>
    Public Sub New(
        name As String,
        calory As Integer,
        nutritionals As Dictionary(Of String, Double),
        recipe As String,
        Optional prepTime As Integer = 0,
        Optional cookTime As Integer = 0,
        Optional mealTypes As IEnumerable(Of String) = Nothing,
        Optional ingredients As IEnumerable(Of RecipeIngredient) = Nothing,
        Optional preparationMethod As String = Nothing,
        Optional advancedScrapeStatus As String = Nothing,
        Optional advancedScrapeNote As String = Nothing
    )
        Me.Name = name
        Me.Calory = calory
        Me.Nutritionals = ParseNutritionals(nutritionals)
        Me.Recipe = recipe
        Me.PrepTime = prepTime
        Me.CookTime = cookTime
        Me.TotalTime = prepTime + cookTime
        SetMealTypes(mealTypes)
        Me.Ingredients = NormalizeIngredients(ingredients)
        Me.PreparationMethod = If(preparationMethod, String.Empty).Trim()
        Me.AdvancedScrapeStatus = NormalizeAdvancedScrapeStatus(advancedScrapeStatus)
        Me.AdvancedScrapeNote = If(advancedScrapeNote, String.Empty).Trim()
    End Sub

    Public Sub SetMealTypes(mealTypes As IEnumerable(Of String))
        Me.MealTypes = NormalizeMealTypes(mealTypes)
    End Sub

    Public Function SupportsMealType(mealType As String) As Boolean
        If MealTypes Is Nothing OrElse MealTypes.Count = 0 Then Return True
        Return MealTypes.Any(
            Function(value) String.Equals(value, mealType, StringComparison.OrdinalIgnoreCase)
        )
    End Function

    Public Function NeedsAdvancedScrape() As Boolean
        Return String.Equals(
            AdvancedScrapeStatus,
            AdvancedScrapePending,
            StringComparison.Ordinal
        )
    End Function

    Public Function IsAdvancedScrapeUnavailable() As Boolean
        Return String.Equals(
            AdvancedScrapeStatus,
            AdvancedScrapeUnavailable,
            StringComparison.Ordinal
        )
    End Function

    Public Sub ApplyAdvancedDetails(details As AdvancedRecipeDetails)
        If details Is Nothing Then Throw New ArgumentNullException(NameOf(details))

        Ingredients = NormalizeIngredients(details.Ingredients)
        PreparationMethod = If(details.PreparationMethod, String.Empty).Trim()
        AdvancedScrapeStatus = AdvancedScrapeComplete
        AdvancedScrapeNote = String.Empty
    End Sub

    Public Sub MarkAdvancedScrapeUnavailable(note As String)
        AdvancedScrapeStatus = AdvancedScrapeUnavailable
        AdvancedScrapeNote = If(note, String.Empty).Trim()
    End Sub

    Private Shared Function NormalizeMealTypes(mealTypes As IEnumerable(Of String)) As List(Of String)
        Dim normalized As New List(Of String)
        If mealTypes Is Nothing Then Return normalized
        Dim includesBreakfast = mealTypes.Any(
            Function(value) String.Equals(
                value,
                "Breakfast",
                StringComparison.OrdinalIgnoreCase
            )
        )

        For Each optionName In MealTypeOrder
            If (
                String.Equals(optionName, "Brunch", StringComparison.Ordinal) AndAlso
                includesBreakfast
            ) OrElse mealTypes.Any(
                Function(value) String.Equals(value, optionName, StringComparison.OrdinalIgnoreCase)
            ) Then
                normalized.Add(optionName)
            End If
        Next
        Return normalized
    End Function

    Private Shared Function NormalizeIngredients(
        ingredients As IEnumerable(Of RecipeIngredient)
    ) As List(Of RecipeIngredient)
        Dim normalized As New List(Of RecipeIngredient)
        If ingredients Is Nothing Then Return normalized

        For Each ingredient In ingredients
            If ingredient Is Nothing OrElse String.IsNullOrWhiteSpace(ingredient.Ingredient) Then
                Continue For
            End If
            normalized.Add(
                New RecipeIngredient(ingredient.Ingredient, ingredient.Amount)
            )
        Next
        Return normalized
    End Function

    Private Function NormalizeAdvancedScrapeStatus(status As String) As String
        If String.IsNullOrWhiteSpace(status) Then
            If Ingredients.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(PreparationMethod) Then
                Return AdvancedScrapeComplete
            End If
            Return AdvancedScrapePending
        End If

        If String.Equals(status, AdvancedScrapeComplete, StringComparison.OrdinalIgnoreCase) Then
            Return AdvancedScrapeComplete
        End If
        If String.Equals(status, AdvancedScrapeUnavailable, StringComparison.OrdinalIgnoreCase) Then
            Return AdvancedScrapeUnavailable
        End If
        Return AdvancedScrapePending
    End Function

    Public Function ParseNutritionals(
        nutritionals As Dictionary(Of String, Double)
    ) As Dictionary(Of String, Double)
        Dim returnedNutritionals As New Dictionary(Of String, Double)
        If nutritionals Is Nothing Then Return returnedNutritionals

        For Each unparsedNutritional As KeyValuePair(Of String, Double) In nutritionals
            Dim nutritional = New Nutrition(unparsedNutritional.Key, unparsedNutritional.Value)
            returnedNutritionals.Add(nutritional.Name, unparsedNutritional.Value)
        Next

        Return returnedNutritionals
    End Function

    Public Function ViewNutritionals() As Dictionary(Of String, String)
        'Not all nutritionals are alike, some nutritionals show in mg, some in g:
        Dim returnedNutritionals As New Dictionary(Of String, String)

        For Each unparsedNutritional As KeyValuePair(Of String, Double) In Nutritionals
            Dim nutritional = New Nutrition(unparsedNutritional.Key, unparsedNutritional.Value)
            returnedNutritionals.Add(nutritional.Name, nutritional.FormattedAmount)
        Next

        Return returnedNutritionals
    End Function
End Class
