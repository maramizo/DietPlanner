Imports System.IO
Imports Newtonsoft.Json

Public NotInheritable Class AddMealResult
    Public ReadOnly Property Added As Boolean
    Public ReadOnly Property Meal As Meal

    Public Sub New(added As Boolean, meal As Meal)
        Me.Added = added
        Me.Meal = meal
    End Sub
End Class

Public Module MealRepository
    Public Const CurrentCategoryVersion As Integer = 2

    Private ReadOnly DataDirectory As String = Path.Combine(".", "data")
    Private ReadOnly MealsPath As String = Path.Combine(DataDirectory, "meals.json")
    Private ReadOnly CategoryVersionPath As String =
        Path.Combine(DataDirectory, "meal-category-version.txt")
    Private ReadOnly RepositoryMutex As New Threading.Mutex(
        False,
        "Local\DietPlanner.MealRepository"
    )

    Public Function LoadAll() As List(Of Meal)
        Return WithRepositoryLock(Function() LoadAllUnsafe())
    End Function

    Public Sub SaveAll(meals As IEnumerable(Of Meal))
        WithRepositoryLock(
            Sub()
                WriteMealsUnsafe(
                    If(meals, Enumerable.Empty(Of Meal)).ToList()
                )
            End Sub
        )
    End Sub

    Public Sub MergeAll(meals As IEnumerable(Of Meal))
        WithRepositoryLock(
            Sub()
                Dim currentMeals = LoadAllUnsafe()
                For Each meal In If(meals, Enumerable.Empty(Of Meal))
                    If meal Is Nothing Then Continue For
                    Dim existingIndex = FindMealIndex(currentMeals, meal)
                    If existingIndex >= 0 Then
                        currentMeals(existingIndex) = meal
                    Else
                        currentMeals.Add(meal)
                    End If
                Next
                WriteMealsUnsafe(currentMeals)
            End Sub
        )
    End Sub

    Public Function AddIfMissing(meal As Meal) As AddMealResult
        If meal Is Nothing Then Throw New ArgumentNullException(NameOf(meal))

        Return WithRepositoryLock(
            Function()
                Dim meals = LoadAllUnsafe()
                Dim existingIndex = FindMealIndex(meals, meal)
                If existingIndex >= 0 Then
                    Return New AddMealResult(False, meals(existingIndex))
                End If

                meals.Add(meal)
                WriteMealsUnsafe(meals)
                Return New AddMealResult(True, meal)
            End Function
        )
    End Function

    Public Function GetMealsFilePath() As String
        Return Path.GetFullPath(MealsPath)
    End Function

    Public Function GetDataDirectoryPath() As String
        Return Path.GetFullPath(DataDirectory)
    End Function

    Private Function LoadAllUnsafe() As List(Of Meal)
        If Not File.Exists(MealsPath) Then Return New List(Of Meal)

        Dim json = File.ReadAllText(MealsPath)
        Dim meals = JsonConvert.DeserializeObject(Of List(Of Meal))(json)
        If meals Is Nothing Then meals = New List(Of Meal)
        Return meals.OrderBy(Function(meal) meal.Name).ToList()
    End Function

    Private Sub WriteMealsUnsafe(meals As IEnumerable(Of Meal))
        Directory.CreateDirectory(DataDirectory)
        WriteTextAtomically(
            MealsPath,
            JsonConvert.SerializeObject(
                If(meals, Enumerable.Empty(Of Meal)).
                    Where(Function(meal) meal IsNot Nothing).
                    OrderBy(Function(meal) meal.Name).
                    ToList(),
                Formatting.Indented
            )
        )
    End Sub

    Public Function LoadCategoryVersion() As Integer
        Return WithRepositoryLock(
            Function()
                Try
                    Dim version As Integer
                    If Integer.TryParse(
                        File.ReadAllText(CategoryVersionPath).Trim(),
                        version
                    ) Then
                        Return Math.Max(0, version)
                    End If
                Catch ex As IOException
                Catch ex As UnauthorizedAccessException
                End Try
                Return 0
            End Function
        )
    End Function

    Public Sub SaveCurrentCategoryVersion()
        WithRepositoryLock(
            Sub()
                Directory.CreateDirectory(DataDirectory)
                WriteTextAtomically(
                    CategoryVersionPath,
                    CurrentCategoryVersion.ToString(
                        Globalization.CultureInfo.InvariantCulture
                    )
                )
            End Sub
        )
    End Sub

    Private Function FindMealIndex(
        meals As IList(Of Meal),
        candidate As Meal
    ) As Integer
        If meals Is Nothing OrElse candidate Is Nothing Then Return -1

        Dim candidateUrl = NormalizeRecipeUrl(candidate.Recipe)
        For index As Integer = 0 To meals.Count - 1
            Dim existing = meals(index)
            If existing Is Nothing Then Continue For
            Dim existingUrl = NormalizeRecipeUrl(existing.Recipe)
            If candidateUrl <> String.Empty AndAlso existingUrl <> String.Empty Then
                If String.Equals(
                    candidateUrl,
                    existingUrl,
                    StringComparison.OrdinalIgnoreCase
                ) Then
                    Return index
                End If
                Continue For
            End If
            If String.Equals(
                If(candidate.Name, String.Empty).Trim(),
                If(existing.Name, String.Empty).Trim(),
                StringComparison.CurrentCultureIgnoreCase
            ) Then
                Return index
            End If
        Next
        Return -1
    End Function

    Private Function NormalizeRecipeUrl(value As String) As String
        Dim recipeUri As Uri = Nothing
        If Not Uri.TryCreate(value, UriKind.Absolute, recipeUri) Then
            Return If(value, String.Empty).Trim()
        End If

        Dim builder As New UriBuilder(recipeUri) With {.Fragment = String.Empty}
        Return builder.Uri.AbsoluteUri.TrimEnd("/"c)
    End Function

    Private Sub WriteTextAtomically(filePath As String, contents As String)
        Dim directoryPath = Path.GetDirectoryName(Path.GetFullPath(filePath))
        Directory.CreateDirectory(directoryPath)
        Dim temporaryPath = Path.Combine(
            directoryPath,
            "." & Path.GetFileName(filePath) & "." & Guid.NewGuid().ToString("N") & ".tmp"
        )
        Try
            File.WriteAllText(temporaryPath, contents)
            File.Move(temporaryPath, filePath, overwrite:=True)
        Finally
            Try
                If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
            Catch ex As IOException
            Catch ex As UnauthorizedAccessException
            End Try
        End Try
    End Sub

    Private Function WithRepositoryLock(Of T)(action As Func(Of T)) As T
        Dim lockTaken = False
        Try
            Try
                lockTaken = RepositoryMutex.WaitOne(TimeSpan.FromSeconds(30))
            Catch ex As Threading.AbandonedMutexException
                lockTaken = True
            End Try
            If Not lockTaken Then
                Throw New TimeoutException(
                    "DietPlanner could not access the recipe collection because another process is still saving it."
                )
            End If
            Return action()
        Finally
            If lockTaken Then RepositoryMutex.ReleaseMutex()
        End Try
    End Function

    Private Sub WithRepositoryLock(action As Action)
        WithRepositoryLock(
            Function()
                action()
                Return True
            End Function
        )
    End Sub
End Module
