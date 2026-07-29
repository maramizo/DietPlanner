Public Class Loading
    Public Sub New(loadingText As String)
        InitializeComponent()
        ApplyAppIcon(Me)
        Label1.Text = loadingText
        With ProgressBar1
            .Visible = True
            .MarqueeAnimationSpeed = 30
            .Style = ProgressBarStyle.Marquee
        End With
    End Sub

    Public Sub UpdateMessage(message As String)
        Label1.Text = message
    End Sub
End Class
