Imports System.Globalization
Imports System.Text.RegularExpressions

Public Class RecipeIngredient
    Private Shared ReadOnly ParentheticalPurposePattern As New Regex(
        "\s*[\(\[]\s*(?:" &
        "(?:optional|divided)\b.*|" &
        "(?:plus\s+more\s+)?for\b.+|" &
        "to\s+(?:serve|garnish|finish|taste|season|coat|fry|grease)\b.*|" &
        "as\s+needed" &
        ")\s*[\)\]]\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly SeparatedPurposePattern As New Regex(
        "\s*(?:,|[-–—])\s*(?:" &
        "(?:optional|divided)\b.*|" &
        "(?:plus\s+more\s+)?for\b.+|" &
        "to\s+(?:serve|garnish|finish|taste|season|coat|fry|grease)\b.*|" &
        "as\s+needed" &
        ")\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly PlainPurposePattern As New Regex(
        "\s+(?:" &
        "(?:plus\s+more\s+)?for\s+(?:the\s+)?(?:" &
        "filling|mash|serving|garnish(?:ing)?|finishing|topping|" &
        "sauce|dough|crust|coating|(?:deep[-\s]+)?frying|cooking|" &
        "assembly|marinade|dressing|batter|brine|greasing|pan|" &
        "dusting|boiling|roasting|baking|pasta\s+water" &
        ")\b.*|" &
        "to\s+(?:serve|garnish|finish|taste|season|coat|fry|grease)\b.*|" &
        "as\s+needed" &
        ")\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly ParentheticalPackagePattern As New Regex(
        "\s*\((?<detail>[^)]*\d[^)]*(?:" &
        "fluid\s+ounces?|ounces?|oz|pounds?|lbs?|grams?|kilograms?|kg|" &
        "count|cans?|packages?|pieces?|sheets?|inches?|inch" &
        ")[^)]*)\)\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly LeadingPackagePattern As New Regex(
        "^(?<detail>\d+(?:\.\d+)?\s*[- ]\s*(?:" &
        "fluid\s+ounces?|ounces?|oz|pounds?|lbs?|grams?|kilograms?|kg" &
        "))\s+(?<name>.+)$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly TrailingPackagePattern As New Regex(
        "^(?<name>.+?)\s+(?<detail>\d+(?:\.\d+)?\s*[- ]?\s*(?:" &
        "fluid\s+ounces?|ounces?|oz|pounds?|lbs?|grams?|kilograms?|kg|" &
        "count)(?:\s+(?:can|package|piece|sheet))?)\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly TrailingSizePattern As New Regex(
        "^(?<name>.+?),\s*(?<detail>small|medium|large)\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )
    Private Shared ReadOnly LeadingSizePattern As New Regex(
        "^(?<detail>small|medium|large)\s+(?<name>.+)$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled
    )

    Public Property Ingredient As String
    Public Property Details As String
    Public Property Amount As String
    Public Property MinAmount As Double?
    Public Property MaxAmount As Double?
    Public Property Measurement As String
    Public Property OriginalMeasurement As String

    ' Quantity and Unit only exist as backwards-compatible JSON aliases. New
    ' files store the full range and the source measurement instead.
    Public Property Quantity As Double?
        Get
            Return MinAmount
        End Get
        Set(value As Double?)
            MinAmount = NormalizeAmount(value)
            If MinAmount.HasValue AndAlso Not MaxAmount.HasValue Then
                MaxAmount = MinAmount
            End If
        End Set
    End Property

    Public Property Unit As String
        Get
            Return Measurement
        End Get
        Set(value As String)
            Measurement = IngredientMeasurementConverter.NormalizeUnit(value)
        End Set
    End Property

    Public Function ShouldSerializeQuantity() As Boolean
        Return False
    End Function

    Public Function ShouldSerializeUnit() As Boolean
        Return False
    End Function

    <Newtonsoft.Json.JsonConstructor>
    Public Sub New(
        ingredient As String,
        Optional amount As String = Nothing,
        Optional quantity As Double? = Nothing,
        Optional unit As String = Nothing,
        Optional details As String = Nothing,
        Optional minAmount As Double? = Nothing,
        Optional maxAmount As Double? = Nothing,
        Optional measurement As String = Nothing,
        Optional originalMeasurement As String = Nothing
    )
        Dim inferredDetails = String.Empty
        Me.Ingredient = NormalizeIngredientIdentity(
            ingredient,
            inferredDetails
        )
        Me.Details = MergeDetails(details, inferredDetails)
        Me.Amount = If(amount, String.Empty).Trim()
        Me.MinAmount = NormalizeAmount(If(minAmount, quantity))
        Me.MaxAmount = NormalizeAmount(maxAmount)
        If Me.MinAmount.HasValue AndAlso Not Me.MaxAmount.HasValue Then
            Me.MaxAmount = Me.MinAmount
        End If
        If Me.MinAmount.HasValue AndAlso Me.MaxAmount.HasValue AndAlso
            Me.MaxAmount.Value < Me.MinAmount.Value Then
            Me.MaxAmount = Me.MinAmount
        End If

        Dim sourceMeasurement = If(
            String.IsNullOrWhiteSpace(measurement),
            unit,
            measurement
        )
        Dim hasSourceMeasurement =
            Not String.IsNullOrWhiteSpace(sourceMeasurement)
        Me.Measurement =
            IngredientMeasurementConverter.NormalizeUnit(sourceMeasurement)
        Dim hasOriginalMeasurement = originalMeasurement IsNot Nothing
        Me.OriginalMeasurement = If(
            hasOriginalMeasurement,
            originalMeasurement,
            If(sourceMeasurement, String.Empty)
        ).Trim()

        If Not Me.MinAmount.HasValue AndAlso Me.Amount <> String.Empty Then
            Dim parsedMin As Double
            Dim parsedMax As Double
            Dim parsedMeasurement As String = Nothing
            If IngredientMeasurementConverter.TryParseLegacyAmountRange(
                Me.Amount,
                parsedMin,
                parsedMax,
                parsedMeasurement
            ) Then
                If parsedMeasurement <> "to taste" AndAlso
                    parsedMeasurement <> "none" Then
                    Me.MinAmount = parsedMin
                    Me.MaxAmount = parsedMax
                End If
                If Me.Measurement = String.Empty Then
                    Me.Measurement = parsedMeasurement
                End If
                If Me.OriginalMeasurement = String.Empty AndAlso
                    Not hasOriginalMeasurement Then
                    Me.OriginalMeasurement = parsedMeasurement
                End If
            End If
        End If
        If Me.Measurement = String.Empty Then
            If Not hasSourceMeasurement AndAlso Me.MinAmount.HasValue Then
                Me.Measurement = If(
                    Me.MaxAmount.GetValueOrDefault() > 0,
                    "piece",
                    "none"
                )
            ElseIf Not hasSourceMeasurement AndAlso Me.Amount = String.Empty Then
                Me.Measurement = "none"
            End If
        End If
        If Me.Measurement = "to taste" OrElse Me.Measurement = "none" Then
            Me.MinAmount = Nothing
            Me.MaxAmount = Nothing
        End If
        If Me.OriginalMeasurement = String.Empty AndAlso
            Not hasOriginalMeasurement Then
            Me.OriginalMeasurement = Me.Measurement
        End If
        If Me.Amount = String.Empty AndAlso Me.MinAmount.HasValue Then
            Me.Amount = IngredientMeasurementConverter.FormatSourceAmount(
                Me.MinAmount.Value,
                Me.MaxAmount.Value
            )
        End If
        ApplyIdentityBearingDetails()
        ApplyMeasurementIdentityRules()
    End Sub

    Public Function HasStructuredMeasurement() As Boolean
        If Not IngredientMeasurementConverter.IsSupportedUnit(Measurement) Then
            Return False
        End If
        If Measurement = "to taste" OrElse Measurement = "none" Then
            Return True
        End If
        Return MinAmount.HasValue AndAlso MaxAmount.HasValue AndAlso
            MinAmount.Value >= 0 AndAlso MaxAmount.Value >= MinAmount.Value
    End Function

    Public Function TryGetAmountRange(
        ByRef minimum As Double,
        ByRef maximum As Double
    ) As Boolean
        minimum = 0
        maximum = 0
        If Not MinAmount.HasValue OrElse Not MaxAmount.HasValue Then
            Return False
        End If
        minimum = MinAmount.Value
        maximum = MaxAmount.Value
        Return maximum >= minimum
    End Function

    Public Function DisplayAmount(Optional system As String = Nothing) As String
        Return IngredientMeasurementConverter.FormatAmount(Me, system)
    End Function

    Public Function DisplayName() As String
        If String.IsNullOrWhiteSpace(Details) Then Return Ingredient
        Return Ingredient & " (" & Details & ")"
    End Function

    Public Function Clone() As RecipeIngredient
        Return New RecipeIngredient(
            Ingredient,
            amount:=Amount,
            details:=Details,
            minAmount:=MinAmount,
            maxAmount:=MaxAmount,
            measurement:=Measurement,
            originalMeasurement:=OriginalMeasurement
        )
    End Function

    Private Shared Function NormalizeAmount(value As Double?) As Double?
        If Not value.HasValue OrElse
            Double.IsNaN(value.Value) OrElse
            Double.IsInfinity(value.Value) OrElse
            value.Value < 0 Then
            Return Nothing
        End If
        Return value
    End Function

    Public Shared Function NormalizeIngredientName(value As String) As String
        Dim ignoredDetails = String.Empty
        Return NormalizeIngredientIdentity(value, ignoredDetails)
    End Function

    Public Shared Function MergeDetails(
        ParamArray values As String()
    ) As String
        Dim merged As New List(Of String)
        For Each value In If(values, Array.Empty(Of String)())
            For Each part In If(value, String.Empty).Split(";"c)
                Dim normalized = NormalizeIngredientDetails(part)
                If normalized = String.Empty OrElse
                    merged.Any(
                        Function(existing)
                            Return String.Equals(
                                existing,
                                normalized,
                                StringComparison.CurrentCultureIgnoreCase
                            )
                        End Function
                    ) Then
                    Continue For
                End If
                merged.Add(normalized)
            Next
        Next
        Return String.Join("; ", merged)
    End Function

    Private Shared Function NormalizeIngredientIdentity(
        value As String,
        ByRef inferredDetails As String
    ) As String
        Dim normalized = If(value, String.Empty).Trim()
        If normalized = String.Empty Then Return normalized
        normalized = StripPurposeQualifiers(normalized)
        If normalized = String.Empty Then Return normalized
        ExtractNonIdentityDetails(normalized, inferredDetails)
        normalized = NormalizeCapitalization(normalized)
        ApplyKnownAlias(normalized, inferredDetails)
        Return NormalizeCapitalization(normalized)
    End Function

    Private Shared Function NormalizeCapitalization(value As String) As String
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

    Private Shared Function NormalizeIngredientDetails(value As String) As String
        Dim normalized = Regex.Replace(
            If(value, String.Empty).Trim(),
            "\s+",
            " "
        )
        normalized = normalized.Trim(
            " "c,
            ","c,
            ";"c,
            ":"c,
            "-"c,
            "–"c,
            "—"c,
            "("c,
            ")"c,
            "["c,
            "]"c
        )
        If normalized = String.Empty Then Return normalized

        Return normalized.ToLower(CultureInfo.CurrentCulture)
    End Function

    Private Shared Sub ExtractNonIdentityDetails(
        ByRef ingredientName As String,
        ByRef inferredDetails As String
    )
        Dim match = ParentheticalPackagePattern.Match(ingredientName)
        If match.Success Then
            inferredDetails = MergeDetails(
                inferredDetails,
                match.Groups("detail").Value
            )
            ingredientName = ParentheticalPackagePattern.Replace(
                ingredientName,
                String.Empty
            ).Trim()
        End If

        match = LeadingPackagePattern.Match(ingredientName)
        If match.Success Then
            inferredDetails = MergeDetails(
                inferredDetails,
                match.Groups("detail").Value
            )
            ingredientName = match.Groups("name").Value.Trim()
        End If

        match = TrailingPackagePattern.Match(ingredientName)
        If match.Success Then
            inferredDetails = MergeDetails(
                inferredDetails,
                match.Groups("detail").Value
            )
            ingredientName = match.Groups("name").Value.Trim()
        End If

        match = TrailingSizePattern.Match(ingredientName)
        If match.Success Then
            inferredDetails = MergeDetails(
                inferredDetails,
                match.Groups("detail").Value
            )
            ingredientName = match.Groups("name").Value.Trim()
        Else
            match = LeadingSizePattern.Match(ingredientName)
            If match.Success Then
                inferredDetails = MergeDetails(
                    inferredDetails,
                    match.Groups("detail").Value
                )
                ingredientName = match.Groups("name").Value.Trim()
            End If
        End If
    End Sub

    Private Shared Sub ApplyKnownAlias(
        ByRef ingredientName As String,
        ByRef inferredDetails As String
    )
        Select Case ingredientName.ToLowerInvariant()
            Case "pepper"
                ingredientName = "Black Pepper"
            Case "ground black pepper"
                ingredientName = "Black Pepper"
                inferredDetails = MergeDetails(inferredDetails, "ground")
            Case "ground cinnamon"
                ingredientName = "Cinnamon"
                inferredDetails = MergeDetails(inferredDetails, "ground")
            Case "ground clove"
                ingredientName = "Clove"
                inferredDetails = MergeDetails(inferredDetails, "ground")
            Case "ground cumin"
                ingredientName = "Cumin"
                inferredDetails = MergeDetails(inferredDetails, "ground")
            Case "bite-sized salad greens"
                ingredientName = "Salad Greens"
                inferredDetails = MergeDetails(
                    inferredDetails,
                    "bite-sized"
                )
            Case "cherry tomatoes"
                ingredientName = "Cherry Tomato"
            Case "condensed cream of chicken soup, reduced-fat and reduced-sodium"
                ingredientName =
                    "Reduced-Fat, Reduced-Sodium Condensed Cream of Chicken Soup"
            Case "corn tortilla, small 4-inch, no salt added"
                ingredientName = "No-Salt-Added Corn Tortilla"
                inferredDetails = MergeDetails(
                    inferredDetails,
                    "small 4-inch"
                )
            Case "dry orzo pasta"
                ingredientName = "Orzo Pasta"
                inferredDetails = MergeDetails(inferredDetails, "dry")
            Case "flour"
                ingredientName = "All-Purpose Flour"
            Case "fresh pomegranate seed", "fresh pomegranate seeds"
                ingredientName = "Pomegranate Seed"
                inferredDetails = MergeDetails(inferredDetails, "fresh")
            Case "frozen peas"
                ingredientName = "Frozen Pea"
            Case "ginger", "ginger root"
                ingredientName = "Fresh Ginger"
            Case "green pepper"
                ingredientName = "Green Bell Pepper"
            Case "ground oats"
                ingredientName = "Ground Oat"
            Case "kalamata olives"
                ingredientName = "Kalamata Olive"
            Case "low-sodium chickpeas"
                ingredientName = "Low-Sodium Chickpea"
            Case "mini phyllo cups"
                ingredientName = "Mini Phyllo Cup"
            Case "pomegranate seeds"
                ingredientName = "Pomegranate Seed"
            Case "puff dough"
                ingredientName = "Puff Pastry Dough"
            Case "red pepper flakes"
                ingredientName = "Red Pepper Flake"
            Case "rolled oats"
                ingredientName = "Rolled Oat"
            Case "seasoned bread crumbs"
                ingredientName = "Seasoned Bread Crumb"
            Case "sesame seeds"
                ingredientName = "Sesame Seed"
            Case "skinless chicken breast"
                ingredientName = "Chicken Breast"
                inferredDetails = MergeDetails(
                    inferredDetails,
                    "skinless"
                )
            Case "whole egg"
                ingredientName = "Egg"
            Case "all-purpose white flour"
                ingredientName = "All-Purpose Flour"
            Case "fresh basil"
                ingredientName = "Basil"
                inferredDetails = MergeDetails(inferredDetails, "fresh")
            Case "fresh cilantro"
                ingredientName = "Cilantro"
                inferredDetails = MergeDetails(inferredDetails, "fresh")
            Case "fresh parsley"
                ingredientName = "Parsley"
                inferredDetails = MergeDetails(inferredDetails, "fresh")
            Case "fresh rosemary"
                ingredientName = "Rosemary"
                inferredDetails = MergeDetails(inferredDetails, "fresh")
            Case "fresh thyme"
                ingredientName = "Thyme"
                inferredDetails = MergeDetails(inferredDetails, "fresh")
            Case "spring onion"
                ingredientName = "Green Onion"
            Case "sun-dried tomatoes"
                ingredientName = "Sun-Dried Tomato"
            Case "sweet yellow corn kernels"
                ingredientName = "Sweet Yellow Corn Kernel"
            Case "yellow sugar-free cake mix"
                ingredientName = "Sugar-Free Yellow Cake Mix"
            Case "plain cream cheese"
                ingredientName = "Cream Cheese"
            Case "lite soy sauce"
                ingredientName = "Reduced-Sodium Soy Sauce"
            Case "vegetable cooking spray", "vegetable oil spray"
                ingredientName = "Cooking Spray"
        End Select
    End Sub

    Private Sub ApplyIdentityBearingDetails()
        If (
            String.Equals(
                Ingredient,
                "Fresh Ginger",
                StringComparison.OrdinalIgnoreCase
            ) OrElse
            String.Equals(
                Ingredient,
                "Ginger",
                StringComparison.OrdinalIgnoreCase
            )
        ) AndAlso Regex.IsMatch(
            Details,
            "\bground\b",
            RegexOptions.IgnoreCase
        ) Then
            Ingredient = "Ground Ginger"
            Details = RemoveDetailPhrase(Details, "ground")
            Return
        End If

        If String.Equals(
            Ingredient,
            "Salt",
            StringComparison.OrdinalIgnoreCase
        ) Then
            Dim saltVariant = Regex.Match(
                Details,
                "\b(?:kosher|sea|table)\b",
                RegexOptions.IgnoreCase
            )
            If saltVariant.Success Then
                Select Case saltVariant.Value.ToLowerInvariant()
                    Case "kosher"
                        Ingredient = "Kosher Salt"
                    Case "sea"
                        Ingredient = "Sea Salt"
                    Case "table"
                        Ingredient = "Table Salt"
                End Select
                Details = RemoveDetailPhrase(
                    Details,
                    saltVariant.Value
                )
            End If
            Return
        End If

        Dim soupQualifiers = Regex.Match(
            Details,
            "\breduced[- ]fat\b\s*(?:,|;|and)?\s*" &
                "\breduced[- ]sodium\b|" &
                "\breduced[- ]sodium\b\s*(?:,|;|and)?\s*" &
                "\breduced[- ]fat\b",
            RegexOptions.IgnoreCase
        )
        If String.Equals(
            Ingredient,
            "Condensed Cream of Chicken Soup",
            StringComparison.OrdinalIgnoreCase
        ) AndAlso soupQualifiers.Success Then
            Ingredient =
                "Reduced-Fat, Reduced-Sodium Condensed Cream of Chicken Soup"
            Details = RemoveDetailPhrase(
                Details,
                soupQualifiers.Value
            )
        End If

        If String.Equals(
            Ingredient,
            "Mexican Cheese Blend",
            StringComparison.OrdinalIgnoreCase
        ) AndAlso Details.IndexOf(
            "2%",
            StringComparison.OrdinalIgnoreCase
        ) >= 0 Then
            Ingredient = "2% Mexican Cheese Blend"
            Details = RemoveDetailPhrase(Details, "2%")
        End If

        Dim noSaltAdded = Regex.Match(
            Details,
            "\bno[- ]salt[- ]added\b",
            RegexOptions.IgnoreCase
        )
        If String.Equals(
            Ingredient,
            "Corn Tortilla",
            StringComparison.OrdinalIgnoreCase
        ) AndAlso noSaltAdded.Success Then
            Ingredient = "No-Salt-Added Corn Tortilla"
            Details = RemoveDetailPhrase(
                Details,
                noSaltAdded.Value
            )
        End If
    End Sub

    Private Sub ApplyMeasurementIdentityRules()
        If Not String.Equals(
            Measurement,
            "piece",
            StringComparison.OrdinalIgnoreCase
        ) Then
            Return
        End If

        If String.Equals(
            Ingredient,
            "Lemon Juice",
            StringComparison.OrdinalIgnoreCase
        ) Then
            Ingredient = "Lemon"
            Details = MergeDetails(Details, "juiced")
        ElseIf String.Equals(
            Ingredient,
            "Lime Juice",
            StringComparison.OrdinalIgnoreCase
        ) Then
            Ingredient = "Lime"
            Details = MergeDetails(Details, "juiced")
        End If
    End Sub

    Private Shared Function RemoveDetailPhrase(
        details As String,
        phrase As String
    ) As String
        Dim normalized = Regex.Replace(
            If(details, String.Empty),
            Regex.Escape(phrase),
            String.Empty,
            RegexOptions.IgnoreCase
        )
        normalized = Regex.Replace(
            normalized,
            "\s+(?:and|or)\s*$",
            String.Empty,
            RegexOptions.IgnoreCase
        )
        Return NormalizeIngredientDetails(normalized)
    End Function

    Private Shared Function StripPurposeQualifiers(value As String) As String
        Dim normalized = value.Trim()
        Do
            Dim previous = normalized
            normalized = ParentheticalPurposePattern.Replace(
                normalized,
                String.Empty
            ).Trim()
            normalized = SeparatedPurposePattern.Replace(
                normalized,
                String.Empty
            ).Trim()
            normalized = PlainPurposePattern.Replace(
                normalized,
                String.Empty
            ).Trim()
            If String.Equals(
                previous,
                normalized,
                StringComparison.Ordinal
            ) Then
                Exit Do
            End If
        Loop
        Return normalized.TrimEnd(","c, ";"c, ":"c, "-"c, "–"c, "—"c).Trim()
    End Function
End Class
