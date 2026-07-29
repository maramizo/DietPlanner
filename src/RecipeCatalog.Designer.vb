<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RecipeCatalog
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
        CatalogDataGrid = New DataGridView()
        NameColumn = New DataGridViewTextBoxColumn()
        CaloriesColumn = New DataGridViewTextBoxColumn()
        BreakfastColumn = New DataGridViewCheckBoxColumn()
        BrunchColumn = New DataGridViewCheckBoxColumn()
        LunchColumn = New DataGridViewCheckBoxColumn()
        DinnerColumn = New DataGridViewCheckBoxColumn()
        SnackColumn = New DataGridViewCheckBoxColumn()
        StatusColumn = New DataGridViewTextBoxColumn()
        SaveButton = New Button()
        ViewDetailsButton = New Button()
        CancelButton = New Button()
        CType(CatalogDataGrid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' CatalogDataGrid
        '
        CatalogDataGrid.AllowUserToAddRows = False
        CatalogDataGrid.AllowUserToDeleteRows = False
        CatalogDataGrid.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        CatalogDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        CatalogDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        CatalogDataGrid.Columns.AddRange(New DataGridViewColumn() {
            NameColumn,
            CaloriesColumn,
            BreakfastColumn,
            BrunchColumn,
            LunchColumn,
            DinnerColumn,
            SnackColumn,
            StatusColumn
        })
        CatalogDataGrid.Location = New Point(20, 20)
        CatalogDataGrid.MultiSelect = False
        CatalogDataGrid.Name = "CatalogDataGrid"
        CatalogDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        CatalogDataGrid.Size = New Size(940, 490)
        CatalogDataGrid.TabIndex = 0
        '
        ' NameColumn
        '
        NameColumn.FillWeight = 30.0!
        NameColumn.HeaderText = "Recipe"
        NameColumn.Name = "NameColumn"
        NameColumn.ReadOnly = True
        '
        ' CaloriesColumn
        '
        CaloriesColumn.FillWeight = 10.0!
        CaloriesColumn.HeaderText = "Calories"
        CaloriesColumn.Name = "CaloriesColumn"
        CaloriesColumn.ReadOnly = True
        '
        ' BreakfastColumn
        '
        BreakfastColumn.FillWeight = 10.0!
        BreakfastColumn.HeaderText = "Breakfast"
        BreakfastColumn.Name = "BreakfastColumn"
        '
        ' BrunchColumn
        '
        BrunchColumn.FillWeight = 9.0!
        BrunchColumn.HeaderText = "Brunch"
        BrunchColumn.Name = "BrunchColumn"
        '
        ' LunchColumn
        '
        LunchColumn.FillWeight = 9.0!
        LunchColumn.HeaderText = "Lunch"
        LunchColumn.Name = "LunchColumn"
        '
        ' DinnerColumn
        '
        DinnerColumn.FillWeight = 9.0!
        DinnerColumn.HeaderText = "Dinner"
        DinnerColumn.Name = "DinnerColumn"
        '
        ' SnackColumn
        '
        SnackColumn.FillWeight = 9.0!
        SnackColumn.HeaderText = "Snack"
        SnackColumn.Name = "SnackColumn"
        '
        ' StatusColumn
        '
        StatusColumn.FillWeight = 14.0!
        StatusColumn.HeaderText = "Details"
        StatusColumn.Name = "StatusColumn"
        StatusColumn.ReadOnly = True
        '
        ' SaveButton
        '
        SaveButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        SaveButton.Location = New Point(20, 530)
        SaveButton.Name = "SaveButton"
        SaveButton.Size = New Size(125, 28)
        SaveButton.TabIndex = 1
        SaveButton.Text = "Save Changes"
        SaveButton.UseVisualStyleBackColor = True
        '
        ' ViewDetailsButton
        '
        ViewDetailsButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ViewDetailsButton.Location = New Point(155, 530)
        ViewDetailsButton.Name = "ViewDetailsButton"
        ViewDetailsButton.Size = New Size(125, 28)
        ViewDetailsButton.TabIndex = 2
        ViewDetailsButton.Text = "View Details"
        ViewDetailsButton.UseVisualStyleBackColor = True
        '
        ' CancelButton
        '
        CancelButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        CancelButton.Location = New Point(835, 530)
        CancelButton.Name = "CancelButton"
        CancelButton.Size = New Size(125, 28)
        CancelButton.TabIndex = 3
        CancelButton.Text = "Cancel"
        CancelButton.UseVisualStyleBackColor = True
        '
        ' RecipeCatalog
        '
        AutoScaleDimensions = New SizeF(7.0!, 15.0!)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(980, 578)
        Controls.Add(CancelButton)
        Controls.Add(ViewDetailsButton)
        Controls.Add(SaveButton)
        Controls.Add(CatalogDataGrid)
        MinimumSize = New Size(800, 480)
        Name = "RecipeCatalog"
        StartPosition = FormStartPosition.CenterParent
        Text = "All Recipes"
        CType(CatalogDataGrid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents CatalogDataGrid As DataGridView
    Friend WithEvents NameColumn As DataGridViewTextBoxColumn
    Friend WithEvents CaloriesColumn As DataGridViewTextBoxColumn
    Friend WithEvents BreakfastColumn As DataGridViewCheckBoxColumn
    Friend WithEvents BrunchColumn As DataGridViewCheckBoxColumn
    Friend WithEvents LunchColumn As DataGridViewCheckBoxColumn
    Friend WithEvents DinnerColumn As DataGridViewCheckBoxColumn
    Friend WithEvents SnackColumn As DataGridViewCheckBoxColumn
    Friend WithEvents StatusColumn As DataGridViewTextBoxColumn
    Friend WithEvents SaveButton As Button
    Friend WithEvents ViewDetailsButton As Button
    Friend Shadows WithEvents CancelButton As Button
End Class
