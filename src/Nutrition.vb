Imports System.Globalization
Imports System.Text.RegularExpressions

Public Class Nutrition
    Public Property Name As String
    Public Property Amount As Double

    Public Sub New(name As String, amount As Double)
        Dim convertedName = StrConv(name, VbStrConv.ProperCase)
        If convertedName = "Carbohydrates" Then
            convertedName = "Carbs"
        ElseIf convertedName = "Added Sugars" Then
            convertedName = "Sugar"
        ElseIf convertedName = "Total Fat" Then
            convertedName = "Fat"
        ElseIf convertedName = "Fiber" Then
            convertedName = "Dietary Fiber"
        End If
        Me.Name = convertedName
        Me.Amount = amount
    End Sub

    Public Function FormattedAmount() As String
        If String.Equals(
            Name,
            "Calories",
            StringComparison.OrdinalIgnoreCase
        ) Then
            Return Amount.ToString("N0") & " kcal"
        ElseIf gNutritionals.Contains(Name) Then
            Return Amount & " g"
        ElseIf mgNutritionals.Contains(Name) Then
            Return Amount & " mg"
        Else
            Return Amount.ToString()
        End If
    End Function

    Public Shared Function TryParseAmount(value As Object, ByRef amount As Double) As Boolean
        amount = 0
        If value Is Nothing OrElse value Is DBNull.Value Then Return False

        Dim text = Convert.ToString(value).Trim()
        If text = String.Empty Then Return False

        text = Regex.Replace(
            text,
            "\s*(?:mg|g|kcal|calories?)\s*$",
            String.Empty,
            RegexOptions.IgnoreCase
        ).Trim()

        Dim styles = NumberStyles.Float Or NumberStyles.AllowThousands
        If Double.TryParse(text, styles, CultureInfo.CurrentCulture, amount) Then
            Return True
        End If

        Return Double.TryParse(text, styles, CultureInfo.InvariantCulture, amount)
    End Function
End Class
