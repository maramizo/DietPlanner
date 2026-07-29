Imports System.IO
Imports Newtonsoft.Json

Public Module WeekPlanRepository
    Private ReadOnly PlanPath As String =
        Path.Combine(".", "data", "week-plan.json")

    Public Function Load() As WeeklyPlan
        Try
            If Not File.Exists(PlanPath) Then Return Nothing
            Return JsonConvert.DeserializeObject(Of WeeklyPlan)(
                File.ReadAllText(PlanPath)
            )
        Catch ex As IOException
            Return Nothing
        Catch ex As JsonException
            Return Nothing
        End Try
    End Function

    Public Sub Save(plan As WeeklyPlan)
        If plan Is Nothing Then Throw New ArgumentNullException(NameOf(plan))
        Directory.CreateDirectory(Path.GetDirectoryName(PlanPath))
        File.WriteAllText(
            PlanPath,
            JsonConvert.SerializeObject(plan, Formatting.Indented)
        )
    End Sub
End Module
