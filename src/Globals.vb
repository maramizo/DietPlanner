Imports Newtonsoft.Json
Imports System.IO

Module Globals
    Public gNutritionals As New List(Of String) From {"Protein", "Fat", "Carbs", "Dietary Fiber", "Trans Fat", "Saturated Fat", "Sugar"}
    Public mgNutritionals As New List(Of String) From {"Sodium", "Potassium", "Phosphorus", "Calcium", "Iron", "Cholesterol"}
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

        If RecommendedDailyIntakes Is Nothing Then
            RecommendedDailyIntakes = New Dictionary(Of String, Double)
        End If
        If Not RecommendedDailyIntakes.Keys.Any(
            Function(name) String.Equals(
                name,
                "Calories",
                StringComparison.OrdinalIgnoreCase
            )
        ) Then
            RecommendedDailyIntakes.Add("Calories", 2000)
        End If
    End Sub

    Public Sub Save()
        If Not Directory.Exists("./data") Then
            Directory.CreateDirectory("./data")
        End If
        Dim json = JsonConvert.SerializeObject(RecommendedDailyIntakes)
        File.WriteAllText("./data/recommended.json", json)
    End Sub
End Class
