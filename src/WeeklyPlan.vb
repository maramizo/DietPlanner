Public Class WeeklyPlan
    Public Property GeneratedAt As DateTime
    Public Property Days As List(Of PlannedDay)
    Public Property TargetDailyIntakes As Dictionary(Of String, Double)
    Public Property SelectedRecipeUrls As List(Of String)
    Public Property SelectedRecipeNames As List(Of String)
    <Newtonsoft.Json.JsonProperty(
        ObjectCreationHandling:=Newtonsoft.Json.ObjectCreationHandling.Replace
    )>
    Public Property PlannedMealTypes As List(Of String)
    Public Property GenerationMode As String
    Public Property RandomSeed As Integer
    Public Property IngredientFilterApplied As Boolean
    Public Property AllowedIngredientNames As List(Of String)
    Public Property IngredientDisplayMeasurements As Dictionary(Of String, String)

    Public Sub New()
        Days = New List(Of PlannedDay)
        TargetDailyIntakes = New Dictionary(Of String, Double)
        SelectedRecipeUrls = New List(Of String)
        SelectedRecipeNames = New List(Of String)
        PlannedMealTypes = New List(Of String)(WeekPlanGenerator.MealTypes)
        AllowedIngredientNames = New List(Of String)
        IngredientDisplayMeasurements = New Dictionary(Of String, String)(
            StringComparer.OrdinalIgnoreCase
        )
        GenerationMode = WeekPlanGenerationMode.SelectedRecipesOnly.ToString()
    End Sub
End Class

Public Class PlannedDay
    Public Property Name As String
    Public Property Meals As List(Of PlannedMeal)

    Public Sub New()
        Meals = New List(Of PlannedMeal)
    End Sub
End Class

Public Class PlannedMeal
    Public Property MealType As String
    Public Property MealName As String
    Public Property RecipeUrl As String
    Public Property Calories As Integer
    Public Property Nutritionals As Dictionary(Of String, Double)
    Public Property RecipeServings As Integer
    Public Property Ingredients As List(Of RecipeIngredient)

    Public Sub New()
        Nutritionals = New Dictionary(Of String, Double)
        Ingredients = New List(Of RecipeIngredient)
    End Sub
End Class

Public Class WeeklyPlanException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class
