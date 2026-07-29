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

    Public Property Ingredient As String
    Public Property Amount As String
    Public Property Quantity As Double?
    Public Property Unit As String

    <Newtonsoft.Json.JsonConstructor>
    Public Sub New(
        ingredient As String,
        Optional amount As String = Nothing,
        Optional quantity As Double? = Nothing,
        Optional unit As String = Nothing
    )
        Me.Ingredient = NormalizeIngredientName(ingredient)
        Me.Amount = If(amount, String.Empty).Trim()
        Me.Quantity = If(
            quantity.HasValue AndAlso
            Not Double.IsNaN(quantity.Value) AndAlso
            Not Double.IsInfinity(quantity.Value) AndAlso
            quantity.Value >= 0,
            quantity,
            Nothing
        )
        Me.Unit = IngredientMeasurementConverter.NormalizeUnit(unit)

        If Not Me.Quantity.HasValue AndAlso Me.Amount <> String.Empty Then
            Dim parsedQuantity As Double
            Dim parsedUnit As String = Nothing
            If IngredientMeasurementConverter.TryParseLegacyAmount(
                Me.Amount,
                parsedQuantity,
                parsedUnit
            ) Then
                Me.Quantity = parsedQuantity
                Me.Unit = parsedUnit
            End If
        End If
        If Me.Quantity.HasValue AndAlso Me.Unit = String.Empty Then
            Me.Unit = If(Me.Quantity.Value > 0, "piece", "none")
        End If
    End Sub

    Public Function HasStructuredMeasurement() As Boolean
        Return Quantity.HasValue AndAlso
            IngredientMeasurementConverter.IsSupportedUnit(Unit)
    End Function

    Public Function DisplayAmount(Optional system As String = Nothing) As String
        Return IngredientMeasurementConverter.FormatAmount(Me, system)
    End Function

    Public Function Clone() As RecipeIngredient
        Return New RecipeIngredient(Ingredient, Amount, Quantity, Unit)
    End Function

    Public Shared Function NormalizeIngredientName(value As String) As String
        Dim normalized = If(value, String.Empty).Trim()
        If normalized = String.Empty Then Return normalized
        normalized = StripPurposeQualifiers(normalized)
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
