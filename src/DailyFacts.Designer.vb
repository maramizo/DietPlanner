<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DailyFacts
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Label1 = New Label()
        Label2 = New Label()
        NutritionalsDataGrid = New DataGridView()
        Nutrition = New DataGridViewTextBoxColumn()
        Amount = New DataGridViewTextBoxColumn()
        PctDaily = New DataGridViewTextBoxColumn()
        Recommended = New DataGridViewTextBoxColumn()
        CaloriesLabel = New Label()
        CookTimeLabel = New Label()
        CancelButton = New Button()
        ChangeButton = New Button()
        CType(NutritionalsDataGrid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(42, 30)
        Label1.Name = "Label1"
        Label1.Size = New Size(77, 15)
        Label1.TabIndex = 1
        Label1.Text = "Total Calories"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(42, 62)
        Label2.Name = "Label2"
        Label2.Size = New Size(92, 15)
        Label2.TabIndex = 2
        Label2.Text = "Total Cook Time"
        ' 
        ' NutritionalsDataGrid
        ' 
        NutritionalsDataGrid.AllowUserToAddRows = False
        NutritionalsDataGrid.AllowUserToDeleteRows = False
        NutritionalsDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        NutritionalsDataGrid.Columns.AddRange(New DataGridViewColumn() {Nutrition, Amount, PctDaily, Recommended})
        NutritionalsDataGrid.Location = New Point(42, 99)
        NutritionalsDataGrid.Name = "NutritionalsDataGrid"
        NutritionalsDataGrid.ReadOnly = True
        NutritionalsDataGrid.Size = New Size(413, 495)
        NutritionalsDataGrid.TabIndex = 3
        ' 
        ' Nutrition
        ' 
        Nutrition.HeaderText = "Nutrition"
        Nutrition.Name = "Nutrition"
        Nutrition.ReadOnly = True
        ' 
        ' Amount
        ' 
        Amount.HeaderText = "Amount"
        Amount.Name = "Amount"
        Amount.ReadOnly = True
        ' 
        ' PctDaily
        ' 
        PctDaily.HeaderText = "% of Rec. Daily"
        PctDaily.Name = "PctDaily"
        PctDaily.ReadOnly = True
        ' 
        ' Recommended
        ' 
        Recommended.HeaderText = "Rec. Daily"
        Recommended.Name = "Recommended"
        Recommended.ReadOnly = True
        Recommended.Width = 70
        ' 
        ' CaloriesLabel
        ' 
        CaloriesLabel.AutoSize = True
        CaloriesLabel.Location = New Point(186, 30)
        CaloriesLabel.Name = "CaloriesLabel"
        CaloriesLabel.Size = New Size(77, 15)
        CaloriesLabel.TabIndex = 4
        CaloriesLabel.Text = "Total Calories"
        ' 
        ' CookTimeLabel
        ' 
        CookTimeLabel.AutoSize = True
        CookTimeLabel.Location = New Point(186, 62)
        CookTimeLabel.Name = "CookTimeLabel"
        CookTimeLabel.Size = New Size(88, 15)
        CookTimeLabel.TabIndex = 5
        CookTimeLabel.Text = "Total Prep Time"
        ' 
        ' CancelButton
        ' 
        CancelButton.Location = New Point(42, 608)
        CancelButton.Name = "CancelButton"
        CancelButton.Size = New Size(158, 23)
        CancelButton.TabIndex = 6
        CancelButton.Text = "Back to Meals"
        CancelButton.UseVisualStyleBackColor = True
        ' 
        ' ChangeButton
        ' 
        ChangeButton.Location = New Point(245, 608)
        ChangeButton.Name = "ChangeButton"
        ChangeButton.Size = New Size(210, 23)
        ChangeButton.TabIndex = 7
        ChangeButton.Text = "Change my Recommended Intake"
        ChangeButton.UseVisualStyleBackColor = True
        ' 
        ' DailyFacts
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(500, 643)
        Controls.Add(ChangeButton)
        Controls.Add(CancelButton)
        Controls.Add(CookTimeLabel)
        Controls.Add(CaloriesLabel)
        Controls.Add(NutritionalsDataGrid)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Name = "DailyFacts"
        Text = "DailyFacts"
        CType(NutritionalsDataGrid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents NutritionalsDataGrid As DataGridView
    Friend WithEvents CaloriesLabel As Label
    Friend WithEvents CookTimeLabel As Label
    Friend WithEvents Nutrition As DataGridViewTextBoxColumn
    Friend WithEvents Amount As DataGridViewTextBoxColumn
    Friend WithEvents PctDaily As DataGridViewTextBoxColumn
    Friend WithEvents Recommended As DataGridViewTextBoxColumn
    Friend WithEvents CancelButton As Button
    Friend WithEvents ChangeButton As Button
End Class
