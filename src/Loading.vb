Public Class Loading
    Public Sub New(loadingText As String)
        InitializeComponent()
        Label1.Text = loadingText
        With ProgressBar1
            .Visible = True
            .MarqueeAnimationSpeed = 30
            .Style = ProgressBarStyle.Marquee
        End With
    End Sub
End Class