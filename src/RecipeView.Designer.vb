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
        MealTypeTitleLabel = New Label()
        MealTypesLabel = New Label()
        AdvancedDetailsTitleLabel = New Label()
        AdvancedDetailsStatusLabel = New Label()
        IngredientsLabel = New Label()
        IngredientsDataGrid = New DataGridView()
        IngredientNameColumn = New DataGridViewTextBoxColumn()
        IngredientAmountColumn = New DataGridViewTextBoxColumn()
        PreparationMethodLabel = New Label()
        PreparationMethodTextBox = New TextBox()
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
        DataGridView1.Location = New Point(34, 176)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(387, 430)
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
        ' MealTypeTitleLabel
        '
        MealTypeTitleLabel.AutoSize = True
        MealTypeTitleLabel.Location = New Point(34, 123)
        MealTypeTitleLabel.Name = "MealTypeTitleLabel"
        MealTypeTitleLabel.Size = New Size(60, 15)
        MealTypeTitleLabel.TabIndex = 9
        MealTypeTitleLabel.Text = "Meal Type"
        '
        ' MealTypesLabel
        '
        MealTypesLabel.AutoSize = True
        MealTypesLabel.Location = New Point(211, 123)
        MealTypesLabel.Name = "MealTypesLabel"
        MealTypesLabel.Size = New Size(63, 15)
        MealTypesLabel.TabIndex = 10
        MealTypesLabel.Text = "Meal Types"
        '
        ' AdvancedDetailsTitleLabel
        '
        AdvancedDetailsTitleLabel.AutoSize = True
        AdvancedDetailsTitleLabel.Location = New Point(34, 148)
        AdvancedDetailsTitleLabel.Name = "AdvancedDetailsTitleLabel"
        AdvancedDetailsTitleLabel.Size = New Size(99, 15)
        AdvancedDetailsTitleLabel.TabIndex = 15
        AdvancedDetailsTitleLabel.Text = "Advanced Details"
        '
        ' AdvancedDetailsStatusLabel
        '
        AdvancedDetailsStatusLabel.AutoSize = True
        AdvancedDetailsStatusLabel.Location = New Point(211, 148)
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
        IngredientsDataGrid.Columns.AddRange(New DataGridViewColumn() {IngredientNameColumn, IngredientAmountColumn})
        IngredientsDataGrid.Location = New Point(460, 45)
        IngredientsDataGrid.Name = "IngredientsDataGrid"
        IngredientsDataGrid.ReadOnly = True
        IngredientsDataGrid.Size = New Size(410, 240)
        IngredientsDataGrid.TabIndex = 12
        '
        ' IngredientNameColumn
        '
        IngredientNameColumn.FillWeight = 70.0!
        IngredientNameColumn.HeaderText = "Ingredient"
        IngredientNameColumn.Name = "IngredientNameColumn"
        IngredientNameColumn.ReadOnly = True
        '
        ' IngredientAmountColumn
        '
        IngredientAmountColumn.FillWeight = 30.0!
        IngredientAmountColumn.HeaderText = "Amount"
        IngredientAmountColumn.Name = "IngredientAmountColumn"
        IngredientAmountColumn.ReadOnly = True
        '
        ' PreparationMethodLabel
        '
        PreparationMethodLabel.AutoSize = True
        PreparationMethodLabel.Location = New Point(460, 305)
        PreparationMethodLabel.Name = "PreparationMethodLabel"
        PreparationMethodLabel.Size = New Size(118, 15)
        PreparationMethodLabel.TabIndex = 13
        PreparationMethodLabel.Text = "Preparation Method"
        '
        ' PreparationMethodTextBox
        '
        PreparationMethodTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        PreparationMethodTextBox.Location = New Point(460, 328)
        PreparationMethodTextBox.Multiline = True
        PreparationMethodTextBox.Name = "PreparationMethodTextBox"
        PreparationMethodTextBox.ReadOnly = True
        PreparationMethodTextBox.ScrollBars = ScrollBars.Vertical
        PreparationMethodTextBox.Size = New Size(410, 278)
        PreparationMethodTextBox.TabIndex = 14
        '
        ' RecipeView
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(900, 660)
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
    Friend WithEvents MealTypeTitleLabel As Label
    Friend WithEvents MealTypesLabel As Label
    Friend WithEvents AdvancedDetailsTitleLabel As Label
    Friend WithEvents AdvancedDetailsStatusLabel As Label
    Friend WithEvents IngredientsLabel As Label
    Friend WithEvents IngredientsDataGrid As DataGridView
    Friend WithEvents IngredientNameColumn As DataGridViewTextBoxColumn
    Friend WithEvents IngredientAmountColumn As DataGridViewTextBoxColumn
    Friend WithEvents PreparationMethodLabel As Label
    Friend WithEvents PreparationMethodTextBox As TextBox
End Class
