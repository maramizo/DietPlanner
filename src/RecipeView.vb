Public Class RecipeView
    Public Property meal As Meal

    Public Sub New(meal As Meal)
        InitializeComponent()
        Me.meal = meal
        Label1.Text = meal.Name
        For Each nutritional As KeyValuePair(Of String, String) In meal.ViewNutritionals()
            DataGridView1.Rows.Add(nutritional.Key, nutritional.Value)
        Next
        DataGridView1.AllowUserToAddRows = False
        PrepTime.Text = meal.PrepTime & " minutes"
        CookTime.Text = meal.CookTime & " minutes"
        TotalTime.Text = meal.TotalTime & " minutes"
    End Sub

    Private Sub RecipeView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        NotifyIcon1.BalloonTipText = "This is a recipe view"
        NotifyIcon1.BalloonTipTitle = "Recipe View"
        NotifyIcon1.ShowBalloonTip(1000)
    End Sub
End Class