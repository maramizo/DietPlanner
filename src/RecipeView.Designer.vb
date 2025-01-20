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
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' NotifyIcon1
        ' 
        NotifyIcon1.Text = "NotifyIcon1"
        NotifyIcon1.Visible = True
        ' 
        ' DataGridView1
        ' 
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Columns.AddRange(New DataGridViewColumn() {Nutrition, Value})
        DataGridView1.Location = New Point(34, 126)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(387, 445)
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
        ' RecipeView
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(460, 639)
        Controls.Add(TotalTime)
        Controls.Add(CookTime)
        Controls.Add(PrepTime)
        Controls.Add(Label5)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(DataGridView1)
        Name = "RecipeView"
        StartPosition = FormStartPosition.CenterScreen
        Text = "View Recipe"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
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
End Class
