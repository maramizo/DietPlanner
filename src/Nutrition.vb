Public Class Nutrition
    Public Property Name As String
    Public Property Amount As Double

    Public Sub New(name As String, amount As Double)
        Me.Name = StrConv(name, VbStrConv.ProperCase)
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
