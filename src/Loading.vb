Public Class Loading
    Private ReadOnly _minimumContentWidth As Integer
    Private ReadOnly _minimumLabelHeight As Integer
    Private ReadOnly _labelTop As Integer
    Private ReadOnly _progressGap As Integer
    Private ReadOnly _bottomMargin As Integer

    Public Sub New(loadingText As String)
        InitializeComponent()
        ApplyAppIcon(Me)
        _minimumContentWidth = Math.Max(Label1.Width, ProgressBar1.Width)
        _minimumLabelHeight = Label1.Height
        _labelTop = Label1.Top
        _progressGap = ProgressBar1.Top - Label1.Bottom
        _bottomMargin = ClientSize.Height - ProgressBar1.Bottom
        SetMessageAndResize(loadingText)
        With ProgressBar1
            .Visible = True
            .MarqueeAnimationSpeed = 30
            .Style = ProgressBarStyle.Marquee
        End With
    End Sub

    Public Sub UpdateMessage(message As String)
        SetMessageAndResize(message)
    End Sub

    Private Sub SetMessageAndResize(message As String)
        Label1.Text = If(message, String.Empty)

        Dim horizontalMargin = Math.Max(Label1.Left, ProgressBar1.Left)
        Dim singleLineText = TextRenderer.MeasureText(
            Label1.Text,
            Label1.Font,
            New Size(Integer.MaxValue, Integer.MaxValue),
            TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
        )
        Dim textPadding = Math.Max(
            24,
            TextRenderer.MeasureText(
                "MMMM",
                Label1.Font,
                New Size(Integer.MaxValue, Integer.MaxValue),
                TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
            ).Width
        )
        Dim desiredContentWidth = Math.Max(
            _minimumContentWidth,
            singleLineText.Width + textPadding
        )

        Dim workingArea = Screen.FromControl(Me).WorkingArea
        Dim nonClientWidth = Width - ClientSize.Width
        Dim maximumClientWidth = Math.Max(
            _minimumContentWidth + (horizontalMargin * 2),
            workingArea.Width - nonClientWidth - 40
        )
        Dim maximumContentWidth = maximumClientWidth - (horizontalMargin * 2)
        Dim contentWidth = Math.Min(desiredContentWidth, maximumContentWidth)
        Dim labelHeight = _minimumLabelHeight
        Dim wraps = desiredContentWidth > maximumContentWidth
        If wraps Then
            Dim wrappedText = TextRenderer.MeasureText(
                Label1.Text,
                Label1.Font,
                New Size(contentWidth, Integer.MaxValue),
                TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix
            )
            labelHeight = Math.Max(
                _minimumLabelHeight,
                wrappedText.Height + 8
            )
        End If
        Label1.TextAlign = If(
            wraps,
            ContentAlignment.TopCenter,
            ContentAlignment.MiddleCenter
        )
        Label1.Padding = If(
            wraps,
            New Padding(0, 4, 0, 0),
            Padding.Empty
        )
        Dim progressTop = _labelTop + labelHeight + _progressGap
        Dim clientHeight = progressTop + ProgressBar1.Height + _bottomMargin

        ClientSize = New Size(
            contentWidth + (horizontalMargin * 2),
            clientHeight
        )
        Label1.SetBounds(
            horizontalMargin,
            _labelTop,
            contentWidth,
            labelHeight
        )
        ProgressBar1.SetBounds(
            horizontalMargin,
            progressTop,
            contentWidth,
            ProgressBar1.Height
        )

        If Visible Then CenterToParent()
    End Sub
End Class
