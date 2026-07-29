Public Class Meal
    Public Const AdvancedScrapePending As String = "Pending"
    Public Const AdvancedScrapeComplete As String = "Complete"
    Public Const AdvancedScrapeUnavailable As String = "Unavailable"
    Public Const CurrentAdvancedScrapeVersion As Integer = 1
    Public Const CurrentIngredientDataVersion As Integer = 2

    Private Shared ReadOnly MealTypeOrder As String() = {
        "Breakfast",
        "Brunch",
        "Lunch",
        "Dinner",
        "Snack"
    }

    Public Property Name As String
    Public Property Calory As Integer
    Public Property Nutritionals As Dictionary(Of String, Double)
    Public Property Recipe As String
    Public Property Servings As Integer
    Public Property PrepTime As Integer
    Public Property CookTime As Integer
    Public Property TotalTime As Integer
    Public Property MealTypes As List(Of String)
    Public Property Ingredients As List(Of RecipeIngredient)
    Public Property PreparationMethod As String
    Public Property Notes As String
    Public Property AdvancedScrapeStatus As String
    Public Property AdvancedScrapeNote As String
    Public Property AdvancedScrapeVersion As Integer
    Public Property IngredientDataVersion As Integer

    <Newtonsoft.Json.JsonConstructor>
    Public Sub New(
        name As String,
        calory As Integer,
        nutritionals As Dictionary(Of String, Double),
        recipe As String,
        Optional servings As Integer = 0,
        Optional prepTime As Integer = 0,
        Optional cookTime As Integer = 0,
        Optional mealTypes As IEnumerable(Of String) = Nothing,
        Optional ingredients As IEnumerable(Of RecipeIngredient) = Nothing,
        Optional preparationMethod As String = Nothing,
        Optional notes As String = Nothing,
        Optional advancedScrapeStatus As String = Nothing,
        Optional advancedScrapeNote As String = Nothing,
        Optional advancedScrapeVersion As Integer = 0,
        Optional ingredientDataVersion As Integer = 0
    )
        Me.Name = name
        Me.Calory = calory
        Me.Nutritionals = ParseNutritionals(nutritionals)
        Me.Recipe = recipe
        Me.Servings = Math.Max(0, servings)
        Me.PrepTime = prepTime
        Me.CookTime = cookTime
        Me.TotalTime = prepTime + cookTime
        SetMealTypes(mealTypes)
        Me.Ingredients = NormalizeIngredients(ingredients)
        Me.PreparationMethod = If(preparationMethod, String.Empty).Trim()
        Me.Notes = If(notes, String.Empty).Trim()
        Me.AdvancedScrapeStatus = NormalizeAdvancedScrapeStatus(advancedScrapeStatus)
        Me.AdvancedScrapeNote = If(advancedScrapeNote, String.Empty).Trim()
        Me.AdvancedScrapeVersion = Math.Max(0, advancedScrapeVersion)
        Me.IngredientDataVersion = Math.Max(0, ingredientDataVersion)
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
        If String.Equals(
            AdvancedScrapeStatus,
            AdvancedScrapePending,
            StringComparison.Ordinal
        ) Then
            Return True
        End If

        Return String.Equals(
            AdvancedScrapeStatus,
            AdvancedScrapeComplete,
            StringComparison.Ordinal
        ) AndAlso (
            AdvancedScrapeVersion < CurrentAdvancedScrapeVersion OrElse
            Servings < 1
        )
    End Function

    Public Function IsAdvancedScrapeUnavailable() As Boolean
        Return String.Equals(
            AdvancedScrapeStatus,
            AdvancedScrapeUnavailable,
            StringComparison.Ordinal
        )
    End Function

    Public Function NeedsIngredientNormalization() As Boolean
        Return Ingredients IsNot Nothing AndAlso
            Ingredients.Count > 0 AndAlso
            (
                IngredientDataVersion < CurrentIngredientDataVersion OrElse
                Ingredients.Any(
                    Function(ingredient)
                        Return ingredient Is Nothing OrElse
                            Not ingredient.HasStructuredMeasurement()
                    End Function
                )
            )
    End Function

    Public Sub ApplyAdvancedDetails(details As AdvancedRecipeDetails)
        If details Is Nothing Then Throw New ArgumentNullException(NameOf(details))
        If details.Servings < 1 Then
            Throw New ArgumentOutOfRangeException(
                NameOf(details),
                "Serving count must be positive."
            )
        End If
        If details.CaloriesPerServing < 0 Then
            Throw New ArgumentOutOfRangeException(
                NameOf(details),
                "Calories per serving cannot be negative."
            )
        End If

        Ingredients = NormalizeIngredients(details.Ingredients)
        PreparationMethod = If(details.PreparationMethod, String.Empty).Trim()
        Notes = If(details.Notes, String.Empty).Trim()
        Servings = details.Servings
        Calory = details.CaloriesPerServing
        AdvancedScrapeStatus = AdvancedScrapeComplete
        AdvancedScrapeNote = String.Empty
        AdvancedScrapeVersion = CurrentAdvancedScrapeVersion
        IngredientDataVersion = CurrentIngredientDataVersion
    End Sub

    Public Sub ApplyNormalizedIngredients(
        normalizedIngredients As IEnumerable(Of RecipeIngredient)
    )
        Dim normalized = NormalizeIngredients(normalizedIngredients)
        If normalized.Count = 0 Then
            Throw New ArgumentException(
                "Ingredient normalization returned no ingredients.",
                NameOf(normalizedIngredients)
            )
        End If
        If normalized.Any(
            Function(ingredient) Not ingredient.HasStructuredMeasurement()
        ) Then
            Throw New ArgumentException(
                "Ingredient normalization returned an unsupported measurement.",
                NameOf(normalizedIngredients)
            )
        End If

        Ingredients = normalized
        IngredientDataVersion = CurrentIngredientDataVersion
    End Sub

    Public Sub MarkAdvancedScrapeUnavailable(note As String)
        AdvancedScrapeStatus = AdvancedScrapeUnavailable
        AdvancedScrapeNote = If(note, String.Empty).Trim()
        AdvancedScrapeVersion = CurrentAdvancedScrapeVersion
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
        Return IngredientMeasurementConverter.ConsolidateIngredients(
            ingredients
        )
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
