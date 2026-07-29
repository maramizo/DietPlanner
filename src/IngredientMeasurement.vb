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
    Public ReadOnly Property Ingredient As String
    Public ReadOnly Property Amount As String

    Public Sub New(ingredient As String, amount As String)
        Me.Ingredient = ingredient
        Me.Amount = amount
    End Sub
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
        "ounce",
        "pound",
        "gram",
        "kilogram",
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
        Return False
    End Function

    Public Shared Function TryParseLegacyAmount(
        amount As String,
        ByRef quantity As Double,
        ByRef unit As String
    ) As Boolean
        quantity = 0
        unit = String.Empty
        Dim normalized = If(amount, String.Empty).Trim()
        If normalized = String.Empty Then Return False

        If Regex.IsMatch(normalized, "\bto\s+taste\b", RegexOptions.IgnoreCase) Then
            unit = "to taste"
            Return True
        End If
        If Regex.IsMatch(normalized, "\bas\s+needed\b", RegexOptions.IgnoreCase) Then
            unit = "none"
            Return True
        End If

        normalized = ExpandUnicodeFractions(normalized)
        Dim consumedLength As Integer
        If Not TryReadLeadingQuantity(normalized, quantity, consumedLength) Then
            Return False
        End If

        Dim remainder = normalized.Substring(consumedLength).Trim()
        remainder = remainder.TrimStart(","c, ";"c, ":"c)
        unit = InferUnit(remainder)
        If unit = String.Empty Then
            If quantity > 0 AndAlso remainder = String.Empty Then unit = "piece"
            If unit = String.Empty Then Return False
        End If
        Return True
    End Function

    Public Shared Function FormatAmount(
        ingredient As RecipeIngredient,
        Optional system As String = Nothing,
        Optional scale As Double = 1
    ) As String
        If ingredient Is Nothing Then Return String.Empty

        Dim quantity As Double
        Dim unit As String = String.Empty
        If ingredient.Quantity.HasValue AndAlso
            IsSupportedUnit(ingredient.Unit) Then
            quantity = ingredient.Quantity.Value
            unit = NormalizeUnit(ingredient.Unit)
        ElseIf Not TryParseLegacyAmount(ingredient.Amount, quantity, unit) Then
            Return ingredient.Amount
        End If

        Return FormatMeasurement(
            Math.Max(0, quantity * Math.Max(0, scale)),
            unit,
            NormalizeSystem(system)
        )
    End Function

    Public Shared Function FormatMeasurement(
        quantity As Double,
        unit As String,
        system As String
    ) As String
        Dim canonicalUnit = NormalizeUnit(unit)
        If canonicalUnit = String.Empty Then Return FormatNumber(quantity)
        If canonicalUnit = "to taste" Then Return "To taste"
        If canonicalUnit = "none" Then
            Return If(quantity > 0, FormatNumber(quantity), "As needed")
        End If

        Dim convertedQuantity = quantity
        Dim convertedUnit = canonicalUnit
        ConvertForDisplay(
            quantity,
            canonicalUnit,
            NormalizeSystem(system),
            convertedQuantity,
            convertedUnit
        )
        Return FormatNumber(convertedQuantity) &
            " " &
            GetUnitLabel(convertedUnit, convertedQuantity)
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
                sourceIngredient.Amount,
                sourceIngredient.Quantity,
                sourceIngredient.Unit
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
            Dim quantity As Double
            Dim unit As String = String.Empty
            Dim hasMeasurement =
                ingredient.Quantity.HasValue AndAlso
                IsSupportedUnit(ingredient.Unit)
            If hasMeasurement Then
                quantity = ingredient.Quantity.Value
                unit = NormalizeUnit(ingredient.Unit)
            Else
                hasMeasurement = TryParseLegacyAmount(
                    ingredient.Amount,
                    quantity,
                    unit
                )
            End If

            Dim nameKey = IngredientKey(ingredient.Ingredient)
            If Not hasMeasurement Then
                Dim legacyKey =
                    nameKey &
                    "|legacy|" &
                    If(ingredient.Amount, String.Empty).Trim().ToLowerInvariant()
                If Not totals.ContainsKey(legacyKey) Then
                    totals(legacyKey) = New IngredientAccumulator(
                        ingredient.Ingredient,
                        "legacy",
                        String.Empty
                    )
                End If
                totals(legacyKey).LegacyAmount = ingredient.Amount
                totals(legacyKey).Occurrences += 1
                Continue For
            End If

            Dim dimension = GetDimension(unit)
            Dim groupingUnit = If(
                dimension = "mass" OrElse dimension = "volume",
                String.Empty,
                unit
            )
            Dim key = nameKey & "|" & dimension & "|" & groupingUnit
            If Not totals.ContainsKey(key) Then
                totals(key) = New IngredientAccumulator(
                    ingredient.Ingredient,
                    dimension,
                    unit
                )
            End If

            Dim scaledQuantity = Math.Max(0, quantity * entry.Scale)
            totals(key).Quantity += ToBaseQuantity(scaledQuantity, unit)
        Next

        Dim normalizedSystem = NormalizeSystem(system)
        Return totals.Values.
            Select(
                Function(total)
                    Dim amount As String
                    If total.Dimension = "legacy" Then
                        amount = If(total.LegacyAmount, String.Empty).Trim()
                        If total.Occurrences > 1 AndAlso amount <> String.Empty Then
                            amount =
                                total.Occurrences.ToString(
                                    CultureInfo.CurrentCulture
                                ) &
                                " × " &
                                amount
                        End If
                    Else
                        amount = FormatBaseMeasurement(
                            total.Quantity,
                            total.Dimension,
                            total.OriginalUnit,
                            normalizedSystem
                        )
                    End If
                    Return New ConsolidatedIngredient(total.Name, amount)
                End Function
            ).
            OrderBy(
                Function(total) total.Ingredient,
                StringComparer.CurrentCultureIgnoreCase
            ).
            ThenBy(Function(total) total.Amount).
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

        Dim firstUnit = NormalizeUnit(first.Unit)
        Dim secondUnit = NormalizeUnit(second.Unit)
        Dim firstDimension = GetDimension(firstUnit)
        Dim secondDimension = GetDimension(secondUnit)
        If Not String.Equals(
            firstDimension,
            secondDimension,
            StringComparison.Ordinal
        ) Then
            Return False
        End If

        Dim combinedQuantity As Double
        If firstDimension = "mass" OrElse
            firstDimension = "volume" Then
            Dim baseQuantity =
                ToBaseQuantity(first.Quantity.Value, firstUnit) +
                ToBaseQuantity(second.Quantity.Value, secondUnit)
            Dim factors = If(
                firstDimension = "mass",
                MassFactors,
                VolumeFactors
            )
            combinedQuantity = baseQuantity / factors(firstUnit)
        Else
            If Not String.Equals(
                firstUnit,
                secondUnit,
                StringComparison.OrdinalIgnoreCase
            ) Then
                Return False
            End If
            combinedQuantity =
                first.Quantity.Value +
                second.Quantity.Value
        End If
        combinedQuantity = Math.Round(combinedQuantity, 9)

        combined = New RecipeIngredient(
            first.Ingredient,
            quantity:=combinedQuantity,
            unit:=firstUnit
        )
        Return True
    End Function

    Private Shared Function ExpandUnicodeFractions(value As String) As String
        Return value.
            Replace("¼", " 1/4").
            Replace("½", " 1/2").
            Replace("¾", " 3/4").
            Replace("⅓", " 1/3").
            Replace("⅔", " 2/3").
            Replace("⅛", " 1/8").
            Replace("⅜", " 3/8").
            Replace("⅝", " 5/8").
            Replace("⅞", " 7/8")
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

    Private Shared Function GetDimension(unit As String) As String
        If MassFactors.ContainsKey(unit) Then Return "mass"
        If VolumeFactors.ContainsKey(unit) Then Return "volume"
        If unit = "none" OrElse unit = "to taste" Then Return "unquantified"
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

    Private Shared Sub ConvertForDisplay(
        quantity As Double,
        unit As String,
        system As String,
        ByRef convertedQuantity As Double,
        ByRef convertedUnit As String
    )
        convertedQuantity = quantity
        convertedUnit = unit
        If system = SourceUnitsSystem Then Return

        Dim dimension = GetDimension(unit)
        If dimension <> "mass" AndAlso dimension <> "volume" Then Return
        ConvertBaseForDisplay(
            ToBaseQuantity(quantity, unit),
            dimension,
            unit,
            system,
            convertedQuantity,
            convertedUnit
        )
    End Sub

    Private Shared Function FormatBaseMeasurement(
        baseQuantity As Double,
        dimension As String,
        originalUnit As String,
        system As String
    ) As String
        If dimension = "unquantified" Then
            Return FormatMeasurement(baseQuantity, originalUnit, system)
        End If
        If dimension = "count" Then
            Return FormatMeasurement(baseQuantity, originalUnit, system)
        End If

        Dim convertedQuantity As Double
        Dim convertedUnit As String = originalUnit
        ConvertBaseForDisplay(
            baseQuantity,
            dimension,
            originalUnit,
            system,
            convertedQuantity,
            convertedUnit
        )
        Return FormatNumber(convertedQuantity) &
            " " &
            GetUnitLabel(convertedUnit, convertedQuantity)
    End Function

    Private Shared Sub ConvertBaseForDisplay(
        baseQuantity As Double,
        dimension As String,
        originalUnit As String,
        system As String,
        ByRef convertedQuantity As Double,
        ByRef convertedUnit As String
    )
        If system = SourceUnitsSystem Then
            convertedUnit = originalUnit
            Dim factors = If(dimension = "mass", MassFactors, VolumeFactors)
            convertedQuantity = baseQuantity / factors(originalUnit)
            Return
        End If

        If system = MetricSystem Then
            If dimension = "mass" Then
                convertedUnit = If(baseQuantity >= 1000, "kilogram", "gram")
                convertedQuantity = If(
                    convertedUnit = "kilogram",
                    baseQuantity / 1000,
                    baseQuantity
                )
            Else
                convertedUnit = If(baseQuantity >= 1000, "liter", "milliliter")
                convertedQuantity = If(
                    convertedUnit = "liter",
                    baseQuantity / 1000,
                    baseQuantity
                )
            End If
            Return
        End If

        If dimension = "mass" Then
            Dim ounces = baseQuantity / MassFactors("ounce")
            If ounces >= 16 Then
                convertedUnit = "pound"
                convertedQuantity = baseQuantity / MassFactors("pound")
            Else
                convertedUnit = "ounce"
                convertedQuantity = ounces
            End If
            Return
        End If

        Dim cups = baseQuantity / VolumeFactors("cup")
        If cups >= 16 Then
            convertedUnit = "gallon"
        ElseIf cups >= 4 Then
            convertedUnit = "quart"
        ElseIf cups >= 2 Then
            convertedUnit = "pint"
        ElseIf cups >= 0.25 Then
            convertedUnit = "cup"
        ElseIf baseQuantity / VolumeFactors("tablespoon") >= 1 Then
            convertedUnit = "tablespoon"
        Else
            convertedUnit = "teaspoon"
        End If
        convertedQuantity = baseQuantity / VolumeFactors(convertedUnit)
    End Sub

    Private Shared Function FormatNumber(value As Double) As String
        If Math.Abs(value) < 0.0005 Then Return "0"
        Return Math.Round(value, 3).ToString(
            "0.###",
            CultureInfo.CurrentCulture
        )
    End Function

    Private Shared Function GetUnitLabel(
        unit As String,
        quantity As Double
    ) As String
        If Math.Abs(quantity - 1) < 0.0005 Then Return unit
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
        Public ReadOnly Property Name As String
        Public ReadOnly Property Dimension As String
        Public ReadOnly Property OriginalUnit As String
        Public Property Quantity As Double
        Public Property LegacyAmount As String
        Public Property Occurrences As Integer

        Public Sub New(name As String, dimension As String, originalUnit As String)
            Me.Name = name
            Me.Dimension = dimension
            Me.OriginalUnit = originalUnit
        End Sub
    End Class
End Class
