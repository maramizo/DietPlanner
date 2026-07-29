Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Module AppAppearance
    Public Sub ApplyAppIcon(form As Form)
        Dim iconPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "DietPlanner.ico"
        )

        If File.Exists(iconPath) Then
            form.Icon = New Icon(iconPath)
        Else
            Using executableIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
                If executableIcon IsNot Nothing Then
                    form.Icon = DirectCast(executableIcon.Clone(), Icon)
                End If
            End Using
        End If

        ThemeManager.ApplyTheme(form)
    End Sub
End Module

Public NotInheritable Class AppTheme
    Public ReadOnly Property Key As String
    Public ReadOnly Property DisplayName As String
    Public ReadOnly Property Description As String
    Public ReadOnly Property WindowBackColor As Color
    Public ReadOnly Property SurfaceBackColor As Color
    Public ReadOnly Property InputBackColor As Color
    Public ReadOnly Property TextColor As Color
    Public ReadOnly Property MutedTextColor As Color
    Public ReadOnly Property AccentColor As Color
    Public ReadOnly Property AccentHoverColor As Color
    Public ReadOnly Property AccentTextColor As Color
    Public ReadOnly Property BorderColor As Color
    Public ReadOnly Property SelectionColor As Color
    Public ReadOnly Property SelectionTextColor As Color

    Public Sub New(
        key As String,
        displayName As String,
        description As String,
        windowBackColor As Color,
        surfaceBackColor As Color,
        inputBackColor As Color,
        textColor As Color,
        mutedTextColor As Color,
        accentColor As Color,
        accentHoverColor As Color,
        accentTextColor As Color,
        borderColor As Color,
        selectionColor As Color,
        selectionTextColor As Color
    )
        Me.Key = key
        Me.DisplayName = displayName
        Me.Description = description
        Me.WindowBackColor = windowBackColor
        Me.SurfaceBackColor = surfaceBackColor
        Me.InputBackColor = inputBackColor
        Me.TextColor = textColor
        Me.MutedTextColor = mutedTextColor
        Me.AccentColor = accentColor
        Me.AccentHoverColor = accentHoverColor
        Me.AccentTextColor = accentTextColor
        Me.BorderColor = borderColor
        Me.SelectionColor = selectionColor
        Me.SelectionTextColor = selectionTextColor
    End Sub

    Public Overrides Function ToString() As String
        Return DisplayName
    End Function
End Class

