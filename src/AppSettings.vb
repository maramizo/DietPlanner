Imports Newtonsoft.Json
Imports System.IO

Public Class DietPlannerSettings
    Public Property ThemeKey As String = ThemeManager.DefaultThemeKey
End Class

Public NotInheritable Class AppSettingsRepository
    Private Shared ReadOnly SettingsPath As String = Path.Combine(
        ".",
        "data",
        "settings.json"
    )

    Private Sub New()
    End Sub

    Public Shared Function Load() As DietPlannerSettings
        Try
            If Not File.Exists(SettingsPath) Then
                Return New DietPlannerSettings()
            End If

            Dim settings = JsonConvert.DeserializeObject(Of DietPlannerSettings)(
                File.ReadAllText(SettingsPath)
            )
            If settings Is Nothing Then Return New DietPlannerSettings()
            Return settings
        Catch
            Return New DietPlannerSettings()
        End Try
    End Function

    Public Shared Sub Save(settings As DietPlannerSettings)
        If settings Is Nothing Then Throw New ArgumentNullException(NameOf(settings))
        Dim settingsDirectory = Path.GetDirectoryName(SettingsPath)
        If Not Directory.Exists(settingsDirectory) Then
            Directory.CreateDirectory(settingsDirectory)
        End If
        File.WriteAllText(
            SettingsPath,
            JsonConvert.SerializeObject(settings, Formatting.Indented)
        )
    End Sub
End Class
