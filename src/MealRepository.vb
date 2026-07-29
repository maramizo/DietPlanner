Imports System.IO
Imports Newtonsoft.Json

Public Module MealRepository
    Public Const CurrentCategoryVersion As Integer = 2

    Private ReadOnly DataDirectory As String = Path.Combine(".", "data")
    Private ReadOnly MealsPath As String = Path.Combine(DataDirectory, "meals.json")
    Private ReadOnly CategoryVersionPath As String =
        Path.Combine(DataDirectory, "meal-category-version.txt")

    Public Function LoadAll() As List(Of Meal)
        If Not File.Exists(MealsPath) Then Return New List(Of Meal)

        Dim json = File.ReadAllText(MealsPath)
        Dim meals = JsonConvert.DeserializeObject(Of List(Of Meal))(json)
        If meals Is Nothing Then meals = New List(Of Meal)
        Return meals.OrderBy(Function(meal) meal.Name).ToList()
    End Function

    Public Sub SaveAll(meals As IEnumerable(Of Meal))
        Directory.CreateDirectory(DataDirectory)
        File.WriteAllText(
            MealsPath,
            JsonConvert.SerializeObject(
                If(meals, Enumerable.Empty(Of Meal)).ToList(),
                Formatting.Indented
            )
        )
    End Sub

    Public Function LoadCategoryVersion() As Integer
        Try
            Dim version As Integer
            If Integer.TryParse(File.ReadAllText(CategoryVersionPath).Trim(), version) Then
                Return Math.Max(0, version)
            End If
        Catch ex As IOException
        Catch ex As UnauthorizedAccessException
        End Try
        Return 0
    End Function

    Public Sub SaveCurrentCategoryVersion()
        Directory.CreateDirectory(DataDirectory)
        File.WriteAllText(
            CategoryVersionPath,
            CurrentCategoryVersion.ToString(
                Globalization.CultureInfo.InvariantCulture
            )
        )
    End Sub
End Module
