Imports System.Globalization
Imports System.Text.RegularExpressions

Public NotInheritable Class IngredientMeasurementSystemOption
    Public ReadOnly Property Key As String
    Public ReadOnly Property DisplayName As String

    Public Sub New(key As String, displayName As String)
        Me.Key = key
        Me.DisplayName = displayName
    End Sub

    Public Overrides Function ToString() As String
        Return DisplayName
    End Function
End Class

Public NotInheritable Class IngredientAmountEntry
    Public ReadOnly Property Ingredient As RecipeIngredient
    Public ReadOnly Property Scale As Double

    Public Sub New(ingredient As RecipeIngredient, scale As Double)
        Me.Ingredient = ingredient
        Me.Scale = Math.Max(0, scale)
    End Sub
End Class

Public NotInheritable Class ConsolidatedIngredient
    Public ReadOnly Property Key As String
    Public ReadOnly Property Ingredient As String
    Public ReadOnly Property Dimension As String
    Public ReadOnly Property OriginalMeasurement As String
    Public ReadOnly Property MinimumBaseAmount As Double?
    Public ReadOnly Property MaximumBaseAmount As Double?
    Public ReadOnly Property LegacyAmount As String
    Public ReadOnly Property LegacyOccurrences As Integer
    Public ReadOnly Property DefaultMeasurement As String
    Public ReadOnly Property CompatibleMeasurements As IReadOnlyList(Of String)

    Public Sub New(
        key As String,
        ingredient As String,
        dimension As String,
        originalMeasurement As String,
        minimumBaseAmount As Double?,
        maximumBaseAmount As Double?,
        legacyAmount As String,
        legacyOccurrences As Integer,
        defaultMeasurement As String,
        compatibleMeasurements As IEnumerable(Of String)
    )
        Me.Key = key
        Me.Ingredient = ingredient
        Me.Dimension = dimension
        Me.OriginalMeasurement = originalMeasurement
        Me.MinimumBaseAmount = minimumBaseAmount
        Me.MaximumBaseAmount = maximumBaseAmount
        Me.LegacyAmount = If(legacyAmount, String.Empty)
        Me.LegacyOccurrences = Math.Max(0, legacyOccurrences)
        Me.DefaultMeasurement = defaultMeasurement
        Me.CompatibleMeasurements = New List(Of String)(
            If(compatibleMeasurements, Enumerable.Empty(Of String)())
        )
    End Sub

    Public ReadOnly Property Amount As String
        Get
            Return FormatAmount(DefaultMeasurement)
        End Get
    End Property

    Public Function FormatAmount(measurement As String) As String
        If String.Equals(Dimension, "legacy", StringComparison.Ordinal) Then
            If LegacyOccurrences > 1 AndAlso LegacyAmount <> String.Empty Then
                Return LegacyOccurrences.ToString(
                    CultureInfo.CurrentCulture
                ) & " × " & LegacyAmount
            End If
            Return LegacyAmount
        End If

        Return IngredientMeasurementConverter.FormatBaseAmountRange(
            MinimumBaseAmount,
            MaximumBaseAmount,
            Dimension,
            measurement
        )
    End Function
End Class

