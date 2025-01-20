<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Loading
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
        ProgressBar1 = New ProgressBar()
        Label1 = New Label()
        SuspendLayout()
        ' 
        ' ProgressBar1
        ' 
        ProgressBar1.Location = New Point(12, 82)
        ProgressBar1.Name = "ProgressBar1"
        ProgressBar1.Size = New Size(331, 23)
        ProgressBar1.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        Label1.Location = New Point(12, 37)
        Label1.Name = "Label1"
        Label1.Size = New Size(331, 23)
        Label1.TabIndex = 1
        Label1.Text = "Label1"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Loading
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(358, 120)
        Controls.Add(Label1)
        Controls.Add(ProgressBar1)
        Name = "Loading"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Loading"
        ResumeLayout(False)
    End Sub

    Friend WithEvents ProgressBar1 As ProgressBar
    Friend WithEvents Label1 As Label
End Class
