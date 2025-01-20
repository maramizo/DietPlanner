<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AddRecipe
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
        NutritionalsDataGrid = New DataGridView()
        Nutrition = New DataGridViewTextBoxColumn()
        Value = New DataGridViewTextBoxColumn()
        Label1 = New Label()
        Label2 = New Label()
        NameTextBox = New TextBox()
        CaloriesTextBox = New TextBox()
        SaveButton = New Button()
        CancelButton = New Button()
        RecipeTextBox = New TextBox()
        Label3 = New Label()
        PrepTimeTextBox = New TextBox()
        PrepTimeLabel = New Label()
        CookTimeTextBox = New TextBox()
        CookTime = New Label()
        ScrapeButton = New Button()
        CType(NutritionalsDataGrid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' NutritionalsDataGrid
        ' 
        NutritionalsDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        NutritionalsDataGrid.Columns.AddRange(New DataGridViewColumn() {Nutrition, Value})
        NutritionalsDataGrid.Location = New Point(28, 211)
        NutritionalsDataGrid.Name = "NutritionalsDataGrid"
        NutritionalsDataGrid.Size = New Size(328, 343)
        NutritionalsDataGrid.TabIndex = 0
        ' 
        ' Nutrition
        ' 
        Nutrition.HeaderText = "Nutrition"
        Nutrition.Name = "Nutrition"
        ' 
        ' Value
        ' 
        Value.HeaderText = "Value"
        Value.Name = "Value"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(28, 23)
        Label1.Name = "Label1"
        Label1.Size = New Size(39, 15)
        Label1.TabIndex = 1
        Label1.Text = "Name"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(28, 62)
        Label2.Name = "Label2"
        Label2.Size = New Size(49, 15)
        Label2.TabIndex = 2
        Label2.Text = "Calories"
        ' 
        ' NameTextBox
        ' 
        NameTextBox.Location = New Point(121, 23)
        NameTextBox.Name = "NameTextBox"
        NameTextBox.Size = New Size(233, 23)
        NameTextBox.TabIndex = 3
        ' 
        ' CaloriesTextBox
        ' 
        CaloriesTextBox.Location = New Point(121, 59)
        CaloriesTextBox.Name = "CaloriesTextBox"
        CaloriesTextBox.Size = New Size(233, 23)
        CaloriesTextBox.TabIndex = 4
        ' 
        ' SaveButton
        ' 
        SaveButton.Location = New Point(28, 568)
        SaveButton.Name = "SaveButton"
        SaveButton.Size = New Size(108, 23)
        SaveButton.TabIndex = 5
        SaveButton.Text = "Save"
        SaveButton.UseVisualStyleBackColor = True
        ' 
        ' CancelButton
        ' 
        CancelButton.Location = New Point(246, 568)
        CancelButton.Name = "CancelButton"
        CancelButton.Size = New Size(108, 23)
        CancelButton.TabIndex = 6
        CancelButton.Text = "Cancel"
        CancelButton.UseVisualStyleBackColor = True
        ' 
        ' RecipeTextBox
        ' 
        RecipeTextBox.Location = New Point(121, 98)
        RecipeTextBox.Name = "RecipeTextBox"
        RecipeTextBox.Size = New Size(152, 23)
        RecipeTextBox.TabIndex = 8
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(28, 101)
        Label3.Name = "Label3"
        Label3.Size = New Size(67, 15)
        Label3.TabIndex = 7
        Label3.Text = "Recipe Link"
        ' 
        ' PrepTimeTextBox
        ' 
        PrepTimeTextBox.Location = New Point(121, 137)
        PrepTimeTextBox.Name = "PrepTimeTextBox"
        PrepTimeTextBox.Size = New Size(233, 23)
        PrepTimeTextBox.TabIndex = 10
        ' 
        ' PrepTimeLabel
        ' 
        PrepTimeLabel.AutoSize = True
        PrepTimeLabel.Location = New Point(28, 140)
        PrepTimeLabel.Name = "PrepTimeLabel"
        PrepTimeLabel.Size = New Size(60, 15)
        PrepTimeLabel.TabIndex = 9
        PrepTimeLabel.Text = "Prep Time"
        ' 
        ' CookTimeTextBox
        ' 
        CookTimeTextBox.Location = New Point(121, 173)
        CookTimeTextBox.Name = "CookTimeTextBox"
        CookTimeTextBox.Size = New Size(233, 23)
        CookTimeTextBox.TabIndex = 12
        ' 
        ' CookTime
        ' 
        CookTime.AutoSize = True
        CookTime.Location = New Point(28, 176)
        CookTime.Name = "CookTime"
        CookTime.Size = New Size(64, 15)
        CookTime.TabIndex = 11
        CookTime.Text = "Cook Time"
        ' 
        ' ScrapeButton
        ' 
        ScrapeButton.Location = New Point(281, 98)
        ScrapeButton.Name = "ScrapeButton"
        ScrapeButton.Size = New Size(75, 23)
        ScrapeButton.TabIndex = 13
        ScrapeButton.Text = "Scrape"
        ScrapeButton.UseVisualStyleBackColor = True
        ' 
        ' AddRecipe
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(398, 603)
        Controls.Add(ScrapeButton)
        Controls.Add(CookTimeTextBox)
        Controls.Add(CookTime)
        Controls.Add(PrepTimeTextBox)
        Controls.Add(PrepTimeLabel)
        Controls.Add(RecipeTextBox)
        Controls.Add(Label3)
        Controls.Add(CancelButton)
        Controls.Add(SaveButton)
        Controls.Add(CaloriesTextBox)
        Controls.Add(NameTextBox)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(NutritionalsDataGrid)
        Name = "AddRecipe"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Add Recipe"
        CType(NutritionalsDataGrid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents NutritionalsDataGrid As DataGridView
    Friend WithEvents Nutrition As DataGridViewTextBoxColumn
    Friend WithEvents Value As DataGridViewTextBoxColumn
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents NameTextBox As TextBox
    Friend WithEvents CaloriesTextBox As TextBox
    Friend WithEvents SaveButton As Button
    Friend WithEvents CancelButton As Button
    Friend WithEvents RecipeTextBox As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents PrepTimeTextBox As TextBox
    Friend WithEvents PrepTimeLabel As Label
    Friend WithEvents CookTimeTextBox As TextBox
    Friend WithEvents CookTime As Label
    Friend WithEvents ScrapeButton As Button
End Class
