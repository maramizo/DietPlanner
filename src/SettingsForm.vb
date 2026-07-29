Imports System.Drawing
Imports System.Windows.Forms

Public Class SettingsForm
    Inherits Form

    Private ReadOnly _originalThemeKey As String
    Private ReadOnly _themeButtons As New Dictionary(Of String, RadioButton)(
        StringComparer.OrdinalIgnoreCase
    )
    Private ReadOnly _descriptionLabel As New Label()
    Private ReadOnly _previewPanel As New Panel()
    Private ReadOnly _previewTitle As New Label()
    Private ReadOnly _previewSubtitle As New Label()
    Private ReadOnly _previewInput As New TextBox()
    Private ReadOnly _previewPrimaryButton As New Button()
    Private ReadOnly _previewSecondaryButton As New Button()
    Private ReadOnly _previewRows As New List(Of Panel)
    Private _selectedThemeKey As String
    Private _saved As Boolean

    Public Sub New()
        _originalThemeKey = ThemeManager.CurrentTheme.Key
        _selectedThemeKey = _originalThemeKey
        InitializeSettingsForm()
        ApplyAppIcon(Me)
        _themeButtons(_originalThemeKey).Checked = True
        PreviewTheme(ThemeManager.FindTheme(_originalThemeKey))
    End Sub

    Private Sub InitializeSettingsForm()
        Text = "DietPlanner Settings"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ClientSize = New Size(780, 490)

        Dim heading As New Label With {
            .AutoSize = True,
            .Font = New Font(Font, FontStyle.Bold),
            .Location = New Point(22, 20),
            .Text = "Choose how DietPlanner feels"
        }
        Dim subheading As New Label With {
            .AutoSize = True,
            .Location = New Point(22, 45),
            .Text = "Theme changes are previewed immediately across every open window."
        }

        Dim optionsGroup As New GroupBox With {
            .Location = New Point(20, 78),
            .Name = "ThemeOptionsGroup",
            .Size = New Size(345, 344),
            .Text = "Theme concepts"
        }

        Dim optionTop = 27
        For Each theme In ThemeManager.AvailableThemes
            AddThemeOption(optionsGroup, theme, optionTop)
            optionTop += 67
        Next

        _descriptionLabel.Location = New Point(15, 294)
        _descriptionLabel.Name = "ThemeDescriptionLabel"
        _descriptionLabel.Size = New Size(315, 40)
        optionsGroup.Controls.Add(_descriptionLabel)

        Dim previewGroup As New GroupBox With {
            .Location = New Point(385, 78),
            .Name = "ThemePreviewGroup",
            .Size = New Size(375, 344),
            .Text = "Live preview"
        }
        BuildPreview(previewGroup)

        Dim applyButton As New Button With {
            .Location = New Point(585, 444),
            .Name = "ApplyThemeButton",
            .Size = New Size(82, 28),
            .Text = "Save"
        }
        AddHandler applyButton.Click, AddressOf ApplyButton_Click

        Dim cancelButton As New Button With {
            .DialogResult = DialogResult.Cancel,
            .Location = New Point(678, 444),
            .Name = "CancelThemeButton",
            .Size = New Size(82, 28),
            .Text = "Cancel"
        }
        AddHandler cancelButton.Click, AddressOf CancelButton_Click

        AcceptButton = applyButton
        CancelButton = cancelButton
        Controls.AddRange({
            heading,
            subheading,
            optionsGroup,
            previewGroup,
            applyButton,
            cancelButton
        })
        AddHandler FormClosing, AddressOf SettingsForm_FormClosing
    End Sub

    Private Sub AddThemeOption(
        parent As Control,
        theme As AppTheme,
        top As Integer
    )
        Dim radio As New RadioButton With {
            .AutoSize = True,
            .Font = New Font(Font, FontStyle.Bold),
            .Location = New Point(15, top),
            .Name = "Theme_" & theme.Key.Replace("-", "_"),
            .Text = theme.DisplayName
        }
        Dim capturedTheme = theme
        AddHandler radio.CheckedChanged,
            Sub(sender, e)
                If radio.Checked Then SelectPreviewTheme(capturedTheme)
            End Sub
        _themeButtons(theme.Key) = radio
        parent.Controls.Add(radio)

        Dim swatchColors = {
            theme.WindowBackColor,
            theme.AccentColor,
            theme.SelectionColor
        }
        For index As Integer = 0 To swatchColors.Length - 1
            Dim swatch As New Panel With {
                .BackColor = swatchColors(index),
                .BorderStyle = BorderStyle.FixedSingle,
                .Location = New Point(205 + index * 38, top - 3),
                .Size = New Size(30, 22),
                .Tag = "ThemeSwatch"
            }
            parent.Controls.Add(swatch)
        Next
    End Sub

    Private Sub BuildPreview(parent As Control)
        _previewPanel.Location = New Point(17, 28)
        _previewPanel.Name = "PreviewCanvas"
        _previewPanel.Size = New Size(340, 296)
        _previewPanel.BorderStyle = BorderStyle.FixedSingle

        _previewTitle.AutoSize = True
        _previewTitle.Font = New Font(Font, FontStyle.Bold)
        _previewTitle.Location = New Point(18, 18)
        _previewTitle.Text = "Your week at a glance"

        _previewSubtitle.AutoSize = True
        _previewSubtitle.Location = New Point(18, 43)
        _previewSubtitle.Text = "A balanced plan built around your recipes"

        _previewInput.Location = New Point(18, 72)
        _previewInput.ReadOnly = True
        _previewInput.Size = New Size(302, 23)
        _previewInput.Text = "Weekly meal plan"

        Dim rowNames = {"Breakfast", "Lunch", "Dinner"}
        For index As Integer = 0 To rowNames.Length - 1
            Dim row As New Panel With {
                .BorderStyle = BorderStyle.FixedSingle,
                .Location = New Point(18, 108 + index * 38),
                .Size = New Size(302, 30)
            }
            Dim rowLabel As New Label With {
                .AutoSize = True,
                .Location = New Point(10, 7),
                .Text = rowNames(index)
            }
            row.Controls.Add(rowLabel)
            _previewRows.Add(row)
            _previewPanel.Controls.Add(row)
        Next

        _previewPrimaryButton.Location = New Point(18, 236)
        _previewPrimaryButton.Size = New Size(145, 34)
        _previewPrimaryButton.Text = "Generate / Shuffle"

        _previewSecondaryButton.Location = New Point(175, 236)
        _previewSecondaryButton.Size = New Size(145, 34)
        _previewSecondaryButton.Text = "View recipes"

        _previewPanel.Controls.AddRange({
            _previewTitle,
            _previewSubtitle,
            _previewInput,
            _previewPrimaryButton,
            _previewSecondaryButton
        })
        parent.Controls.Add(_previewPanel)
    End Sub

    Private Sub SelectPreviewTheme(theme As AppTheme)
        _selectedThemeKey = theme.Key
        ThemeManager.SelectTheme(theme.Key, False)
        ThemeManager.ApplyToOpenForms()
        PreviewTheme(theme)
    End Sub

    Private Sub PreviewTheme(theme As AppTheme)
        _descriptionLabel.Text = theme.Description
        _previewPanel.BackColor = theme.WindowBackColor
        _previewPanel.ForeColor = theme.TextColor
        _previewTitle.BackColor = Color.Transparent
        _previewTitle.ForeColor = theme.TextColor
        _previewSubtitle.BackColor = Color.Transparent
        _previewSubtitle.ForeColor = theme.MutedTextColor
        _previewInput.BackColor = theme.InputBackColor
        _previewInput.ForeColor = theme.TextColor

        For Each row In _previewRows
            row.BackColor = theme.SurfaceBackColor
            row.ForeColor = theme.TextColor
            For Each child As Control In row.Controls
                child.BackColor = Color.Transparent
                child.ForeColor = theme.TextColor
            Next
        Next

        StylePreviewButton(_previewPrimaryButton, theme, True)
        StylePreviewButton(_previewSecondaryButton, theme, False)
    End Sub

    Private Shared Sub StylePreviewButton(
        button As Button,
        theme As AppTheme,
        primary As Boolean
    )
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
    End Sub

    Private Sub ApplyButton_Click(sender As Object, e As EventArgs)
        ThemeManager.SelectTheme(_selectedThemeKey, True)
        ThemeManager.ApplyToOpenForms()
        _saved = True
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs)
        RestoreOriginalTheme()
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub SettingsForm_FormClosing(
        sender As Object,
        e As FormClosingEventArgs
    )
        If Not _saved Then RestoreOriginalTheme()
    End Sub

    Private Sub RestoreOriginalTheme()
        ThemeManager.SelectTheme(_originalThemeKey, False)
        ThemeManager.ApplyToOpenForms()
    End Sub
End Class
