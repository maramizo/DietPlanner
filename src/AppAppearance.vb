Imports System.Drawing
Imports System.Drawing.Text
Imports System.IO
Imports System.Runtime.InteropServices
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
    Public Const DefaultFontFamilyName As String = "Segoe UI Variable Text"
    Public Const DefaultFontSize As Single = 10.0F
    Public Const MinimumFontSize As Single = 8.0F
    Public Const MaximumFontSize As Single = 12.0F

    Private Shared ReadOnly Themes As IReadOnlyList(Of AppTheme) =
        CreateThemes()
    Private Shared ReadOnly FontFamilyNames As IReadOnlyList(Of String) =
        LoadFontFamilyNames()
    Private Shared _currentTheme As AppTheme
    Private Shared _currentFontFamilyName As String
    Private Shared _currentFontSize As Single
    Private Shared _settingsLoaded As Boolean

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property AvailableThemes As IReadOnlyList(Of AppTheme)
        Get
            Return Themes
        End Get
    End Property

    Public Shared ReadOnly Property AvailableFontFamilies As IReadOnlyList(Of String)
        Get
            Return FontFamilyNames
        End Get
    End Property

    Public Shared ReadOnly Property CurrentTheme As AppTheme
        Get
            EnsureSettingsLoaded()
            Return _currentTheme
        End Get
    End Property

    Public Shared ReadOnly Property CurrentFontFamilyName As String
        Get
            EnsureSettingsLoaded()
            Return _currentFontFamilyName
        End Get
    End Property

    Public Shared ReadOnly Property CurrentFontSize As Single
        Get
            EnsureSettingsLoaded()
            Return _currentFontSize
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
        EnsureSettingsLoaded()
        _currentTheme = FindTheme(key)
        If persist Then SavePreferences()
    End Sub

    Public Shared Sub SelectFont(
        fontFamilyName As String,
        fontSize As Single,
        persist As Boolean
    )
        EnsureSettingsLoaded()
        _currentFontFamilyName = ResolveFontFamilyName(fontFamilyName)
        _currentFontSize = NormalizeFontSize(fontSize)
        If persist Then SavePreferences()
    End Sub

    Public Shared Sub SavePreferences()
        EnsureSettingsLoaded()
        AppSettingsRepository.Save(
            New DietPlannerSettings With {
                .ThemeKey = _currentTheme.Key,
                .FontFamilyName = _currentFontFamilyName,
                .FontSize = _currentFontSize
            }
        )
    End Sub

    Public Shared Function CreateApplicationFont(
        Optional style As FontStyle = FontStyle.Regular
    ) As Font
        EnsureSettingsLoaded()
        Try
            Return New Font(
                _currentFontFamilyName,
                _currentFontSize,
                style,
                GraphicsUnit.Point
            )
        Catch
            Return New Font(
                SystemFonts.MessageBoxFont.FontFamily,
                _currentFontSize,
                style,
                GraphicsUnit.Point
            )
        End Try
    End Function

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
        ApplyApplicationFont(form)
        form.BackColor = theme.WindowBackColor
        form.ForeColor = theme.TextColor
        ApplyControlTheme(form, theme)
        ApplyTitleBarTheme(form, theme)
    End Sub

    Private Shared Sub ApplyTitleBarTheme(form As Form, theme As AppTheme)
        RemoveHandler form.HandleCreated, AddressOf Form_HandleCreated
        AddHandler form.HandleCreated, AddressOf Form_HandleCreated
        If form.IsHandleCreated Then ApplyNativeTitleBar(form, theme)
    End Sub

    Private Shared Sub Form_HandleCreated(sender As Object, e As EventArgs)
        Dim form = TryCast(sender, Form)
        If form Is Nothing Then Return
        ApplyNativeTitleBar(form, CurrentTheme)
    End Sub

    Private Shared Sub ApplyNativeTitleBar(form As Form, theme As AppTheme)
        Try
            Dim darkMode = If(IsDarkColor(theme.WindowBackColor), 1, 0)
            If DwmSetWindowAttribute(
                form.Handle,
                20,
                darkMode,
                Marshal.SizeOf(Of Integer)()
            ) <> 0 Then
                DwmSetWindowAttribute(
                    form.Handle,
                    19,
                    darkMode,
                    Marshal.SizeOf(Of Integer)()
                )
            End If

            Dim captionBackColor = If(
                darkMode = 1,
                theme.SurfaceBackColor,
                theme.AccentColor
            )
            Dim captionTextColor = If(
                darkMode = 1,
                theme.TextColor,
                theme.AccentTextColor
            )
            Dim captionColorValue = ColorTranslator.ToWin32(captionBackColor)
            Dim textColorValue = ColorTranslator.ToWin32(captionTextColor)
            Dim borderColorValue = ColorTranslator.ToWin32(theme.BorderColor)

            DwmSetWindowAttribute(
                form.Handle,
                35,
                captionColorValue,
                Marshal.SizeOf(Of Integer)()
            )
            DwmSetWindowAttribute(
                form.Handle,
                36,
                textColorValue,
                Marshal.SizeOf(Of Integer)()
            )
            DwmSetWindowAttribute(
                form.Handle,
                34,
                borderColorValue,
                Marshal.SizeOf(Of Integer)()
            )
        Catch ex As DllNotFoundException
        Catch ex As EntryPointNotFoundException
        End Try
    End Sub

    Private Shared Function IsDarkColor(color As Color) As Boolean
        Dim luminance =
            color.R * 0.2126 +
            color.G * 0.7152 +
            color.B * 0.0722
        Return luminance < 128
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(
        windowHandle As IntPtr,
        attribute As Integer,
        ByRef attributeValue As Integer,
        attributeSize As Integer
    ) As Integer
    End Function

    Private Shared Sub ApplyApplicationFont(form As Form)
        Dim previousFont = form.Font
        Dim applicationFont = CreateApplicationFont(previousFont.Style)
        Dim changed =
            Not String.Equals(
                previousFont.FontFamily.Name,
                applicationFont.FontFamily.Name,
                StringComparison.OrdinalIgnoreCase
            ) OrElse Math.Abs(previousFont.SizeInPoints - applicationFont.SizeInPoints) > 0.05

        If Not changed Then
            applicationFont.Dispose()
            Return
        End If

        form.Font = applicationFont
        UpdateExplicitControlFonts(
            form,
            previousFont,
            applicationFont.FontFamily.Name,
            applicationFont.SizeInPoints
        )
    End Sub

    Private Shared Sub UpdateExplicitControlFonts(
        parent As Control,
        previousBaseFont As Font,
        newFamilyName As String,
        newBaseSize As Single
    )
        For Each child As Control In parent.Controls
            Dim childFont = child.Font
            Dim stillUsesPreviousBase =
                String.Equals(
                    childFont.FontFamily.Name,
                    previousBaseFont.FontFamily.Name,
                    StringComparison.OrdinalIgnoreCase
                ) AndAlso
                Math.Abs(
                    childFont.SizeInPoints - previousBaseFont.SizeInPoints
                ) <= 0.05
            Dim usesOldFamily = Not String.Equals(
                childFont.FontFamily.Name,
                newFamilyName,
                StringComparison.OrdinalIgnoreCase
            )

            If stillUsesPreviousBase OrElse usesOldFamily Then
                Try
                    child.Font = New Font(
                        newFamilyName,
                        newBaseSize,
                        childFont.Style,
                        GraphicsUnit.Point
                    )
                Catch
                    child.Font = New Font(
                        SystemFonts.MessageBoxFont.FontFamily,
                        newBaseSize,
                        childFont.Style,
                        GraphicsUnit.Point
                    )
                End Try
            End If

            UpdateExplicitControlFonts(
                child,
                previousBaseFont,
                newFamilyName,
                newBaseSize
            )
        Next
    End Sub

    Private Shared Sub EnsureSettingsLoaded()
        If _settingsLoaded Then Return

        Dim settings = AppSettingsRepository.Load()
        _currentTheme = FindTheme(settings.ThemeKey)
        _currentFontFamilyName = ResolveFontFamilyName(
            settings.FontFamilyName
        )
        _currentFontSize = NormalizeFontSize(settings.FontSize)
        _settingsLoaded = True
    End Sub

    Private Shared Function ResolveFontFamilyName(
        requestedName As String
    ) As String
        For Each candidate In {
            requestedName,
            DefaultFontFamilyName,
            "Segoe UI",
            SystemFonts.MessageBoxFont.FontFamily.Name
        }
            If String.IsNullOrWhiteSpace(candidate) Then Continue For
            Dim match = FontFamilyNames.FirstOrDefault(
                Function(fontName) String.Equals(
                    fontName,
                    candidate,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            If match IsNot Nothing Then Return match
        Next

        Return SystemFonts.MessageBoxFont.FontFamily.Name
    End Function

    Private Shared Function NormalizeFontSize(fontSize As Single) As Single
        If fontSize <= 0 OrElse
            Single.IsNaN(fontSize) OrElse
            Single.IsInfinity(fontSize) Then
            Return DefaultFontSize
        End If
        Return Math.Max(
            MinimumFontSize,
            Math.Min(MaximumFontSize, fontSize)
        )
    End Function

    Private Shared Function LoadFontFamilyNames() As IReadOnlyList(Of String)
        Dim names As New List(Of String)
        Try
            Using installedFonts As New InstalledFontCollection()
                For Each family In installedFonts.Families
                    If family.Name.StartsWith(
                        "@",
                        StringComparison.Ordinal
                    ) Then
                        Continue For
                    End If
                    If family.IsStyleAvailable(FontStyle.Regular) AndAlso
                        family.IsStyleAvailable(FontStyle.Bold) Then
                        Using sample As New Font(
                            family,
                            DefaultFontSize,
                            FontStyle.Regular,
                            GraphicsUnit.Point
                        )
                            If sample.GdiCharSet <> 2 Then
                                names.Add(family.Name)
                            End If
                        End Using
                    End If
                Next
            End Using
        Catch
        End Try

        If names.Count = 0 Then
            names.Add(SystemFonts.MessageBoxFont.FontFamily.Name)
        End If
        Return names.Distinct(
            StringComparer.CurrentCultureIgnoreCase
        ).OrderBy(
            Function(name) name,
            StringComparer.CurrentCultureIgnoreCase
        ).ToList()
    End Function

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
