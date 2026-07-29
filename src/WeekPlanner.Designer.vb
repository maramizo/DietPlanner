<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WeekPlanner
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        GenerationModeGroupBox = New GroupBox()
        SelectedOnlyRadioButton = New RadioButton()
        FullCatalogRadioButton = New RadioButton()
        GenerationModeHelpLabel = New Label()
        PlannedMealsGroupBox = New GroupBox()
        PlannedMealTypesCheckedListBox = New CheckedListBox()
        SelectedRecipesLabel = New Label()
        SelectedRecipesCheckedListBox = New CheckedListBox()
        SelectAllButton = New Button()
        ClearSelectionButton = New Button()
        GenerateButton = New Button()
        StatusLabel = New Label()
        PlanLabel = New Label()
        PlanDataGrid = New DataGridView()
        DayColumn = New DataGridViewTextBoxColumn()
        BreakfastColumn = New DataGridViewTextBoxColumn()
        BrunchColumn = New DataGridViewTextBoxColumn()
        LunchColumn = New DataGridViewTextBoxColumn()
        DinnerColumn = New DataGridViewTextBoxColumn()
        SnackColumn = New DataGridViewTextBoxColumn()
        CaloriesColumn = New DataGridViewTextBoxColumn()
        CoverageColumn = New DataGridViewTextBoxColumn()
        BalanceLabel = New Label()
        SummaryLabel = New Label()
        SummaryDataGrid = New DataGridView()
        NutrientColumn = New DataGridViewTextBoxColumn()
        WeeklyTotalColumn = New DataGridViewTextBoxColumn()
        WeeklyTargetColumn = New DataGridViewTextBoxColumn()
        PercentColumn = New DataGridViewTextBoxColumn()
        CloseButton = New Button()
        GenerationModeGroupBox.SuspendLayout()
        PlannedMealsGroupBox.SuspendLayout()
        CType(PlanDataGrid, ComponentModel.ISupportInitialize).BeginInit()
        CType(SummaryDataGrid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' GenerationModeGroupBox
        '
        GenerationModeGroupBox.Controls.Add(GenerationModeHelpLabel)
        GenerationModeGroupBox.Controls.Add(FullCatalogRadioButton)
        GenerationModeGroupBox.Controls.Add(SelectedOnlyRadioButton)
        GenerationModeGroupBox.Location = New Point(20, 14)
        GenerationModeGroupBox.Name = "GenerationModeGroupBox"
        GenerationModeGroupBox.Size = New Size(290, 120)
        GenerationModeGroupBox.TabIndex = 0
        GenerationModeGroupBox.TabStop = False
        GenerationModeGroupBox.Text = "Generation mode"
        '
        ' SelectedOnlyRadioButton
        '
        SelectedOnlyRadioButton.AutoSize = True
        SelectedOnlyRadioButton.Checked = True
        SelectedOnlyRadioButton.Location = New Point(12, 23)
        SelectedOnlyRadioButton.Name = "SelectedOnlyRadioButton"
        SelectedOnlyRadioButton.Size = New Size(156, 19)
        SelectedOnlyRadioButton.TabIndex = 0
        SelectedOnlyRadioButton.TabStop = True
        SelectedOnlyRadioButton.Text = "Only selected recipes"
        SelectedOnlyRadioButton.UseVisualStyleBackColor = True
        '
        ' FullCatalogRadioButton
        '
        FullCatalogRadioButton.AutoSize = True
        FullCatalogRadioButton.Location = New Point(12, 49)
        FullCatalogRadioButton.Name = "FullCatalogRadioButton"
        FullCatalogRadioButton.Size = New Size(216, 19)
        FullCatalogRadioButton.TabIndex = 1
        FullCatalogRadioButton.Text = "Generate freely from all recipes"
        FullCatalogRadioButton.UseVisualStyleBackColor = True
        '
        ' GenerationModeHelpLabel
        '
        GenerationModeHelpLabel.Location = New Point(28, 74)
        GenerationModeHelpLabel.Name = "GenerationModeHelpLabel"
        GenerationModeHelpLabel.Size = New Size(248, 38)
        GenerationModeHelpLabel.TabIndex = 2
        GenerationModeHelpLabel.Text = "The plan uses only checked recipes; each one appears at least once."
        '
        ' PlannedMealsGroupBox
        '
        PlannedMealsGroupBox.Controls.Add(PlannedMealTypesCheckedListBox)
        PlannedMealsGroupBox.Location = New Point(20, 145)
        PlannedMealsGroupBox.Name = "PlannedMealsGroupBox"
        PlannedMealsGroupBox.Size = New Size(290, 153)
        PlannedMealsGroupBox.TabIndex = 1
        PlannedMealsGroupBox.TabStop = False
        PlannedMealsGroupBox.Text = "Meals to plan"
        '
        ' PlannedMealTypesCheckedListBox
        '
        PlannedMealTypesCheckedListBox.CheckOnClick = True
        PlannedMealTypesCheckedListBox.FormattingEnabled = True
        PlannedMealTypesCheckedListBox.Items.AddRange(New Object() {
            "Breakfast",
            "Brunch",
            "Lunch",
            "Dinner",
            "Snack"
        })
        PlannedMealTypesCheckedListBox.Location = New Point(12, 22)
        PlannedMealTypesCheckedListBox.Name = "PlannedMealTypesCheckedListBox"
        PlannedMealTypesCheckedListBox.Size = New Size(266, 118)
        PlannedMealTypesCheckedListBox.TabIndex = 0
        '
        ' SelectedRecipesLabel
        '
        SelectedRecipesLabel.AutoSize = True
        SelectedRecipesLabel.Location = New Point(20, 310)
        SelectedRecipesLabel.Name = "SelectedRecipesLabel"
        SelectedRecipesLabel.Size = New Size(238, 15)
        SelectedRecipesLabel.TabIndex = 2
        SelectedRecipesLabel.Text = "Recipes to use (each appears at least once)"
        '
        ' SelectedRecipesCheckedListBox
        '
        SelectedRecipesCheckedListBox.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        SelectedRecipesCheckedListBox.CheckOnClick = True
        SelectedRecipesCheckedListBox.FormattingEnabled = True
        SelectedRecipesCheckedListBox.HorizontalScrollbar = True
        SelectedRecipesCheckedListBox.Location = New Point(20, 335)
        SelectedRecipesCheckedListBox.Name = "SelectedRecipesCheckedListBox"
        SelectedRecipesCheckedListBox.Size = New Size(290, 253)
        SelectedRecipesCheckedListBox.TabIndex = 3
        '
        ' SelectAllButton
        '
        SelectAllButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        SelectAllButton.Location = New Point(20, 598)
        SelectAllButton.Name = "SelectAllButton"
        SelectAllButton.Size = New Size(135, 26)
        SelectAllButton.TabIndex = 4
        SelectAllButton.Text = "Select All"
        SelectAllButton.UseVisualStyleBackColor = True
        '
        ' ClearSelectionButton
        '
        ClearSelectionButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ClearSelectionButton.Location = New Point(175, 598)
        ClearSelectionButton.Name = "ClearSelectionButton"
        ClearSelectionButton.Size = New Size(135, 26)
        ClearSelectionButton.TabIndex = 5
        ClearSelectionButton.Text = "Clear Selection"
        ClearSelectionButton.UseVisualStyleBackColor = True
        '
        ' GenerateButton
        '
        GenerateButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        GenerateButton.Location = New Point(20, 634)
        GenerateButton.Name = "GenerateButton"
        GenerateButton.Size = New Size(290, 30)
        GenerateButton.TabIndex = 6
        GenerateButton.Text = "Generate / Shuffle and Save Week"
        GenerateButton.UseVisualStyleBackColor = True
        '
        ' StatusLabel
        '
        StatusLabel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        StatusLabel.Location = New Point(20, 674)
        StatusLabel.Name = "StatusLabel"
        StatusLabel.Size = New Size(290, 34)
        StatusLabel.TabIndex = 7
        StatusLabel.Text = "Select recipes, then generate a balanced seven-day plan."
        '
        ' PlanLabel
        '
        PlanLabel.AutoSize = True
        PlanLabel.Location = New Point(335, 20)
        PlanLabel.Name = "PlanLabel"
        PlanLabel.Size = New Size(77, 15)
        PlanLabel.TabIndex = 8
        PlanLabel.Text = "Seven-day plan"
        '
        ' PlanDataGrid
        '
        PlanDataGrid.AllowUserToAddRows = False
        PlanDataGrid.AllowUserToDeleteRows = False
        PlanDataGrid.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PlanDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        PlanDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        PlanDataGrid.Columns.AddRange(New DataGridViewColumn() {
            DayColumn,
            BreakfastColumn,
            BrunchColumn,
            LunchColumn,
            DinnerColumn,
            SnackColumn,
            CaloriesColumn,
            CoverageColumn
        })
        PlanDataGrid.Location = New Point(335, 45)
        PlanDataGrid.MultiSelect = False
        PlanDataGrid.Name = "PlanDataGrid"
        PlanDataGrid.ReadOnly = True
        PlanDataGrid.RowHeadersVisible = False
        PlanDataGrid.SelectionMode = DataGridViewSelectionMode.CellSelect
        PlanDataGrid.Size = New Size(925, 365)
        PlanDataGrid.TabIndex = 9
        PlanDataGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
        '
        ' DayColumn
        '
        DayColumn.FillWeight = 10.0!
        DayColumn.HeaderText = "Day"
        DayColumn.Name = "DayColumn"
        DayColumn.ReadOnly = True
        '
        ' BreakfastColumn
        '
        BreakfastColumn.FillWeight = 16.0!
        BreakfastColumn.HeaderText = "Breakfast"
        BreakfastColumn.Name = "BreakfastColumn"
        BreakfastColumn.ReadOnly = True
        '
        ' BrunchColumn
        '
        BrunchColumn.FillWeight = 16.0!
        BrunchColumn.HeaderText = "Brunch"
        BrunchColumn.Name = "BrunchColumn"
        BrunchColumn.ReadOnly = True
        '
        ' LunchColumn
        '
        LunchColumn.FillWeight = 16.0!
        LunchColumn.HeaderText = "Lunch"
        LunchColumn.Name = "LunchColumn"
        LunchColumn.ReadOnly = True
        '
        ' DinnerColumn
        '
        DinnerColumn.FillWeight = 16.0!
        DinnerColumn.HeaderText = "Dinner"
        DinnerColumn.Name = "DinnerColumn"
        DinnerColumn.ReadOnly = True
        '
        ' SnackColumn
        '
        SnackColumn.FillWeight = 16.0!
        SnackColumn.HeaderText = "Snack"
        SnackColumn.Name = "SnackColumn"
        SnackColumn.ReadOnly = True
        '
        ' CaloriesColumn
        '
        CaloriesColumn.FillWeight = 8.0!
        CaloriesColumn.HeaderText = "Calories"
        CaloriesColumn.Name = "CaloriesColumn"
        CaloriesColumn.ReadOnly = True
        '
        ' CoverageColumn
        '
        CoverageColumn.FillWeight = 10.0!
        CoverageColumn.HeaderText = "Avg. Target"
        CoverageColumn.Name = "CoverageColumn"
        CoverageColumn.ReadOnly = True
        '
        ' BalanceLabel
        '
        BalanceLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        BalanceLabel.AutoEllipsis = True
        BalanceLabel.Location = New Point(335, 420)
        BalanceLabel.Name = "BalanceLabel"
        BalanceLabel.Size = New Size(925, 34)
        BalanceLabel.TabIndex = 10
        BalanceLabel.Text = "Generate a plan to see its day-to-day balance."
        '
        ' SummaryLabel
        '
        SummaryLabel.AutoSize = True
        SummaryLabel.Location = New Point(335, 460)
        SummaryLabel.Name = "SummaryLabel"
        SummaryLabel.Size = New Size(198, 15)
        SummaryLabel.TabIndex = 11
        SummaryLabel.Text = "Weekly recommended-intake summary"
        '
        ' SummaryDataGrid
        '
        SummaryDataGrid.AllowUserToAddRows = False
        SummaryDataGrid.AllowUserToDeleteRows = False
        SummaryDataGrid.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        SummaryDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        SummaryDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        SummaryDataGrid.Columns.AddRange(New DataGridViewColumn() {
            NutrientColumn,
            WeeklyTotalColumn,
            WeeklyTargetColumn,
            PercentColumn
        })
        SummaryDataGrid.Location = New Point(335, 485)
        SummaryDataGrid.Name = "SummaryDataGrid"
        SummaryDataGrid.ReadOnly = True
        SummaryDataGrid.RowHeadersVisible = False
        SummaryDataGrid.Size = New Size(925, 179)
        SummaryDataGrid.TabIndex = 12
        '
        ' NutrientColumn
        '
        NutrientColumn.HeaderText = "Nutrient"
        NutrientColumn.Name = "NutrientColumn"
        NutrientColumn.ReadOnly = True
        '
        ' WeeklyTotalColumn
        '
        WeeklyTotalColumn.HeaderText = "Planned Week"
        WeeklyTotalColumn.Name = "WeeklyTotalColumn"
        WeeklyTotalColumn.ReadOnly = True
        '
        ' WeeklyTargetColumn
        '
        WeeklyTargetColumn.HeaderText = "7-Day Target"
        WeeklyTargetColumn.Name = "WeeklyTargetColumn"
        WeeklyTargetColumn.ReadOnly = True
        '
        ' PercentColumn
        '
        PercentColumn.HeaderText = "% of Target"
        PercentColumn.Name = "PercentColumn"
        PercentColumn.ReadOnly = True
        '
        ' CloseButton
        '
        CloseButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        CloseButton.Location = New Point(1135, 680)
        CloseButton.Name = "CloseButton"
        CloseButton.Size = New Size(125, 28)
        CloseButton.TabIndex = 13
        CloseButton.Text = "Close"
        CloseButton.UseVisualStyleBackColor = True
        '
        ' WeekPlanner
        '
        AutoScaleDimensions = New SizeF(7.0!, 15.0!)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1280, 728)
        Controls.Add(PlannedMealsGroupBox)
        Controls.Add(GenerationModeGroupBox)
        Controls.Add(CloseButton)
        Controls.Add(SummaryDataGrid)
        Controls.Add(SummaryLabel)
        Controls.Add(BalanceLabel)
        Controls.Add(PlanDataGrid)
        Controls.Add(PlanLabel)
        Controls.Add(StatusLabel)
        Controls.Add(GenerateButton)
        Controls.Add(ClearSelectionButton)
        Controls.Add(SelectAllButton)
        Controls.Add(SelectedRecipesCheckedListBox)
        Controls.Add(SelectedRecipesLabel)
        MinimumSize = New Size(1050, 650)
        Name = "WeekPlanner"
        StartPosition = FormStartPosition.CenterParent
        Text = "Plan My Week"
        GenerationModeGroupBox.ResumeLayout(False)
        GenerationModeGroupBox.PerformLayout()
        PlannedMealsGroupBox.ResumeLayout(False)
        CType(PlanDataGrid, ComponentModel.ISupportInitialize).EndInit()
        CType(SummaryDataGrid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents GenerationModeGroupBox As GroupBox
    Friend WithEvents SelectedOnlyRadioButton As RadioButton
    Friend WithEvents FullCatalogRadioButton As RadioButton
    Friend WithEvents GenerationModeHelpLabel As Label
    Friend WithEvents PlannedMealsGroupBox As GroupBox
    Friend WithEvents PlannedMealTypesCheckedListBox As CheckedListBox
    Friend WithEvents SelectedRecipesLabel As Label
    Friend WithEvents SelectedRecipesCheckedListBox As CheckedListBox
    Friend WithEvents SelectAllButton As Button
    Friend WithEvents ClearSelectionButton As Button
    Friend WithEvents GenerateButton As Button
    Friend WithEvents StatusLabel As Label
    Friend WithEvents PlanLabel As Label
    Friend WithEvents PlanDataGrid As DataGridView
    Friend WithEvents DayColumn As DataGridViewTextBoxColumn
    Friend WithEvents BreakfastColumn As DataGridViewTextBoxColumn
    Friend WithEvents BrunchColumn As DataGridViewTextBoxColumn
    Friend WithEvents LunchColumn As DataGridViewTextBoxColumn
    Friend WithEvents DinnerColumn As DataGridViewTextBoxColumn
    Friend WithEvents SnackColumn As DataGridViewTextBoxColumn
    Friend WithEvents CaloriesColumn As DataGridViewTextBoxColumn
    Friend WithEvents CoverageColumn As DataGridViewTextBoxColumn
    Friend WithEvents BalanceLabel As Label
    Friend WithEvents SummaryLabel As Label
    Friend WithEvents SummaryDataGrid As DataGridView
    Friend WithEvents NutrientColumn As DataGridViewTextBoxColumn
    Friend WithEvents WeeklyTotalColumn As DataGridViewTextBoxColumn
    Friend WithEvents WeeklyTargetColumn As DataGridViewTextBoxColumn
    Friend WithEvents PercentColumn As DataGridViewTextBoxColumn
    Friend WithEvents CloseButton As Button
End Class
