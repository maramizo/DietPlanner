<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RecipeView
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
        components = New ComponentModel.Container()
        NotifyIcon1 = New NotifyIcon(components)
        DataGridView1 = New DataGridView()
        Nutrition = New DataGridViewTextBoxColumn()
        Value = New DataGridViewTextBoxColumn()
        Label1 = New Label()
        Label2 = New Label()
        Label3 = New Label()
        Label4 = New Label()
        Label5 = New Label()
        TotalTime = New Label()
        CookTime = New Label()
        PrepTime = New Label()
        ServingsTitleLabel = New Label()
        ServingsValueLabel = New Label()
        CaloriesPerServingTitleLabel = New Label()
        CaloriesPerServingValueLabel = New Label()
        BatchCaloriesTitleLabel = New Label()
        BatchCaloriesValueLabel = New Label()
        MealTypeTitleLabel = New Label()
        MealTypesLabel = New Label()
        AdvancedDetailsTitleLabel = New Label()
        AdvancedDetailsStatusLabel = New Label()
        IngredientsLabel = New Label()
        IngredientsDataGrid = New DataGridView()
        IngredientNameColumn = New DataGridViewTextBoxColumn()
        IngredientDetailsColumn = New DataGridViewTextBoxColumn()
        IngredientAmountColumn = New DataGridViewTextBoxColumn()
        PreparationMethodLabel = New Label()
        PreparationMethodTextBox = New TextBox()
        NotesLabel = New Label()
        NotesTextBox = New TextBox()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        CType(IngredientsDataGrid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' NotifyIcon1
        '
        NotifyIcon1.Text = "NotifyIcon1"
        NotifyIcon1.Visible = True
        '
        ' DataGridView1
        '
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Columns.AddRange(New DataGridViewColumn() {Nutrition, Value})
        DataGridView1.Location = New Point(34, 251)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(387, 355)
        DataGridView1.TabIndex = 0
        '
        ' Nutrition
        '
        Nutrition.HeaderText = "Nutrition"
        Nutrition.Name = "Nutrition"
        Nutrition.ReadOnly = True
        '
        ' Value
        '
        Value.HeaderText = "Value"
        Value.Name = "Value"
        Value.ReadOnly = True
        '
        ' Label1
        '
        Label1.AutoSize = True
        Label1.Location = New Point(211, 20)
        Label1.Name = "Label1"
        Label1.Size = New Size(41, 15)
        Label1.TabIndex = 1
        Label1.Text = "Label1"
        '
        ' Label2
        '
        Label2.AutoSize = True
        Label2.Location = New Point(34, 20)
        Label2.Name = "Label2"
        Label2.Size = New Size(39, 15)
        Label2.TabIndex = 2
        Label2.Text = "Name"
        '
        ' Label3
        '
        Label3.AutoSize = True
        Label3.Location = New Point(34, 47)
        Label3.Name = "Label3"
        Label3.Size = New Size(60, 15)
        Label3.TabIndex = 3
        Label3.Text = "Prep Time"
        '
        ' Label4
        '
        Label4.AutoSize = True
        Label4.Location = New Point(34, 72)
        Label4.Name = "Label4"
        Label4.Size = New Size(64, 15)
        Label4.TabIndex = 4
        Label4.Text = "Cook Time"
        '
        ' Label5
        '
        Label5.AutoSize = True
        Label5.Location = New Point(34, 98)
        Label5.Name = "Label5"
        Label5.Size = New Size(61, 15)
        Label5.TabIndex = 5
        Label5.Text = "Total Time"
        '
        ' TotalTime
        '
        TotalTime.AutoSize = True
        TotalTime.Location = New Point(211, 98)
        TotalTime.Name = "TotalTime"
        TotalTime.Size = New Size(61, 15)
        TotalTime.TabIndex = 8
        TotalTime.Text = "Total Time"
        '
        ' CookTime
        '
        CookTime.AutoSize = True
        CookTime.Location = New Point(211, 72)
        CookTime.Name = "CookTime"
        CookTime.Size = New Size(64, 15)
        CookTime.TabIndex = 7
        CookTime.Text = "Cook Time"
        '
        ' PrepTime
        '
        PrepTime.AutoSize = True
        PrepTime.Location = New Point(211, 47)
        PrepTime.Name = "PrepTime"
        PrepTime.Size = New Size(60, 15)
        PrepTime.TabIndex = 6
        PrepTime.Text = "Prep Time"
        '
        ' ServingsTitleLabel
        '
        ServingsTitleLabel.AutoSize = True
        ServingsTitleLabel.Location = New Point(34, 123)
        ServingsTitleLabel.Name = "ServingsTitleLabel"
        ServingsTitleLabel.Size = New Size(50, 15)
        ServingsTitleLabel.TabIndex = 17
        ServingsTitleLabel.Text = "Servings"
        '
        ' ServingsValueLabel
        '
        ServingsValueLabel.AutoSize = True
        ServingsValueLabel.Location = New Point(211, 123)
        ServingsValueLabel.Name = "ServingsValueLabel"
        ServingsValueLabel.Size = New Size(50, 15)
        ServingsValueLabel.TabIndex = 18
        ServingsValueLabel.Text = "Servings"
        '
        ' CaloriesPerServingTitleLabel
        '
        CaloriesPerServingTitleLabel.AutoSize = True
        CaloriesPerServingTitleLabel.Location = New Point(34, 148)
        CaloriesPerServingTitleLabel.Name = "CaloriesPerServingTitleLabel"
        CaloriesPerServingTitleLabel.Size = New Size(99, 15)
        CaloriesPerServingTitleLabel.TabIndex = 19
        CaloriesPerServingTitleLabel.Text = "Calories / Serving"
        '
        ' CaloriesPerServingValueLabel
        '
        CaloriesPerServingValueLabel.AutoSize = True
        CaloriesPerServingValueLabel.Location = New Point(211, 148)
        CaloriesPerServingValueLabel.Name = "CaloriesPerServingValueLabel"
        CaloriesPerServingValueLabel.Size = New Size(47, 15)
        CaloriesPerServingValueLabel.TabIndex = 20
        CaloriesPerServingValueLabel.Text = "Calories"
        '
        ' BatchCaloriesTitleLabel
        '
        BatchCaloriesTitleLabel.AutoSize = True
        BatchCaloriesTitleLabel.Location = New Point(34, 173)
        BatchCaloriesTitleLabel.Name = "BatchCaloriesTitleLabel"
        BatchCaloriesTitleLabel.Size = New Size(109, 15)
        BatchCaloriesTitleLabel.TabIndex = 21
        BatchCaloriesTitleLabel.Text = "Total Batch Calories"
        '
        ' BatchCaloriesValueLabel
        '
        BatchCaloriesValueLabel.AutoSize = True
        BatchCaloriesValueLabel.Location = New Point(211, 173)
        BatchCaloriesValueLabel.Name = "BatchCaloriesValueLabel"
        BatchCaloriesValueLabel.Size = New Size(47, 15)
        BatchCaloriesValueLabel.TabIndex = 22
        BatchCaloriesValueLabel.Text = "Calories"
        '
        ' MealTypeTitleLabel
        '
        MealTypeTitleLabel.AutoSize = True
        MealTypeTitleLabel.Location = New Point(34, 198)
        MealTypeTitleLabel.Name = "MealTypeTitleLabel"
        MealTypeTitleLabel.Size = New Size(60, 15)
        MealTypeTitleLabel.TabIndex = 9
        MealTypeTitleLabel.Text = "Meal Type"
        '
        ' MealTypesLabel
        '
        MealTypesLabel.AutoSize = True
        MealTypesLabel.Location = New Point(211, 198)
        MealTypesLabel.Name = "MealTypesLabel"
        MealTypesLabel.Size = New Size(63, 15)
        MealTypesLabel.TabIndex = 10
        MealTypesLabel.Text = "Meal Types"
        '
        ' AdvancedDetailsTitleLabel
        '
        AdvancedDetailsTitleLabel.AutoSize = True
        AdvancedDetailsTitleLabel.Location = New Point(34, 223)
        AdvancedDetailsTitleLabel.Name = "AdvancedDetailsTitleLabel"
        AdvancedDetailsTitleLabel.Size = New Size(99, 15)
        AdvancedDetailsTitleLabel.TabIndex = 15
        AdvancedDetailsTitleLabel.Text = "Advanced Details"
        '
        ' AdvancedDetailsStatusLabel
        '
        AdvancedDetailsStatusLabel.AutoSize = True
        AdvancedDetailsStatusLabel.Location = New Point(211, 223)
        AdvancedDetailsStatusLabel.Name = "AdvancedDetailsStatusLabel"
        AdvancedDetailsStatusLabel.Size = New Size(39, 15)
        AdvancedDetailsStatusLabel.TabIndex = 16
        AdvancedDetailsStatusLabel.Text = "Status"
        '
        ' IngredientsLabel
        '
        IngredientsLabel.AutoSize = True
        IngredientsLabel.Location = New Point(460, 23)
        IngredientsLabel.Name = "IngredientsLabel"
        IngredientsLabel.Size = New Size(65, 15)
        IngredientsLabel.TabIndex = 11
        IngredientsLabel.Text = "Ingredients"
        '
        ' IngredientsDataGrid
        '
        IngredientsDataGrid.AllowUserToAddRows = False
        IngredientsDataGrid.AllowUserToDeleteRows = False
        IngredientsDataGrid.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        IngredientsDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        IngredientsDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        IngredientsDataGrid.Columns.AddRange(New DataGridViewColumn() {
            IngredientNameColumn,
            IngredientDetailsColumn,
            IngredientAmountColumn
        })
        IngredientsDataGrid.Location = New Point(460, 45)
        IngredientsDataGrid.Name = "IngredientsDataGrid"
        IngredientsDataGrid.ReadOnly = True
        IngredientsDataGrid.Size = New Size(410, 180)
        IngredientsDataGrid.TabIndex = 12
        '
        ' IngredientNameColumn
        '
        IngredientNameColumn.FillWeight = 40.0!
        IngredientNameColumn.HeaderText = "Ingredient"
        IngredientNameColumn.Name = "IngredientNameColumn"
        IngredientNameColumn.ReadOnly = True
        '
        ' IngredientDetailsColumn
        '
        IngredientDetailsColumn.FillWeight = 32.0!
        IngredientDetailsColumn.HeaderText = "Details"
        IngredientDetailsColumn.Name = "IngredientDetailsColumn"
        IngredientDetailsColumn.ReadOnly = True
        '
        ' IngredientAmountColumn
        '
        IngredientAmountColumn.FillWeight = 28.0!
        IngredientAmountColumn.HeaderText = "Amount"
        IngredientAmountColumn.Name = "IngredientAmountColumn"
        IngredientAmountColumn.ReadOnly = True
        '
        ' PreparationMethodLabel
        '
        PreparationMethodLabel.AutoSize = True
        PreparationMethodLabel.Location = New Point(460, 245)
        PreparationMethodLabel.Name = "PreparationMethodLabel"
        PreparationMethodLabel.Size = New Size(118, 15)
        PreparationMethodLabel.TabIndex = 13
        PreparationMethodLabel.Text = "Preparation Method"
        '
        ' PreparationMethodTextBox
        '
        PreparationMethodTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PreparationMethodTextBox.Location = New Point(460, 268)
        PreparationMethodTextBox.Multiline = True
        PreparationMethodTextBox.Name = "PreparationMethodTextBox"
        PreparationMethodTextBox.ReadOnly = True
        PreparationMethodTextBox.ScrollBars = ScrollBars.Vertical
        PreparationMethodTextBox.Size = New Size(410, 178)
        PreparationMethodTextBox.TabIndex = 14
        '
        ' NotesLabel
        '
        NotesLabel.AutoSize = True
        NotesLabel.Location = New Point(460, 466)
        NotesLabel.Name = "NotesLabel"
        NotesLabel.Size = New Size(205, 15)
        NotesLabel.TabIndex = 17
        NotesLabel.Text = "Notes (Storage, Freezing, Variations)"
        '
        ' NotesTextBox
        '
        NotesTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        NotesTextBox.Location = New Point(460, 489)
        NotesTextBox.Multiline = True
        NotesTextBox.Name = "NotesTextBox"
        NotesTextBox.ReadOnly = True
        NotesTextBox.ScrollBars = ScrollBars.Vertical
        NotesTextBox.Size = New Size(410, 117)
        NotesTextBox.TabIndex = 18
        '
        ' RecipeView
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(900, 660)
        Controls.Add(BatchCaloriesValueLabel)
        Controls.Add(BatchCaloriesTitleLabel)
        Controls.Add(CaloriesPerServingValueLabel)
        Controls.Add(CaloriesPerServingTitleLabel)
        Controls.Add(ServingsValueLabel)
        Controls.Add(ServingsTitleLabel)
        Controls.Add(NotesTextBox)
        Controls.Add(NotesLabel)
        Controls.Add(PreparationMethodTextBox)
        Controls.Add(PreparationMethodLabel)
        Controls.Add(IngredientsDataGrid)
        Controls.Add(IngredientsLabel)
        Controls.Add(AdvancedDetailsStatusLabel)
        Controls.Add(AdvancedDetailsTitleLabel)
        Controls.Add(MealTypesLabel)
        Controls.Add(MealTypeTitleLabel)
        Controls.Add(TotalTime)
        Controls.Add(CookTime)
        Controls.Add(PrepTime)
        Controls.Add(Label5)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(DataGridView1)
        MinimumSize = New Size(916, 699)
        Name = "RecipeView"
        StartPosition = FormStartPosition.CenterScreen
        Text = "View Recipe"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        CType(IngredientsDataGrid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents Nutrition As DataGridViewTextBoxColumn
    Friend WithEvents Value As DataGridViewTextBoxColumn
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents TotalTime As Label
    Friend WithEvents CookTime As Label
    Friend WithEvents PrepTime As Label
    Friend WithEvents ServingsTitleLabel As Label
    Friend WithEvents ServingsValueLabel As Label
    Friend WithEvents CaloriesPerServingTitleLabel As Label
    Friend WithEvents CaloriesPerServingValueLabel As Label
    Friend WithEvents BatchCaloriesTitleLabel As Label
    Friend WithEvents BatchCaloriesValueLabel As Label
    Friend WithEvents MealTypeTitleLabel As Label
    Friend WithEvents MealTypesLabel As Label
    Friend WithEvents AdvancedDetailsTitleLabel As Label
    Friend WithEvents AdvancedDetailsStatusLabel As Label
    Friend WithEvents IngredientsLabel As Label
    Friend WithEvents IngredientsDataGrid As DataGridView
    Friend WithEvents IngredientNameColumn As DataGridViewTextBoxColumn
    Friend WithEvents IngredientDetailsColumn As DataGridViewTextBoxColumn
    Friend WithEvents IngredientAmountColumn As DataGridViewTextBoxColumn
    Friend WithEvents PreparationMethodLabel As Label
    Friend WithEvents PreparationMethodTextBox As TextBox
    Friend WithEvents NotesLabel As Label
    Friend WithEvents NotesTextBox As TextBox
End Class