Public NotInheritable Class ThemeManager
    Public Const DefaultThemeKey As String = "fresh-sage"

    Private Shared ReadOnly Themes As IReadOnlyList(Of AppTheme) =
        CreateThemes()
    Private Shared _currentTheme As AppTheme

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property AvailableThemes As IReadOnlyList(Of AppTheme)
        Get
            Return Themes
        End Get
    End Property

    Public Shared ReadOnly Property CurrentTheme As AppTheme
        Get
            If _currentTheme Is Nothing Then
                Dim settings = AppSettingsRepository.Load()
                _currentTheme = FindTheme(settings.ThemeKey)
            End If
            Return _currentTheme
        End Get
    End Property

    Public Shared Function FindTheme(key As String) As AppTheme
        Dim theme = Themes.FirstOrDefault(
            Function(candidate) String.Equals(
                candidate.Key,
                key,
                StringComparison.OrdinalIgnoreCase
            )
        )
        If theme IsNot Nothing Then Return theme
        Return Themes.First(
            Function(candidate) String.Equals(
                candidate.Key,
                DefaultThemeKey,
                StringComparison.OrdinalIgnoreCase
            )
        )
    End Function

    Public Shared Sub SelectTheme(key As String, persist As Boolean)
        _currentTheme = FindTheme(key)
        If persist Then
            AppSettingsRepository.Save(
                New DietPlannerSettings With {
                    .ThemeKey = _currentTheme.Key
                }
            )
        End If
    End Sub

    Public Shared Sub ApplyToOpenForms()
        Dim openForms = Application.OpenForms.Cast(Of Form)().ToList()
        For Each form In openForms
            ApplyTheme(form)
            form.Invalidate(True)
        Next
    End Sub

    Public Shared Sub ApplyTheme(form As Form)
        If form Is Nothing Then Return
        Dim theme = CurrentTheme
        form.BackColor = theme.WindowBackColor
        form.ForeColor = theme.TextColor
        ApplyControlTheme(form, theme)
    End Sub

    Private Shared Sub ApplyControlTheme(control As Control, theme As AppTheme)
        If String.Equals(
            TryCast(control.Tag, String),
            "ThemeSwatch",
            StringComparison.Ordinal
        ) Then
            Return
        End If

        If TypeOf control Is DataGridView Then
            ApplyDataGridTheme(DirectCast(control, DataGridView), theme)
        ElseIf TypeOf control Is Button Then
            ApplyButtonTheme(DirectCast(control, Button), theme)
        ElseIf TypeOf control Is RadioButton OrElse TypeOf control Is CheckBox Then
            control.BackColor = Color.Transparent
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is TextBoxBase Then
            control.BackColor = theme.InputBackColor
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is ComboBox Then
            Dim comboBox = DirectCast(control, ComboBox)
            comboBox.BackColor = theme.InputBackColor
            comboBox.ForeColor = theme.TextColor
            comboBox.FlatStyle = FlatStyle.Flat
        ElseIf TypeOf control Is NumericUpDown Then
            control.BackColor = theme.InputBackColor
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is CheckedListBox OrElse TypeOf control Is ListBox Then
            control.BackColor = theme.InputBackColor
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is ListView Then
            control.BackColor = theme.InputBackColor
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is LinkLabel Then
            Dim link = DirectCast(control, LinkLabel)
            link.BackColor = Color.Transparent
            link.ForeColor = theme.AccentColor
            link.LinkColor = theme.AccentColor
            link.ActiveLinkColor = theme.AccentHoverColor
            link.VisitedLinkColor = theme.AccentColor
        ElseIf TypeOf control Is Label Then
            control.BackColor = Color.Transparent
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is GroupBox Then
            control.BackColor = theme.WindowBackColor
            control.ForeColor = theme.TextColor
        ElseIf TypeOf control Is TabPage OrElse
            TypeOf control Is Panel OrElse
            TypeOf control Is TableLayoutPanel OrElse
            TypeOf control Is FlowLayoutPanel Then
            control.BackColor = theme.SurfaceBackColor
            control.ForeColor = theme.TextColor
        Else
            control.ForeColor = theme.TextColor
        End If

        For Each child As Control In control.Controls
            ApplyControlTheme(child, theme)
        Next
    End Sub

    Private Shared Sub ApplyButtonTheme(button As Button, theme As AppTheme)
        Dim primary = IsPrimaryButton(button)
        button.UseVisualStyleBackColor = False
        button.FlatStyle = FlatStyle.Flat
        button.FlatAppearance.BorderSize = 1
        button.FlatAppearance.BorderColor = If(
            primary,
            theme.AccentColor,
            theme.BorderColor
        )
        button.BackColor = If(
            primary,
            theme.AccentColor,
            theme.SurfaceBackColor
        )
        button.ForeColor = If(
            primary,
            theme.AccentTextColor,
            theme.TextColor
        )
        button.FlatAppearance.MouseOverBackColor = If(
            primary,
            theme.AccentHoverColor,
            Blend(theme.SurfaceBackColor, theme.AccentColor, 0.12)
        )
        button.FlatAppearance.MouseDownBackColor = Blend(
            theme.AccentColor,
            Color.Black,
            0.12
        )
    End Sub

    Private Shared Function IsPrimaryButton(button As Button) As Boolean
        Select Case button.Name
            Case "AddButton",
                 "ApplyThemeButton",
                 "GenerateButton",
                 "PlanWeekButton",
                 "Save",
                 "SaveButton",
                 "ScrapeButton"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Sub ApplyDataGridTheme(grid As DataGridView, theme As AppTheme)
        grid.BackgroundColor = theme.InputBackColor
        grid.BorderStyle = BorderStyle.FixedSingle
        grid.GridColor = theme.BorderColor
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = theme.AccentColor
        grid.ColumnHeadersDefaultCellStyle.ForeColor = theme.AccentTextColor
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = theme.AccentColor
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = theme.AccentTextColor
        grid.DefaultCellStyle.BackColor = theme.InputBackColor
        grid.DefaultCellStyle.ForeColor = theme.TextColor
        grid.DefaultCellStyle.SelectionBackColor = theme.SelectionColor
        grid.DefaultCellStyle.SelectionForeColor = theme.SelectionTextColor
        grid.AlternatingRowsDefaultCellStyle.BackColor = Blend(
            theme.InputBackColor,
            theme.AccentColor,
            0.045
        )
        grid.AlternatingRowsDefaultCellStyle.ForeColor = theme.TextColor
        grid.RowHeadersDefaultCellStyle.BackColor = theme.SurfaceBackColor
        grid.RowHeadersDefaultCellStyle.ForeColor = theme.TextColor
    End Sub

    Private Shared Function Blend(
        first As Color,
        second As Color,
        secondWeight As Double
    ) As Color
        Dim weight = Math.Max(0, Math.Min(1, secondWeight))
        Return Color.FromArgb(
            CInt(first.R * (1 - weight) + second.R * weight),
            CInt(first.G * (1 - weight) + second.G * weight),
            CInt(first.B * (1 - weight) + second.B * weight)
        )
    End Function

    Private Shared Function CreateThemes() As IReadOnlyList(Of AppTheme)
        Return New List(Of AppTheme) From {
            New AppTheme(
                "fresh-sage",
                "Fresh Sage",
                "Warm ivory surfaces, forest green, soft sage, and a small coral accent.",
                Color.FromArgb(247, 244, 236),
                Color.FromArgb(255, 253, 247),
                Color.FromArgb(255, 255, 252),
                Color.FromArgb(33, 54, 43),
                Color.FromArgb(101, 116, 106),
                Color.FromArgb(31, 104, 72),
                Color.FromArgb(24, 84, 58),
                Color.White,
                Color.FromArgb(202, 211, 201),
                Color.FromArgb(211, 233, 218),
                Color.FromArgb(25, 67, 47)
            ),
            New AppTheme(
                "coastal-blue",
                "Coastal Blue",
                "Airy blue-gray surfaces with deep navy and a clear turquoise accent.",
                Color.FromArgb(239, 246, 249),
                Color.FromArgb(249, 252, 253),
                Color.White,
                Color.FromArgb(29, 51, 67),
                Color.FromArgb(91, 112, 126),
                Color.FromArgb(19, 122, 135),
                Color.FromArgb(14, 96, 108),
                Color.White,
                Color.FromArgb(190, 211, 220),
                Color.FromArgb(202, 234, 238),
                Color.FromArgb(22, 69, 78)
            ),
            New AppTheme(
                "berry-bloom",
                "Berry Bloom",
                "A gentle blush canvas with plum text and a lively raspberry accent.",
                Color.FromArgb(250, 241, 245),
                Color.FromArgb(255, 250, 252),
                Color.FromArgb(255, 253, 254),
                Color.FromArgb(70, 42, 61),
                Color.FromArgb(122, 94, 113),
                Color.FromArgb(158, 52, 100),
                Color.FromArgb(126, 39, 79),
                Color.White,
                Color.FromArgb(225, 198, 211),
                Color.FromArgb(242, 211, 225),
                Color.FromArgb(91, 38, 64)
            ),
            New AppTheme(
                "midnight-kitchen",
                "Midnight Kitchen",
                "A comfortable charcoal theme with mint highlights and warm apricot contrast.",
                Color.FromArgb(28, 33, 32),
                Color.FromArgb(38, 45, 43),
                Color.FromArgb(47, 55, 53),
                Color.FromArgb(239, 244, 241),
                Color.FromArgb(169, 182, 176),
                Color.FromArgb(101, 191, 150),
                Color.FromArgb(124, 210, 171),
                Color.FromArgb(20, 39, 30),
                Color.FromArgb(76, 89, 84),
                Color.FromArgb(59, 105, 82),
                Color.White
            )
        }
    End Function
End Class
