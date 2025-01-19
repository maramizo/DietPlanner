Imports Newtonsoft.Json
Imports System.IO

Module Globals
    Public gNutritionals As New List(Of String) From {"Protein", "Fat", "Carbs", "Dietary Fiber", "Trans Fat", "Saturated Fat"}
    Public mgNutritionals As New List(Of String) From {"Sugar", "Sodium", "Potassium", "Phosphorus", "Calcium"}
    Public AllNutritionals As New List(Of String)(gNutritionals.Concat(mgNutritionals).OrderBy(Function(n) n).Select(Function(name) StrConv(name, VbStrConv.ProperCase)).ToList())
End Module

Public Class NutrientInfo
    Public RecommendedDailyIntakes As New Dictionary(Of String, Double)

    Public Sub New()
        Try
            Dim json As String = File.ReadAllText("./data/recommended.json")
            RecommendedDailyIntakes = JsonConvert.DeserializeObject(Of Dictionary(Of String, Double))(json)
        Catch ex As Exception
            RecommendedDailyIntakes.Add("Protein", 100)
            RecommendedDailyIntakes.Add("Fat", 70)
            RecommendedDailyIntakes.Add("Carbs", 310)
            RecommendedDailyIntakes.Add("Sugar", 90)
            RecommendedDailyIntakes.Add("Sodium", 2000)
            RecommendedDailyIntakes.Add("Potassium", 47)
            RecommendedDailyIntakes.Add("Phosphorus", 0.7)
            RecommendedDailyIntakes.Add("Calcium", 1000)
        End Try
    End Sub

    Public Sub Save()
        Dim json = JsonConvert.SerializeObject(RecommendedDailyIntakes)
        File.WriteAllText("./data/recommended.json", json)
    End Sub
End Class