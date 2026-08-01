Imports System.Diagnostics
Imports System.IO
Imports System.Security.Cryptography
Imports Microsoft.Win32
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Enum SupportedChromiumBrowser
    Chrome
    Edge
End Enum

Public NotInheritable Class BrowserExtensionInstaller
    Public Const NativeHostName As String =
        "com.dietplanner.recipe_importer"
    ' This ID is derived from BrowserExtension/manifest.json's public key.
    ' Keeping it stable preserves the native-host permission after app updates.
    Public Const ExtensionId As String =
        "pjamdohcbdickbfhdfjjlpipkooocjoj"
    Public Const AllowedExtensionOrigin As String =
        "chrome-extension://" & ExtensionId & "/"

    Private Sub New()
    End Sub

    Public Shared Function GetBundledExtensionDirectory() As String
        Return Path.Combine(AppContext.BaseDirectory, "BrowserExtension")
    End Function

    Public Shared Function RegisterNativeHost(
        browser As SupportedChromiumBrowser
    ) As String
        ValidateBundledExtension()

        Dim manifestDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DietPlanner",
            "BrowserExtension"
        )
        Directory.CreateDirectory(manifestDirectory)
        Dim manifestPath = Path.Combine(
            manifestDirectory,
            "native-messaging-host.json"
        )
        File.WriteAllText(
            manifestPath,
            CreateNativeHostManifest(GetDietPlannerExecutablePath()),
            New Text.UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)
        )

        Using hostKey = Registry.CurrentUser.CreateSubKey(
            GetNativeHostRegistryPath(browser),
            writable:=True
        )
            If hostKey Is Nothing Then
                Throw New UnauthorizedAccessException(
                    "Windows could not register the DietPlanner browser connection."
                )
            End If
            hostKey.SetValue(
                Nothing,
                manifestPath,
                RegistryValueKind.String
            )
        End Using
        Return manifestPath
    End Function

    Public Shared Sub RepairExistingRegistrations()
        For Each browser In {
            SupportedChromiumBrowser.Chrome,
            SupportedChromiumBrowser.Edge
        }
            Try
                Using hostKey = Registry.CurrentUser.OpenSubKey(
                    GetNativeHostRegistryPath(browser),
                    writable:=False
                )
                    If hostKey Is Nothing Then Continue For
                End Using
                RegisterNativeHost(browser)
            Catch ex As IOException
            Catch ex As UnauthorizedAccessException
            Catch ex As Security.SecurityException
            End Try
        Next
    End Sub

    Public Shared Function IsNativeHostRegistered(
        browser As SupportedChromiumBrowser
    ) As Boolean
        Try
            Using hostKey = Registry.CurrentUser.OpenSubKey(
                GetNativeHostRegistryPath(browser),
                writable:=False
            )
                If hostKey Is Nothing Then Return False
                Dim manifestPath = TryCast(hostKey.GetValue(Nothing), String)
                Return Not String.IsNullOrWhiteSpace(manifestPath) AndAlso
                    File.Exists(manifestPath)
            End Using
        Catch ex As IOException
            Return False
        Catch ex As UnauthorizedAccessException
            Return False
        Catch ex As Security.SecurityException
            Return False
        End Try
    End Function

    Public Shared Function CreateNativeHostManifest(
        executablePath As String
    ) As String
        If String.IsNullOrWhiteSpace(executablePath) Then
            Throw New ArgumentException(
                "The DietPlanner executable path is missing.",
                NameOf(executablePath)
            )
        End If

        Return New JObject(
            New JProperty("name", NativeHostName),
            New JProperty(
                "description",
                "Import the current browser recipe into DietPlanner"
            ),
            New JProperty("path", Path.GetFullPath(executablePath)),
            New JProperty("type", "stdio"),
            New JProperty(
                "allowed_origins",
                New JArray(AllowedExtensionOrigin)
            )
        ).ToString(Formatting.Indented)
    End Function

    Public Shared Sub OpenBundledExtensionFolder()
        ValidateBundledExtension()
        Dim startInfo As New ProcessStartInfo("explorer.exe") With {
            .UseShellExecute = True
        }
        startInfo.ArgumentList.Add(GetBundledExtensionDirectory())
        If Process.Start(startInfo) Is Nothing Then
            Throw New InvalidOperationException(
                "Windows could not open the bundled extension folder."
            )
        End If
    End Sub

    Public Shared Sub OpenExtensionsPage(
        browser As SupportedChromiumBrowser
    )
        Dim executablePath = FindBrowserExecutable(browser)
        If executablePath Is Nothing Then
            Throw New FileNotFoundException(
                GetBrowserDisplayName(browser) &
                " was not found on this computer."
            )
        End If

        Dim extensionsPage = If(
            browser = SupportedChromiumBrowser.Edge,
            "edge://extensions",
            "chrome://extensions"
        )
        Dim startInfo As New ProcessStartInfo(executablePath) With {
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .WorkingDirectory = Path.GetDirectoryName(executablePath)
        }
        startInfo.ArgumentList.Add(extensionsPage)
        If Process.Start(startInfo) Is Nothing Then
            Throw New InvalidOperationException(
                "Windows could not open " &
                GetBrowserDisplayName(browser) &
                "."
            )
        End If
    End Sub

    Public Shared Function GetBrowserDisplayName(
        browser As SupportedChromiumBrowser
    ) As String
        Return If(
            browser = SupportedChromiumBrowser.Edge,
            "Microsoft Edge",
            "Google Chrome"
        )
    End Function

    Private Shared Sub ValidateBundledExtension()
        Dim extensionDirectory = GetBundledExtensionDirectory()
        For Each requiredFile In {
            "manifest.json",
            "popup.html",
            "popup.js",
            "service-worker.js",
            "styles.css",
            "icon.png"
        }
            If Not File.Exists(Path.Combine(extensionDirectory, requiredFile)) Then
                Throw New FileNotFoundException(
                    "The DietPlanner release is missing the bundled browser extension file " &
                    requiredFile &
                    "."
                )
            End If
        Next

        Dim manifestPath = Path.Combine(extensionDirectory, "manifest.json")
        Try
            Dim manifest = JObject.Parse(File.ReadAllText(manifestPath))
            Dim publicKey = manifest.Value(Of String)("key")
            If Not String.Equals(
                CalculateExtensionId(publicKey),
                ExtensionId,
                StringComparison.Ordinal
            ) Then
                Throw New InvalidDataException(
                    "The bundled extension key does not match DietPlanner's registered extension ID."
                )
            End If
        Catch ex As JsonException
            Throw New InvalidDataException(
                "The bundled browser extension manifest is invalid.",
                ex
            )
        Catch ex As FormatException
            Throw New InvalidDataException(
                "The bundled browser extension key is invalid.",
                ex
            )
        End Try
    End Sub

    Private Shared Function CalculateExtensionId(publicKey As String) As String
        If String.IsNullOrWhiteSpace(publicKey) Then
            Throw New InvalidDataException(
                "The bundled browser extension does not have a stable key."
            )
        End If

        Dim hash = SHA256.HashData(Convert.FromBase64String(publicKey))
        Dim extensionId As New Text.StringBuilder(32)
        For index As Integer = 0 To 15
            Dim value = CInt(hash(index))
            extensionId.Append(ChrW(AscW("a"c) + (value >> 4)))
            extensionId.Append(ChrW(AscW("a"c) + (value And &HF)))
        Next
        Return extensionId.ToString()
    End Function

    Private Shared Function GetNativeHostRegistryPath(
        browser As SupportedChromiumBrowser
    ) As String
        Dim browserPath = If(
            browser = SupportedChromiumBrowser.Edge,
            "Microsoft\Edge",
            "Google\Chrome"
        )
        Return "Software\" & browserPath &
            "\NativeMessagingHosts\" &
            NativeHostName
    End Function

    Private Shared Function GetDietPlannerExecutablePath() As String
        Dim bundledExecutable = Path.Combine(
            AppContext.BaseDirectory,
            "DietPlanner.exe"
        )
        If File.Exists(bundledExecutable) Then Return bundledExecutable

        Dim processPath = Environment.ProcessPath
        If Not String.IsNullOrWhiteSpace(processPath) AndAlso
            File.Exists(processPath) Then
            Return processPath
        End If
        Throw New FileNotFoundException(
            "DietPlanner could not locate its Windows executable."
        )
    End Function

    Private Shared Function FindBrowserExecutable(
        browser As SupportedChromiumBrowser
    ) As String
        Dim executableName = If(
            browser = SupportedChromiumBrowser.Edge,
            "msedge.exe",
            "chrome.exe"
        )
        For Each root In {Registry.CurrentUser, Registry.LocalMachine}
            Try
                Using appPathKey = root.OpenSubKey(
                    "Software\Microsoft\Windows\CurrentVersion\App Paths\" &
                    executableName
                )
                    Dim registeredPath = TryCast(
                        appPathKey?.GetValue(Nothing),
                        String
                    )
                    If Not String.IsNullOrWhiteSpace(registeredPath) AndAlso
                        File.Exists(registeredPath) Then
                        Return registeredPath
                    End If
                End Using
            Catch ex As Security.SecurityException
            End Try
        Next

        Dim relativePath = If(
            browser = SupportedChromiumBrowser.Edge,
            Path.Combine("Microsoft", "Edge", "Application", executableName),
            Path.Combine("Google", "Chrome", "Application", executableName)
        )
        For Each rootDirectory In {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        }
            If String.IsNullOrWhiteSpace(rootDirectory) Then Continue For
            Dim candidate = Path.Combine(rootDirectory, relativePath)
            If File.Exists(candidate) Then Return candidate
        Next
        Return Nothing
    End Function
End Class
