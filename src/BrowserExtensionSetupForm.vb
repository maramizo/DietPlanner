Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Public Class BrowserExtensionSetupForm
    Inherits Form

    Private ReadOnly _chromeButton As New Button()
    Private ReadOnly _edgeButton As New Button()
    Private ReadOnly _statusLabel As New Label()
    Private ReadOnly _extensionPathTextBox As New TextBox()

    Public Sub New()
        InitializeSetupForm()
        ApplyAppIcon(Me)
        RefreshBrowserStatus()
    End Sub

    Private Sub InitializeSetupForm()
        Text = "Install the DietPlanner Browser Extension"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(720, 455)

        Dim heading As New Label With {
            .AutoSize = True,
            .Font = New Font(Font, FontStyle.Bold),
            .Location = New Point(22, 20),
            .Text = "Add recipes straight from your browser"
        }
        Dim description As New Label With {
            .Location = New Point(22, 50),
            .Size = New Size(675, 42),
            .Text =
                "DietPlanner includes the extension and its secure Windows connection. " &
                "Browser security requires one manual Load unpacked step."
        }

        Dim stepsGroup As New GroupBox With {
            .Location = New Point(20, 96),
            .Size = New Size(680, 135),
            .Text = "Install in three steps"
        }
        Dim steps As New Label With {
            .Location = New Point(16, 26),
            .Size = New Size(646, 96),
            .Text =
                "1. Select your browser below. DietPlanner registers its local connection and opens both pages." &
                Environment.NewLine &
                "2. On the Extensions page, turn on Developer mode and select Load unpacked." &
                Environment.NewLine &
                "3. Choose the BrowserExtension folder that DietPlanner opened. The folder path is also copied."
        }
        stepsGroup.Controls.Add(steps)

        Dim browserGroup As New GroupBox With {
            .Location = New Point(20, 243),
            .Size = New Size(680, 86),
            .Text = "Choose a browser"
        }
        _chromeButton.Location = New Point(16, 31)
        _chromeButton.Name = "SetUpChromeButton"
        _chromeButton.Size = New Size(310, 34)
        AddHandler _chromeButton.Click,
            Sub() SetUpBrowser(SupportedChromiumBrowser.Chrome)

        _edgeButton.Location = New Point(350, 31)
        _edgeButton.Name = "SetUpEdgeButton"
        _edgeButton.Size = New Size(310, 34)
        AddHandler _edgeButton.Click,
            Sub() SetUpBrowser(SupportedChromiumBrowser.Edge)
        browserGroup.Controls.AddRange({_chromeButton, _edgeButton})

        Dim pathLabel As New Label With {
            .AutoSize = True,
            .Location = New Point(22, 345),
            .Text = "Bundled extension folder"
        }
        _extensionPathTextBox.Location = New Point(22, 368)
        _extensionPathTextBox.Name = "ExtensionPathTextBox"
        _extensionPathTextBox.ReadOnly = True
        _extensionPathTextBox.Size = New Size(437, 23)
        _extensionPathTextBox.Text =
            BrowserExtensionInstaller.GetBundledExtensionDirectory()

        Dim copyButton As New Button With {
            .Location = New Point(467, 367),
            .Name = "CopyExtensionPathButton",
            .Size = New Size(105, 26),
            .Text = "Copy Path"
        }
        AddHandler copyButton.Click, AddressOf CopyPathButton_Click

        Dim openFolderButton As New Button With {
            .Location = New Point(580, 367),
            .Name = "OpenExtensionFolderButton",
            .Size = New Size(120, 26),
            .Text = "Open Folder"
        }
        AddHandler openFolderButton.Click, AddressOf OpenFolderButton_Click

        _statusLabel.Location = New Point(22, 405)
        _statusLabel.Name = "ExtensionSetupStatusLabel"
        _statusLabel.Size = New Size(540, 40)
        _statusLabel.Text =
            "After the first Codex sign-in, the extension can import several pages in parallel while the app is closed."

        Dim closeButton As New Button With {
            .DialogResult = DialogResult.OK,
            .Location = New Point(580, 410),
            .Name = "CloseExtensionSetupButton",
            .Size = New Size(120, 28),
            .Text = "Close"
        }

        AcceptButton = closeButton
        CancelButton = closeButton
        Controls.AddRange({
            heading,
            description,
            stepsGroup,
            browserGroup,
            pathLabel,
            _extensionPathTextBox,
            copyButton,
            openFolderButton,
            _statusLabel,
            closeButton
        })
    End Sub

    Private Sub RefreshBrowserStatus()
        _chromeButton.Text = GetSetupButtonText(
            SupportedChromiumBrowser.Chrome
        )
        _edgeButton.Text = GetSetupButtonText(
            SupportedChromiumBrowser.Edge
        )
    End Sub

    Private Function GetSetupButtonText(
        browser As SupportedChromiumBrowser
    ) As String
        Dim browserName =
            BrowserExtensionInstaller.GetBrowserDisplayName(browser)
        If BrowserExtensionInstaller.IsNativeHostRegistered(browser) Then
            Return browserName & " — connection registered"
        End If
        Return "Set Up " & browserName
    End Function

    Private Sub SetUpBrowser(browser As SupportedChromiumBrowser)
        Dim browserName =
            BrowserExtensionInstaller.GetBrowserDisplayName(browser)
        Try
            BrowserExtensionInstaller.RegisterNativeHost(browser)
            CopyExtensionPath()
            BrowserExtensionInstaller.OpenBundledExtensionFolder()
            BrowserExtensionInstaller.OpenExtensionsPage(browser)
            RefreshBrowserStatus()
            _statusLabel.Text =
                browserName &
                " is ready for Load unpacked. Select the folder whose path was copied."
        Catch ex As Exception
            _statusLabel.Text = browserName & " setup failed: " & ex.Message
            MessageBox.Show(
                ex.Message,
                "Browser extension setup",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
        End Try
    End Sub

    Private Sub CopyPathButton_Click(sender As Object, e As EventArgs)
        Try
            CopyExtensionPath()
            _statusLabel.Text = "The bundled extension folder path was copied."
        Catch ex As Exception
            _statusLabel.Text = "Could not copy the folder path: " & ex.Message
        End Try
    End Sub

    Private Sub OpenFolderButton_Click(sender As Object, e As EventArgs)
        Try
            BrowserExtensionInstaller.OpenBundledExtensionFolder()
            _statusLabel.Text = "The bundled extension folder is open."
        Catch ex As Exception
            _statusLabel.Text = "Could not open the extension folder: " & ex.Message
        End Try
    End Sub

    Private Sub CopyExtensionPath()
        Dim extensionDirectory =
            BrowserExtensionInstaller.GetBundledExtensionDirectory()
        If Not Directory.Exists(extensionDirectory) Then
            Throw New DirectoryNotFoundException(
                "The bundled BrowserExtension folder is missing."
            )
        End If
        Clipboard.SetText(extensionDirectory)
    End Sub
End Class
