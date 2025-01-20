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

    Public Function FormattedAmount()
        If gNutritionals.Contains(Name) Then
            Return Amount & " g"
        ElseIf mgNutritionals.Contains(Name) Then
            Return Amount & " mg"
        Else
            Return Amount.ToString()
        End If
    End Function
End Class