Public NotInheritable Class IngredientMeasurementConverter
    Public Const SourceUnitsSystem As String = "Source"
    Public Const UsCustomarySystem As String = "US"
    Public Const MetricSystem As String = "Metric"

    Public Shared ReadOnly SupportedUnits As String() = {
        "teaspoon",
        "tablespoon",
        "fluid ounce",
        "cup",
        "pint",
        "quart",
        "gallon",
        "milliliter",
        "liter",
        "milligram",
        "gram",
        "kilogram",
        "ounce",
        "pound",
        "piece",
        "clove",
        "slice",
        "can",
        "package",
        "bunch",
        "pinch",
        "dash",
        "to taste",
        "none"
    }

    Public Shared ReadOnly MeasurementSystems As IngredientMeasurementSystemOption() = {
        New IngredientMeasurementSystemOption(
            SourceUnitsSystem,
            "Source units (standardized)"
        ),
        New IngredientMeasurementSystemOption(
            UsCustomarySystem,
            "US customary"
        ),
        New IngredientMeasurementSystemOption(
            MetricSystem,
            "Metric"
        )
    }

    Private Shared ReadOnly DisplayFractionDenominators As Integer() = {
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        24
    }
    Private Const DisplayFractionTolerance As Double = 0.0005

    Private Shared ReadOnly UnitAliases As New Dictionary(Of String, String)(
        StringComparer.OrdinalIgnoreCase
    ) From {
        {"tsp", "teaspoon"},
        {"tsps", "teaspoon"},
        {"teaspoon", "teaspoon"},
        {"teaspoons", "teaspoon"},
        {"tbsp", "tablespoon"},
        {"tbsps", "tablespoon"},
        {"tbs", "tablespoon"},
        {"tablespoon", "tablespoon"},
        {"tablespoons", "tablespoon"},
        {"fl oz", "fluid ounce"},
        {"fluid oz", "fluid ounce"},
        {"fluid ounce", "fluid ounce"},
        {"fluid ounces", "fluid ounce"},
        {"cup", "cup"},
        {"cups", "cup"},
        {"pt", "pint"},
        {"pint", "pint"},
        {"pints", "pint"},
        {"qt", "quart"},
        {"quart", "quart"},
        {"quarts", "quart"},
        {"gal", "gallon"},
        {"gallon", "gallon"},
        {"gallons", "gallon"},
        {"ml", "milliliter"},
        {"millilitre", "milliliter"},
        {"millilitres", "milliliter"},
        {"milliliter", "milliliter"},
        {"milliliters", "milliliter"},
        {"l", "liter"},
        {"litre", "liter"},
        {"litres", "liter"},
        {"liter", "liter"},
        {"liters", "liter"},
        {"mg", "milligram"},
        {"milligram", "milligram"},
        {"milligrams", "milligram"},
        {"oz", "ounce"},
        {"ounce", "ounce"},
        {"ounces", "ounce"},
        {"lb", "pound"},
        {"lbs", "pound"},
        {"pound", "pound"},
        {"pounds", "pound"},
        {"g", "gram"},
        {"gram", "gram"},
        {"grams", "gram"},
        {"kg", "kilogram"},
        {"kilogram", "kilogram"},
        {"kilograms", "kilogram"},
        {"item", "piece"},
        {"items", "piece"},
        {"whole", "piece"},
        {"piece", "piece"},
        {"pieces", "piece"},
        {"clove", "clove"},
        {"cloves", "clove"},
        {"slice", "slice"},
        {"slices", "slice"},
        {"can", "can"},
        {"cans", "can"},
        {"pkg", "package"},
        {"package", "package"},
        {"packages", "package"},
        {"bunch", "bunch"},
        {"bunches", "bunch"},
        {"pinch", "pinch"},
        {"pinches", "pinch"},
        {"dash", "dash"},
        {"dashes", "dash"},
        {"to taste", "to taste"},
        {"as needed", "none"},
        {"none", "none"}
    }

    Private Shared ReadOnly MassFactors As New Dictionary(Of String, Double)(
        StringComparer.OrdinalIgnoreCase
    ) From {
        {"milligram", 0.001},
        {"gram", 1},
        {"kilogram", 1000},
        {"ounce", 28.349523125},
        {"pound", 453.59237}
    }

    Private Shared ReadOnly VolumeFactors As New Dictionary(Of String, Double)(
        StringComparer.OrdinalIgnoreCase
    ) From {
        {"milliliter", 1},
        {"liter", 1000},
        {"teaspoon", 4.92892159375},
        {"tablespoon", 14.78676478125},
        {"fluid ounce", 29.5735295625},
        {"cup", 236.5882365},
        {"pint", 473.176473},
        {"quart", 946.352946},
        {"gallon", 3785.411784}
    }

    Private Sub New()
    End Sub

    Public Shared Function NormalizeSystem(value As String) As String
        If String.Equals(value, UsCustomarySystem, StringComparison.OrdinalIgnoreCase) Then
            Return UsCustomarySystem
        End If
        If String.Equals(value, MetricSystem, StringComparison.OrdinalIgnoreCase) Then
            Return MetricSystem
        End If
        Return SourceUnitsSystem
    End Function

    Public Shared Function NormalizeUnit(value As String) As String
        Dim normalized = If(value, String.Empty).Trim().ToLowerInvariant()
        normalized = normalized.Replace(".", String.Empty)
        normalized = normalized.Replace("_", " ")
        normalized = normalized.Replace("-", " ")
        normalized = Regex.Replace(normalized, "\s+", " ")
        If normalized = String.Empty Then Return String.Empty

        Dim canonical As String = Nothing
        If UnitAliases.TryGetValue(normalized, canonical) Then Return canonical
        Return String.Empty
    End Function

    Public Shared Function IsSupportedUnit(value As String) As Boolean
        Return NormalizeUnit(value) <> String.Empty
    End Function

    Public Shared Function IngredientKey(value As String) As String
        Dim normalized = RecipeIngredient.NormalizeIngredientName(
            value
        ).ToLowerInvariant()
        normalized = Regex.Replace(normalized, "[^\p{L}\p{Nd}]+", " ")
        Return Regex.Replace(normalized, "\s+", " ").Trim()
    End Function

    Public Shared Function TryParseQuantity(
        value As Object,
        ByRef quantity As Double
    ) As Boolean
        quantity = 0
        If value Is Nothing Then Return False

        If TypeOf value Is Double OrElse
            TypeOf value Is Single OrElse
            TypeOf value Is Decimal OrElse
            TypeOf value Is Integer OrElse
            TypeOf value Is Long Then
            quantity = Convert.ToDouble(value, CultureInfo.CurrentCulture)
            Return Not Double.IsNaN(quantity) AndAlso
                Not Double.IsInfinity(quantity) AndAlso
                quantity >= 0
        End If

        Dim text = Convert.ToString(value).Trim()
        If Double.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.CurrentCulture,
            quantity
        ) OrElse Double.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            quantity
        ) Then
            Return quantity >= 0 AndAlso
                Not Double.IsNaN(quantity) AndAlso
                Not Double.IsInfinity(quantity)
        End If

        Dim expanded = ExpandUnicodeFractions(text).Trim()
        Dim consumedLength As Integer
        If TryReadLeadingQuantity(
            expanded,
            quantity,
            consumedLength
        ) AndAlso expanded.Substring(consumedLength).Trim() = String.Empty Then
            Return quantity >= 0 AndAlso
                Not Double.IsNaN(quantity) AndAlso
                Not Double.IsInfinity(quantity)
        End If
        Return False
    End Function

    Public Shared Function TryParseLegacyAmount(
        amount As String,
        ByRef quantity As Double,
        ByRef unit As String
    ) As Boolean
        Dim maximum As Double
        Return TryParseLegacyAmountRange(
            amount,
            quantity,
            maximum,
            unit
        )
    End Function

    Public Shared Function TryParseLegacyAmountRange(
        amount As String,
        ByRef minimum As Double,
        ByRef maximum As Double,
        ByRef measurement As String
    ) As Boolean
        minimum = 0
        maximum = 0
        measurement = String.Empty
        Dim normalized = If(amount, String.Empty).Trim()
        If normalized = String.Empty Then Return False

        If Regex.IsMatch(normalized, "\bto\s+taste\b", RegexOptions.IgnoreCase) Then
            measurement = "to taste"
            Return True
        End If
        If Regex.IsMatch(normalized, "\bas\s+needed\b", RegexOptions.IgnoreCase) Then
            measurement = "none"
            Return True
        End If

        normalized = ExpandUnicodeFractions(normalized)
        Dim consumedLength As Integer
        If Not TryReadLeadingQuantity(normalized, minimum, consumedLength) Then
            Return False
        End If

        Dim remainder = normalized.Substring(consumedLength).Trim()
        Dim rangeSeparator = Regex.Match(
            remainder,
            "^(?:[-–—]|to\b)\s*",
            RegexOptions.IgnoreCase
        )
        If rangeSeparator.Success Then
            remainder = remainder.Substring(rangeSeparator.Length)
            Dim maximumLength As Integer
            If Not TryReadLeadingQuantity(
                remainder,
                maximum,
                maximumLength
            ) OrElse maximum < minimum Then
                Return False
            End If
            remainder = remainder.Substring(maximumLength).Trim()
        Else
            maximum = minimum
        End If

        remainder = remainder.TrimStart(","c, ";"c, ":"c)
        measurement = InferUnit(remainder)
        If measurement = String.Empty Then
            If minimum > 0 AndAlso remainder = String.Empty Then
                measurement = "piece"
            End If
            If measurement = String.Empty Then Return False
        End If
        Return True
    End Function

    Public Shared Function FormatSourceAmount(
        minimum As Double,
        maximum As Double
    ) As String
        If Math.Abs(maximum - minimum) < 0.000000001 Then
            Return FormatInvariantNumber(minimum)
        End If
        Return FormatInvariantNumber(minimum) & "-" &
            FormatInvariantNumber(maximum)
    End Function

    Public Shared Function FormatQuantity(value As Double) As String
        Return FormatNumber(value)
    End Function

    Public Shared Function FormatAmount(
        ingredient As RecipeIngredient,
        Optional system As String = Nothing,
        Optional scale As Double = 1
    ) As String
        If ingredient Is Nothing Then Return String.Empty

        Dim minimum As Double
        Dim maximum As Double
        Dim measurement = NormalizeUnit(ingredient.Measurement)
        If Not ingredient.TryGetAmountRange(minimum, maximum) Then
            If measurement = "to taste" OrElse measurement = "none" Then
                Return FormatUnquantifiedMeasurement(measurement)
            End If
            If Not TryParseLegacyAmountRange(
                ingredient.Amount,
                minimum,
                maximum,
                measurement
            ) Then
                Return ingredient.Amount
            End If
        End If

        Dim targetMeasurement = GetDefaultDisplayMeasurement(
            minimum,
            maximum,
            measurement,
            NormalizeSystem(system)
        )
        Dim convertedMinimum As Double
        Dim convertedMaximum As Double
        If Not TryConvertAmountRange(
            minimum * Math.Max(0, scale),
            maximum * Math.Max(0, scale),
            measurement,
            targetMeasurement,
            convertedMinimum,
            convertedMaximum
        ) Then
            Return ingredient.Amount
        End If
        Return FormatAmountRange(
            convertedMinimum,
            convertedMaximum
        ) & " " & GetUnitLabel(
            targetMeasurement,
            convertedMinimum,
            convertedMaximum
        )
    End Function

    Public Shared Function FormatAmountValue(
        ingredient As RecipeIngredient,
        targetMeasurement As String,
        Optional scale As Double = 1
    ) As String
        If ingredient Is Nothing Then Return String.Empty
        Dim sourceMeasurement = NormalizeUnit(ingredient.Measurement)
        If sourceMeasurement = "to taste" OrElse sourceMeasurement = "none" Then
            Return FormatUnquantifiedMeasurement(sourceMeasurement)
        End If

        Dim minimum As Double
        Dim maximum As Double
        If Not ingredient.TryGetAmountRange(minimum, maximum) Then
            Return ingredient.Amount
        End If
        Return FormatConvertedAmountRange(
            minimum * Math.Max(0, scale),
            maximum * Math.Max(0, scale),
            sourceMeasurement,
            targetMeasurement
        )
    End Function

    Public Shared Function GetDefaultDisplayMeasurement(
        ingredient As RecipeIngredient,
        system As String
    ) As String
        If ingredient Is Nothing Then Return String.Empty
        Dim minimum As Double
        Dim maximum As Double
        If Not ingredient.TryGetAmountRange(minimum, maximum) Then
            Return NormalizeUnit(ingredient.Measurement)
        End If
        Return GetDefaultDisplayMeasurement(
            minimum,
            maximum,
            ingredient.Measurement,
            NormalizeSystem(system)
        )
    End Function

    Public Shared Function FormatMeasurement(
        quantity As Double,
        unit As String,
        system As String
    ) As String
        Return FormatMeasurementRange(quantity, quantity, unit, system)
    End Function

    Public Shared Function FormatMeasurementRange(
        minimum As Double,
        maximum As Double,
        measurement As String,
        system As String
    ) As String
        Dim sourceMeasurement = NormalizeUnit(measurement)
        If sourceMeasurement = String.Empty Then
            Return FormatAmountRange(minimum, maximum)
        End If
        If sourceMeasurement = "to taste" OrElse sourceMeasurement = "none" Then
            Return FormatUnquantifiedMeasurement(sourceMeasurement)
        End If

        Dim targetMeasurement = GetDefaultDisplayMeasurement(
            minimum,
            maximum,
            sourceMeasurement,
            NormalizeSystem(system)
        )
        Dim convertedMinimum As Double
        Dim convertedMaximum As Double
        If Not TryConvertAmountRange(
            minimum,
            maximum,
            sourceMeasurement,
            targetMeasurement,
            convertedMinimum,
            convertedMaximum
        ) Then
            Return FormatAmountRange(minimum, maximum)
        End If
        Return FormatAmountRange(
            convertedMinimum,
            convertedMaximum
        ) & " " & GetUnitLabel(
            targetMeasurement,
            convertedMinimum,
            convertedMaximum
        )
    End Function

    Public Shared Function ConsolidateIngredients(
        ingredients As IEnumerable(Of RecipeIngredient)
    ) As List(Of RecipeIngredient)
        Dim consolidated As New List(Of RecipeIngredient)
        For Each sourceIngredient In If(
            ingredients,
            Enumerable.Empty(Of RecipeIngredient)
        )
            If sourceIngredient Is Nothing Then Continue For

            Dim ingredient = New RecipeIngredient(
                sourceIngredient.Ingredient,
                amount:=sourceIngredient.Amount,
                details:=sourceIngredient.Details,
                minAmount:=sourceIngredient.MinAmount,
                maxAmount:=sourceIngredient.MaxAmount,
                measurement:=sourceIngredient.Measurement,
                originalMeasurement:=sourceIngredient.OriginalMeasurement
            )
            If String.IsNullOrWhiteSpace(ingredient.Ingredient) Then
                Continue For
            End If

            Dim merged = False
            For index As Integer = 0 To consolidated.Count - 1
                If Not String.Equals(
                    IngredientKey(consolidated(index).Ingredient),
                    IngredientKey(ingredient.Ingredient),
                    StringComparison.OrdinalIgnoreCase
                ) Then
                    Continue For
                End If

                Dim combined As RecipeIngredient = Nothing
                If TryCombineIngredients(
                    consolidated(index),
                    ingredient,
                    combined
                ) Then
                    consolidated(index) = combined
                    merged = True
                    Exit For
                End If
            Next
            If Not merged Then consolidated.Add(ingredient)
        Next
        Return consolidated
    End Function

    Public Shared Function Aggregate(
        entries As IEnumerable(Of IngredientAmountEntry),
        system As String
    ) As List(Of ConsolidatedIngredient)
        Dim totals As New Dictionary(Of String, IngredientAccumulator)(
            StringComparer.OrdinalIgnoreCase
        )

        For Each entry In If(entries, Enumerable.Empty(Of IngredientAmountEntry))
            If entry Is Nothing OrElse entry.Ingredient Is Nothing OrElse
                String.IsNullOrWhiteSpace(entry.Ingredient.Ingredient) Then
                Continue For
            End If

            Dim ingredient = entry.Ingredient
            Dim minimum As Double
            Dim maximum As Double
            Dim measurement = NormalizeUnit(ingredient.Measurement)
            Dim hasAmountRange = ingredient.TryGetAmountRange(
                minimum,
                maximum
            )
            Dim hasMeasurement = IsSupportedUnit(measurement) AndAlso
                (
                    hasAmountRange OrElse
                    measurement = "to taste" OrElse
                    measurement = "none"
                )
            If Not hasMeasurement Then
                hasMeasurement = TryParseLegacyAmountRange(
                    ingredient.Amount,
                    minimum,
                    maximum,
                    measurement
                )
                hasAmountRange = hasMeasurement AndAlso
                    measurement <> "to taste" AndAlso measurement <> "none"
            End If

            Dim nameKey = IngredientKey(ingredient.Ingredient)
            If Not hasMeasurement Then
                Dim legacyKey =
                    nameKey &
                    "|legacy|" &
                    If(ingredient.Amount, String.Empty).Trim().ToLowerInvariant()
                If Not totals.ContainsKey(legacyKey) Then
                    totals(legacyKey) = New IngredientAccumulator(
                        legacyKey,
                        ingredient.Ingredient,
                        "legacy",
                        String.Empty
                    )
                End If
                totals(legacyKey).LegacyAmount = ingredient.Amount
                totals(legacyKey).Occurrences += 1
                Continue For
            End If

            Dim dimension = GetDimension(measurement)
            Dim groupingUnit = If(
                dimension = "mass" OrElse dimension = "volume",
                String.Empty,
                measurement
            )
            Dim key = nameKey & "|" & dimension & "|" & groupingUnit
            If Not totals.ContainsKey(key) Then
                totals(key) = New IngredientAccumulator(
                    key,
                    ingredient.Ingredient,
                    dimension,
                    measurement
                )
            End If

            totals(key).Occurrences += 1
            If hasAmountRange Then
                Dim scaledMinimum = Math.Max(0, minimum * entry.Scale)
                Dim scaledMaximum = Math.Max(0, maximum * entry.Scale)
                totals(key).MinimumQuantity += ToBaseQuantity(
                    scaledMinimum,
                    measurement
                )
                totals(key).MaximumQuantity += ToBaseQuantity(
                    scaledMaximum,
                    measurement
                )
                totals(key).HasAmountRange = True
            End If
        Next

        Dim normalizedSystem = NormalizeSystem(system)
        Return totals.Values.
            Select(
                Function(total)
                    Dim minimum As Double? = Nothing
                    Dim maximum As Double? = Nothing
                    If total.HasAmountRange Then
                        minimum = total.MinimumQuantity
                        maximum = total.MaximumQuantity
                    End If
                    Dim defaultMeasurement =
                        GetDefaultDisplayMeasurementFromBase(
                            total.MaximumQuantity,
                            total.Dimension,
                            total.OriginalMeasurement,
                            normalizedSystem
                        )
                    Return New ConsolidatedIngredient(
                        total.Key,
                        total.Name,
                        total.Dimension,
                        total.OriginalMeasurement,
                        minimum,
                        maximum,
                        total.LegacyAmount,
                        total.Occurrences,
                        defaultMeasurement,
                        GetCompatibleMeasurements(defaultMeasurement)
                    )
                End Function
            ).
            OrderBy(
                Function(total) total.Ingredient,
                StringComparer.CurrentCultureIgnoreCase
            ).
            ThenBy(Function(total) total.Dimension).
            ToList()
    End Function

    Private Shared Function TryCombineIngredients(
        first As RecipeIngredient,
        second As RecipeIngredient,
        ByRef combined As RecipeIngredient
    ) As Boolean
        combined = Nothing
        If first Is Nothing OrElse second Is Nothing OrElse
            Not first.HasStructuredMeasurement() OrElse
            Not second.HasStructuredMeasurement() Then
            Return False
        End If

        Dim firstUnit = NormalizeUnit(first.Measurement)
        Dim secondUnit = NormalizeUnit(second.Measurement)
        Dim firstDimension = GetDimension(firstUnit)
        Dim secondDimension = GetDimension(secondUnit)
        If Not String.Equals(
            firstDimension,
            secondDimension,
            StringComparison.Ordinal
        ) Then
            Return False
        End If

        If firstDimension = "unquantified" Then
            If Not String.Equals(
                firstUnit,
                secondUnit,
                StringComparison.OrdinalIgnoreCase
            ) Then
                Return False
            End If
            combined = New RecipeIngredient(
                first.Ingredient,
                amount:=first.Amount,
                details:=RecipeIngredient.MergeDetails(
                    first.Details,
                    second.Details
                ),
                measurement:=firstUnit,
                originalMeasurement:=first.OriginalMeasurement
            )
            Return True
        End If

        Dim firstMinimum As Double
        Dim firstMaximum As Double
        Dim secondMinimum As Double
        Dim secondMaximum As Double
        If Not first.TryGetAmountRange(firstMinimum, firstMaximum) OrElse
            Not second.TryGetAmountRange(secondMinimum, secondMaximum) Then
            Return False
        End If

        Dim combinedMinimum As Double
        Dim combinedMaximum As Double
        If firstDimension = "mass" OrElse
            firstDimension = "volume" Then
            Dim baseMinimum =
                ToBaseQuantity(firstMinimum, firstUnit) +
                ToBaseQuantity(secondMinimum, secondUnit)
            Dim baseMaximum =
                ToBaseQuantity(firstMaximum, firstUnit) +
                ToBaseQuantity(secondMaximum, secondUnit)
            Dim factors = If(
                firstDimension = "mass",
                MassFactors,
                VolumeFactors
            )
            combinedMinimum = baseMinimum / factors(firstUnit)
            combinedMaximum = baseMaximum / factors(firstUnit)
        Else
            If Not String.Equals(
                firstUnit,
                secondUnit,
                StringComparison.OrdinalIgnoreCase
            ) Then
                Return False
            End If
            combinedMinimum = firstMinimum + secondMinimum
            combinedMaximum = firstMaximum + secondMaximum
        End If
        combinedMinimum = Math.Round(combinedMinimum, 9)
        combinedMaximum = Math.Round(combinedMaximum, 9)

        combined = New RecipeIngredient(
            first.Ingredient,
            amount:=FormatSourceAmount(
                combinedMinimum,
                combinedMaximum
            ),
            minAmount:=combinedMinimum,
            maxAmount:=combinedMaximum,
            measurement:=firstUnit,
            originalMeasurement:=first.OriginalMeasurement,
            details:=RecipeIngredient.MergeDetails(
                first.Details,
                second.Details
            )
        )
        Return True
    End Function

    Private Shared Function ExpandUnicodeFractions(value As String) As String
        Return value.
            Replace("⁄", "/").
            Replace("¼", " 1/4").
            Replace("½", " 1/2").
            Replace("¾", " 3/4").
            Replace("⅓", " 1/3").
            Replace("⅔", " 2/3").
            Replace("⅕", " 1/5").
            Replace("⅖", " 2/5").
            Replace("⅗", " 3/5").
            Replace("⅘", " 4/5").
            Replace("⅙", " 1/6").
            Replace("⅚", " 5/6").
            Replace("⅛", " 1/8").
            Replace("⅜", " 3/8").
            Replace("⅝", " 5/8").
            Replace("⅞", " 7/8").
            Replace("⅐", " 1/7").
            Replace("⅑", " 1/9").
            Replace("⅒", " 1/10")
    End Function

    Private Shared Function TryReadLeadingQuantity(
        value As String,
        ByRef quantity As Double,
        ByRef consumedLength As Integer
    ) As Boolean
        quantity = 0
        consumedLength = 0

        Dim mixed = Regex.Match(
            value,
            "^\s*(?<whole>\d+)\s*(?:\s+|-)\s*(?<numerator>\d+)\s*/\s*(?<denominator>\d+)"
        )
        If mixed.Success Then
            Dim denominator = Double.Parse(
                mixed.Groups("denominator").Value,
                CultureInfo.InvariantCulture
            )
            If denominator <= 0 Then Return False
            quantity =
                Double.Parse(
                    mixed.Groups("whole").Value,
                    CultureInfo.InvariantCulture
                ) +
                Double.Parse(
                    mixed.Groups("numerator").Value,
                    CultureInfo.InvariantCulture
                ) / denominator
            consumedLength = mixed.Length
            Return True
        End If

        Dim fraction = Regex.Match(
            value,
            "^\s*(?<numerator>\d+)\s*/\s*(?<denominator>\d+)"
        )
        If fraction.Success Then
            Dim denominator = Double.Parse(
                fraction.Groups("denominator").Value,
                CultureInfo.InvariantCulture
            )
            If denominator <= 0 Then Return False
            quantity =
                Double.Parse(
                    fraction.Groups("numerator").Value,
                    CultureInfo.InvariantCulture
                ) / denominator
            consumedLength = fraction.Length
            Return True
        End If

        Dim number = Regex.Match(value, "^\s*(?<number>\d+(?:[\.,]\d+)?)")
        If Not number.Success Then Return False
        Dim numberText = number.Groups("number").Value.Replace(
            ","c,
            "."c
        )
        If Not Double.TryParse(
            numberText,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            quantity
        ) Then
            Return False
        End If
        consumedLength = number.Length
        Return True
    End Function

    Private Shared Function InferUnit(value As String) As String
        Dim normalized = Regex.Replace(
            If(value, String.Empty).Trim().ToLowerInvariant(),
            "[\._-]+",
            " "
        )
        normalized = Regex.Replace(normalized, "\s+", " ")
        normalized = normalized.Trim()
        If normalized = String.Empty Then Return String.Empty

        For Each aliasEntry In UnitAliases.OrderByDescending(
            Function(entry) entry.Key.Length
        )
            If Regex.IsMatch(
                normalized,
                "^(?:" & Regex.Escape(aliasEntry.Key) & ")\b",
                RegexOptions.IgnoreCase
            ) Then
                Return aliasEntry.Value
            End If
        Next

        For Each countAlias In {"cans?", "packages?", "bunch(?:es)?", "cloves?", "slices?"}
            Dim match = Regex.Match(
                normalized,
                "\b" & countAlias & "\b",
                RegexOptions.IgnoreCase
            )
            If Not match.Success Then Continue For
            Return NormalizeUnit(match.Value)
        Next
        Return String.Empty
    End Function

    Public Shared Function GetMeasurementDimension(measurement As String) As String
        Return GetDimension(NormalizeUnit(measurement))
    End Function

    Public Shared Function GetCompatibleMeasurements(
        measurement As String
    ) As IReadOnlyList(Of String)
        Dim canonicalMeasurement = NormalizeUnit(measurement)
        Dim dimension = GetDimension(canonicalMeasurement)
        If dimension = "mass" Then
            Return New List(Of String) From {
                "milligram",
                "gram",
                "kilogram",
                "ounce",
                "pound"
            }
        End If
        If dimension = "volume" Then
            Return New List(Of String) From {
                "teaspoon",
                "tablespoon",
                "fluid ounce",
                "cup",
                "pint",
                "quart",
                "gallon",
                "milliliter",
                "liter"
            }
        End If
        If canonicalMeasurement = String.Empty Then
            Return New List(Of String)
        End If
        Return New List(Of String) From {canonicalMeasurement}
    End Function

    Private Shared Function GetDimension(unit As String) As String
        If MassFactors.ContainsKey(unit) Then Return "mass"
        If VolumeFactors.ContainsKey(unit) Then Return "volume"
        If unit = "none" OrElse unit = "to taste" Then Return "unquantified"
        If unit = String.Empty Then Return "legacy"
        Return "count"
    End Function

    Private Shared Function ToBaseQuantity(
        quantity As Double,
        unit As String
    ) As Double
        If MassFactors.ContainsKey(unit) Then Return quantity * MassFactors(unit)
        If VolumeFactors.ContainsKey(unit) Then Return quantity * VolumeFactors(unit)
        Return quantity
    End Function

    Public Shared Function FormatBaseAmountRange(
        minimumBaseAmount As Double?,
        maximumBaseAmount As Double?,
        dimension As String,
        targetMeasurement As String
    ) As String
        If dimension = "unquantified" Then
            Return FormatUnquantifiedMeasurement(
                NormalizeUnit(targetMeasurement)
            )
        End If
        If Not minimumBaseAmount.HasValue OrElse
            Not maximumBaseAmount.HasValue Then
            Return String.Empty
        End If

        Dim canonicalTarget = NormalizeUnit(targetMeasurement)
        If GetDimension(canonicalTarget) <> dimension Then
            Return String.Empty
        End If
        Return FormatAmountRange(
            FromBaseQuantity(
                minimumBaseAmount.Value,
                canonicalTarget
            ),
            FromBaseQuantity(
                maximumBaseAmount.Value,
                canonicalTarget
            )
        )
    End Function

    Private Shared Function GetDefaultDisplayMeasurement(
        minimum As Double,
        maximum As Double,
        sourceMeasurement As String,
        system As String
    ) As String
        Dim canonicalSource = NormalizeUnit(sourceMeasurement)
        Return GetDefaultDisplayMeasurementFromBase(
            ToBaseQuantity(Math.Max(minimum, maximum), canonicalSource),
            GetDimension(canonicalSource),
            canonicalSource,
            system
        )
    End Function

    Private Shared Function GetDefaultDisplayMeasurementFromBase(
        maximumBaseAmount As Double,
        dimension As String,
        originalMeasurement As String,
        system As String
    ) As String
        Dim canonicalOriginal = NormalizeUnit(originalMeasurement)
        If system = SourceUnitsSystem OrElse
            (dimension <> "mass" AndAlso dimension <> "volume") Then
            Return canonicalOriginal
        End If

        If system = MetricSystem Then
            If dimension = "mass" Then
                If maximumBaseAmount < 1 Then Return "milligram"
                If maximumBaseAmount >= 1000 Then Return "kilogram"
                Return "gram"
            End If
            Return If(
                maximumBaseAmount >= 1000,
                "liter",
                "milliliter"
            )
        End If

        If dimension = "mass" Then
            Dim ounces = maximumBaseAmount / MassFactors("ounce")
            Return If(ounces >= 16, "pound", "ounce")
        End If

        Dim cups = maximumBaseAmount / VolumeFactors("cup")
        If cups >= 16 Then
            Return "gallon"
        ElseIf cups >= 4 Then
            Return "quart"
        ElseIf cups >= 2 Then
            Return "pint"
        ElseIf cups >= 0.25 Then
            Return "cup"
        ElseIf maximumBaseAmount / VolumeFactors("tablespoon") >= 1 Then
            Return "tablespoon"
        End If
        Return "teaspoon"
    End Function

    Private Shared Function FormatConvertedAmountRange(
        minimum As Double,
        maximum As Double,
        sourceMeasurement As String,
        targetMeasurement As String
    ) As String
        Dim convertedMinimum As Double
        Dim convertedMaximum As Double
        If Not TryConvertAmountRange(
            minimum,
            maximum,
            sourceMeasurement,
            targetMeasurement,
            convertedMinimum,
            convertedMaximum
        ) Then
            Return String.Empty
        End If
        Return FormatAmountRange(convertedMinimum, convertedMaximum)
    End Function

    Private Shared Function TryConvertAmountRange(
        minimum As Double,
        maximum As Double,
        sourceMeasurement As String,
        targetMeasurement As String,
        ByRef convertedMinimum As Double,
        ByRef convertedMaximum As Double
    ) As Boolean
        convertedMinimum = 0
        convertedMaximum = 0
        Dim canonicalSource = NormalizeUnit(sourceMeasurement)
        Dim canonicalTarget = NormalizeUnit(targetMeasurement)
        If canonicalSource = String.Empty OrElse canonicalTarget = String.Empty OrElse
            GetDimension(canonicalSource) <> GetDimension(canonicalTarget) Then
            Return False
        End If
        convertedMinimum = FromBaseQuantity(
            ToBaseQuantity(minimum, canonicalSource),
            canonicalTarget
        )
        convertedMaximum = FromBaseQuantity(
            ToBaseQuantity(maximum, canonicalSource),
            canonicalTarget
        )
        Return True
    End Function

    Private Shared Function FromBaseQuantity(
        baseQuantity As Double,
        measurement As String
    ) As Double
        If MassFactors.ContainsKey(measurement) Then
            Return baseQuantity / MassFactors(measurement)
        End If
        If VolumeFactors.ContainsKey(measurement) Then
            Return baseQuantity / VolumeFactors(measurement)
        End If
        Return baseQuantity
    End Function

    Private Shared Function FormatAmountRange(
        minimum As Double,
        maximum As Double
    ) As String
        If Math.Abs(maximum - minimum) < 0.000000001 Then
            Return FormatNumber(minimum)
        End If
        Return FormatNumber(minimum) & "–" & FormatNumber(maximum)
    End Function

    Private Shared Function FormatUnquantifiedMeasurement(
        measurement As String
    ) As String
        If measurement = "to taste" Then Return "To taste"
        If measurement = "none" Then Return "As needed"
        Return String.Empty
    End Function

    Private Shared Function FormatNumber(value As Double) As String
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return value.ToString(CultureInfo.CurrentCulture)
        End If
        Dim absoluteValue = Math.Abs(value)
        If absoluteValue < 0.0000000005 Then Return "0"

        Dim roundedWhole = Math.Round(value)
        If Math.Abs(value - roundedWhole) < 0.000000001 Then
            Return roundedWhole.ToString(
                "0",
                CultureInfo.CurrentCulture
            )
        End If

        Dim wholePart = Math.Floor(absoluteValue)
        Dim fractionalPart = absoluteValue - wholePart
        For Each denominator In DisplayFractionDenominators
            Dim numerator = CInt(
                Math.Round(
                    fractionalPart * denominator,
                    MidpointRounding.AwayFromZero
                )
            )
            If numerator <= 0 OrElse numerator >= denominator Then
                Continue For
            End If
            If Math.Abs(
                fractionalPart - numerator / CDbl(denominator)
            ) > DisplayFractionTolerance Then
                Continue For
            End If

            Dim divisor = GreatestCommonDivisor(numerator, denominator)
            numerator \= divisor
            Dim reducedDenominator = denominator \ divisor
            Dim sign = If(value < 0, "-", String.Empty)
            Dim fraction = numerator.ToString(
                CultureInfo.CurrentCulture
            ) & "/" & reducedDenominator.ToString(
                CultureInfo.CurrentCulture
            )
            If wholePart < 1 Then Return sign & fraction
            Return sign & wholePart.ToString(
                "0",
                CultureInfo.CurrentCulture
            ) & " " & fraction
        Next

        Dim decimalPlaces = If(absoluteValue < 0.001, 9, 3)
        Dim format = If(
            decimalPlaces = 9,
            "0.#########",
            "0.###"
        )
        Return Math.Round(value, decimalPlaces).ToString(
            format,
            CultureInfo.CurrentCulture
        )
    End Function

    Private Shared Function GreatestCommonDivisor(
        first As Integer,
        second As Integer
    ) As Integer
        first = Math.Abs(first)
        second = Math.Abs(second)
        While second <> 0
            Dim remainder = first Mod second
            first = second
            second = remainder
        End While
        Return Math.Max(1, first)
    End Function

    Private Shared Function FormatInvariantNumber(value As Double) As String
        Return Math.Round(value, 9).ToString(
            "0.#########",
            CultureInfo.InvariantCulture
        )
    End Function

    Private Shared Function GetUnitLabel(
        unit As String,
        minimum As Double,
        maximum As Double
    ) As String
        If Math.Abs(minimum - 1) < 0.0005 AndAlso
            Math.Abs(maximum - 1) < 0.0005 Then
            Return unit
        End If
        Select Case unit
            Case "pinch"
                Return "pinches"
            Case "dash"
                Return "dashes"
            Case "bunch"
                Return "bunches"
            Case Else
                Return unit & "s"
        End Select
    End Function

    Private NotInheritable Class IngredientAccumulator
        Public ReadOnly Property Key As String
        Public ReadOnly Property Name As String
        Public ReadOnly Property Dimension As String
        Public ReadOnly Property OriginalMeasurement As String
        Public Property MinimumQuantity As Double
        Public Property MaximumQuantity As Double
        Public Property HasAmountRange As Boolean
        Public Property LegacyAmount As String
        Public Property Occurrences As Integer

        Public Sub New(
            key As String,
            name As String,
            dimension As String,
            originalMeasurement As String
        )
            Me.Key = key
            Me.Name = name
            Me.Dimension = dimension
            Me.OriginalMeasurement = originalMeasurement
        End Sub
    End Class
End Class
