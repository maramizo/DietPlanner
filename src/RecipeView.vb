Public Class RecipeView

    Public Sub New(meal As Meal)
        InitializeComponent()
        ApplyAppIcon(Me)
        NotifyIcon1.Icon = DirectCast(Icon.Clone(), Icon)
        Label1.Text = meal.Name
        For Each nutritional As KeyValuePair(Of String, String) In meal.ViewNutritionals()
            DataGridView1.Rows.Add(nutritional.Key, nutritional.Value)
        Next
        DataGridView1.AllowUserToAddRows = False
        PrepTime.Text = meal.PrepTime & " minutes"
        CookTime.Text = meal.CookTime & " minutes"
        TotalTime.Text = meal.TotalTime & " minutes"
        MealTypesLabel.Text = If(
            meal.MealTypes Is Nothing OrElse meal.MealTypes.Count = 0,
            "Not categorized",
            String.Join(", ", meal.MealTypes)
        )
        AdvancedDetailsStatusLabel.Text = If(
            meal.IsAdvancedScrapeUnavailable(),
            "Source unavailable",
            If(meal.NeedsAdvancedScrape(), "Pending", "Complete")
        )
        If meal.Ingredients IsNot Nothing Then
            For Each ingredient In meal.Ingredients
                IngredientsDataGrid.Rows.Add(
                    ingredient.Ingredient,
                    ingredient.Amount
                )
            Next
        End If
        PreparationMethodTextBox.Text = If(
            String.IsNullOrWhiteSpace(meal.PreparationMethod),
            If(
                meal.IsAdvancedScrapeUnavailable(),
                "Source unavailable. Automatic extraction will not be retried.",
                "Not available"
            ),
            meal.PreparationMethod
        )
    End Sub

    Private Sub RecipeView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        NotifyIcon1.BalloonTipText = "This is a recipe view"
        NotifyIcon1.BalloonTipTitle = "Recipe View"
        NotifyIcon1.ShowBalloonTip(1000)
    End Sub
End Class
