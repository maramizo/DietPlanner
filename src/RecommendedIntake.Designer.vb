<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RecommendedIntake
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
        RecommendedGridView = New DataGridView()
        Nutrition = New DataGridViewTextBoxColumn()
        Amount = New DataGridViewTextBoxColumn()
        Save = New Button()
        Cancel = New Button()
        CType(RecommendedGridView, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' RecommendedGridView
        ' 
        RecommendedGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        RecommendedGridView.Columns.AddRange(New DataGridViewColumn() {Nutrition, Amount})
        RecommendedGridView.Location = New Point(26, 34)
        RecommendedGridView.Name = "RecommendedGridView"
        RecommendedGridView.Size = New Size(243, 267)
        RecommendedGridView.TabIndex = 0
        ' 
        ' Nutrition
        ' 
        Nutrition.HeaderText = "Nutrition"
        Nutrition.Name = "Nutrition"
        ' 
        ' Amount
        ' 
        Amount.HeaderText = "Amount"
        Amount.Name = "Amount"
        ' 
        ' Save
        ' 
        Save.Location = New Point(26, 341)
        Save.Name = "Save"
        Save.Size = New Size(112, 23)
        Save.TabIndex = 1
        Save.Text = "Save"
        Save.UseVisualStyleBackColor = True
        ' 
        ' Cancel
        ' 
        Cancel.Location = New Point(162, 341)
        Cancel.Name = "Cancel"
        Cancel.Size = New Size(107, 23)
        Cancel.TabIndex = 2
        Cancel.Text = "Cancel"
        Cancel.UseVisualStyleBackColor = True
        ' 
        ' RecommendedIntake
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(304, 387)
        Controls.Add(Cancel)
        Controls.Add(Save)
        Controls.Add(RecommendedGridView)
        Name = "RecommendedIntake"
        Text = "RecommendedIntake"
        CType(RecommendedGridView, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents RecommendedGridView As DataGridView
    Friend WithEvents Nutrition As DataGridViewTextBoxColumn
    Friend WithEvents Amount As DataGridViewTextBoxColumn
    Friend WithEvents Save As Button
    Friend WithEvents Cancel As Button
End Class
