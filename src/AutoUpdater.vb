Imports System.IO
Imports System.IO.Compression
Imports System.Net.Http
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public NotInheritable Class AutoUpdater
    Private Const LatestReleaseUrl As String =
        "https://api.github.com/repos/maramizo/DietPlanner/releases/latest"
    Private Const ReleaseArchiveName As String = "release.zip"
    Private Const ReleaseChecksumName As String = "release.zip.sha256"
    Private Const MaximumArchiveBytes As Long = 250L * 1024L * 1024L

    Private Sub New()
    End Sub

    Public Shared Async Function TryInstallLatestReleaseAsync(owner As Form) As Task(Of Boolean)
        CleanupStaleUpdates()

        Dim releaseTag As String = Nothing
        Dim loader As Loading = Nothing

        Try
            Using client As HttpClient = CreateHttpClient()
                Dim release = Await GetLatestReleaseAsync(client)
                releaseTag = release.TagName

                Dim releaseVersion = ParseReleaseVersion(release.TagName)
                If releaseVersion Is Nothing OrElse releaseVersion <= GetCurrentVersion() Then
                    Return False
                End If

                If HasFailedForRelease(release.TagName) Then Return False

                loader = New Loading("Downloading DietPlanner " & release.TagName & "...")
                loader.Show(owner)
                loader.Refresh()

                Dim updateRoot = CreateUpdateDirectory()
                Dim archivePath = Path.Combine(updateRoot, ReleaseArchiveName)
                Dim stagingDirectory = Path.Combine(updateRoot, "staged")
                Dim backupDirectory = Path.Combine(updateRoot, "backup")

                Await DownloadFileAsync(client, release.Archive.DownloadUrl, archivePath)
                Dim expectedChecksum = Await GetExpectedChecksumAsync(client, release)
                Await VerifyChecksumAsync(archivePath, expectedChecksum)

                loader.UpdateMessage("Preparing DietPlanner " & release.TagName & "...")
                ExtractRelease(archivePath, stagingDirectory)
                ValidateRelease(stagingDirectory)

                Dim updaterScript = WriteUpdaterScript(updateRoot)
                StartUpdater(
                    updaterScript,
                    stagingDirectory,
                    backupDirectory,
                    AppContext.BaseDirectory,
                    Application.ExecutablePath,
                    release.TagName
                )

                Return True
            End Using
        Catch ex As Exception
            If Not String.IsNullOrWhiteSpace(releaseTag) Then
                RecordFailedRelease(releaseTag, ex.Message)
                MessageBox.Show(
                    "DietPlanner found an update, but could not install it." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "DietPlanner update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
            End If

            Return False
        Finally
            If loader IsNot Nothing Then loader.Close()
        End Try
    End Function

    Private Shared Function CreateHttpClient() As HttpClient
        Dim client As New HttpClient() With {
            .Timeout = TimeSpan.FromSeconds(45)
        }
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "DietPlanner/" & GetCurrentVersion().ToString()
        )
        Return client
    End Function

    Private Shared Async Function GetLatestReleaseAsync(client As HttpClient) As Task(Of ReleaseInfo)
        Using response = Await client.GetAsync(LatestReleaseUrl)
            response.EnsureSuccessStatusCode()
            Dim document = JObject.Parse(Await response.Content.ReadAsStringAsync())
            Dim tagName = CStr(document("tag_name"))
            If String.IsNullOrWhiteSpace(tagName) Then
                Throw New InvalidDataException("The latest release does not have a version tag.")
            End If

            Dim archive = FindAsset(document, ReleaseArchiveName)
            If archive Is Nothing Then
                Throw New InvalidDataException(
                    "The latest release does not include " & ReleaseArchiveName & "."
                )
            End If

            Return New ReleaseInfo With {
                .TagName = tagName.Trim(),
                .Archive = archive,
                .Checksum = FindAsset(document, ReleaseChecksumName)
            }
        End Using
    End Function

    Private Shared Function FindAsset(document As JObject, assetName As String) As ReleaseAsset
        Dim assets = TryCast(document("assets"), JArray)
        If assets Is Nothing Then Return Nothing

        For Each token As JToken In assets
            Dim asset = TryCast(token, JObject)
            If asset Is Nothing Then Continue For
            If Not String.Equals(CStr(asset("name")), assetName, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            Dim downloadUrl = CStr(asset("browser_download_url"))
            Dim downloadUri As Uri = Nothing
            If Not Uri.TryCreate(downloadUrl, UriKind.Absolute, downloadUri) OrElse
                Not String.Equals(downloadUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException(
                    "The release asset has an invalid download address."
                )
            End If

            Return New ReleaseAsset With {
                .DownloadUrl = downloadUri,
                .Digest = CStr(asset("digest"))
            }
        Next

        Return Nothing
    End Function

    Private Shared Function ParseReleaseVersion(tagName As String) As Version
        If String.IsNullOrWhiteSpace(tagName) Then Return Nothing

        Dim normalized = tagName.Trim().TrimStart("v"c, "V"c)
        If Regex.IsMatch(normalized, "^\d+$") Then
            Dim legacyBuild As Integer
            If Integer.TryParse(normalized, legacyBuild) Then
                Return New Version(0, 0, legacyBuild)
            End If
            Return Nothing
        End If

        Dim metadataIndex = normalized.IndexOfAny({"-"c, "+"c})
        If metadataIndex >= 0 Then normalized = normalized.Substring(0, metadataIndex)

        Dim parsed As Version = Nothing
        If Not Version.TryParse(normalized, parsed) Then Return Nothing
        Return New Version(
            Math.Max(0, parsed.Major),
            Math.Max(0, parsed.Minor),
            Math.Max(0, parsed.Build),
            Math.Max(0, parsed.Revision)
        )
    End Function

    Private Shared Function GetCurrentVersion() As Version
        Dim version = Assembly.GetExecutingAssembly().GetName().Version
        If version Is Nothing Then Return New Version(0, 0, 0, 0)
        Return New Version(
            Math.Max(0, version.Major),
            Math.Max(0, version.Minor),
            Math.Max(0, version.Build),
            Math.Max(0, version.Revision)
        )
    End Function

    Private Shared Async Function DownloadFileAsync(
        client As HttpClient,
        downloadUrl As Uri,
        destination As String
    ) As Task
        Using response = Await client.GetAsync(
            downloadUrl,
            HttpCompletionOption.ResponseHeadersRead
        )
            response.EnsureSuccessStatusCode()

            Dim contentLength = response.Content.Headers.ContentLength
            If contentLength.HasValue AndAlso contentLength.Value > MaximumArchiveBytes Then
                Throw New InvalidDataException("The release archive is unexpectedly large.")
            End If

            Using input = Await response.Content.ReadAsStreamAsync()
                Using output As New FileStream(
                    destination,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync:=True
                )
                    Await input.CopyToAsync(output)
                    If output.Length > MaximumArchiveBytes Then
                        Throw New InvalidDataException("The release archive is unexpectedly large.")
                    End If
                End Using
            End Using
        End Using
    End Function

    Private Shared Async Function GetExpectedChecksumAsync(
        client As HttpClient,
        release As ReleaseInfo
    ) As Task(Of String)
        If Not String.IsNullOrWhiteSpace(release.Archive.Digest) Then
            Dim digestMatch = Regex.Match(
                release.Archive.Digest,
                "^sha256:([0-9a-f]{64})$",
                RegexOptions.IgnoreCase
            )
            If digestMatch.Success Then Return digestMatch.Groups(1).Value.ToLowerInvariant()
        End If

        If release.Checksum Is Nothing Then
            Throw New InvalidDataException(
                "The release does not include a SHA-256 checksum."
            )
        End If

        Using response = Await client.GetAsync(release.Checksum.DownloadUrl)
            response.EnsureSuccessStatusCode()
            Dim checksumDocument = Await response.Content.ReadAsStringAsync()
            If checksumDocument.Length > 4096 Then
                Throw New InvalidDataException("The release checksum file is invalid.")
            End If

            Dim checksumMatch = Regex.Match(
                checksumDocument,
                "\b[0-9a-f]{64}\b",
                RegexOptions.IgnoreCase
            )
            If Not checksumMatch.Success Then
                Throw New InvalidDataException("The release checksum file is invalid.")
            End If

            Return checksumMatch.Value.ToLowerInvariant()
        End Using
    End Function

    Private Shared Async Function VerifyChecksumAsync(
        archivePath As String,
        expectedChecksum As String
    ) As Task
        Dim actualChecksum As String
        Using input = File.OpenRead(archivePath)
            Using hasher = SHA256.Create()
                actualChecksum = Convert.ToHexString(
                    Await hasher.ComputeHashAsync(input)
                ).ToLowerInvariant()
            End Using
        End Using

        If Not String.Equals(
            actualChecksum,
            expectedChecksum,
            StringComparison.OrdinalIgnoreCase
        ) Then
            Throw New InvalidDataException(
                "The downloaded update did not match its SHA-256 checksum."
            )
        End If
    End Function

    Private Shared Sub ExtractRelease(archivePath As String, stagingDirectory As String)
        Directory.CreateDirectory(stagingDirectory)
        Dim stagingRoot = Path.GetFullPath(stagingDirectory).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        ) & Path.DirectorySeparatorChar

        Using archive = ZipFile.OpenRead(archivePath)
            For Each entry In archive.Entries
                Dim destination = Path.GetFullPath(
                    Path.Combine(stagingDirectory, entry.FullName)
                )
                If Not destination.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidDataException(
                        "The release archive contains an unsafe file path."
                    )
                End If

                If String.IsNullOrEmpty(entry.Name) Then
                    Directory.CreateDirectory(destination)
                Else
                    Dim parent = Path.GetDirectoryName(destination)
                    If Not String.IsNullOrEmpty(parent) Then Directory.CreateDirectory(parent)
                    entry.ExtractToFile(destination, overwrite:=True)
                End If
            Next
        End Using
    End Sub

    Private Shared Sub ValidateRelease(stagingDirectory As String)
        For Each requiredFile In {
            "DietPlanner.exe",
            "DietPlanner.dll",
            "DietPlanner.runtimeconfig.json"
        }
            If Not File.Exists(Path.Combine(stagingDirectory, requiredFile)) Then
                Throw New InvalidDataException(
                    "The release archive is missing " & requiredFile & "."
                )
            End If
        Next
    End Sub

    Private Shared Function CreateUpdateDirectory() As String
        Dim updateRoot = Path.Combine(
            GetUpdatesDirectory(),
            DateTime.UtcNow.ToString("yyyyMMddHHmmss") & "-" & Guid.NewGuid().ToString("N")
        )
        Directory.CreateDirectory(updateRoot)
        Return updateRoot
    End Function

    Private Shared Function WriteUpdaterScript(updateRoot As String) As String
        Dim scriptPath = Path.Combine(updateRoot, "Install-DietPlannerUpdate.ps1")
        Dim script = String.Join(
            Environment.NewLine,
            {
                "param(",
                "    [Parameter(Mandatory=$true)][int]$ProcessId,",
                "    [Parameter(Mandatory=$true)][string]$SourceDirectory,",
                "    [Parameter(Mandatory=$true)][string]$BackupDirectory,",
                "    [Parameter(Mandatory=$true)][string]$TargetDirectory,",
                "    [Parameter(Mandatory=$true)][string]$ExecutablePath,",
                "    [Parameter(Mandatory=$true)][string]$FailureMarkerPath,",
                "    [Parameter(Mandatory=$true)][string]$ReleaseTag",
                ")",
                "$ErrorActionPreference = 'Stop'",
                "$sourceRoot = [IO.Path]::GetFullPath($SourceDirectory).TrimEnd([char[]]'\/')",
                "$targetRoot = [IO.Path]::GetFullPath($TargetDirectory).TrimEnd([char[]]'\/')",
                "$createdFiles = New-Object 'System.Collections.Generic.List[string]'",
                "try {",
                "    Wait-Process -Id $ProcessId -ErrorAction SilentlyContinue",
                "    New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null",
                "    $sourceFiles = Get-ChildItem -LiteralPath $sourceRoot -File -Recurse -Force",
                "    foreach ($sourceFile in $sourceFiles) {",
                "        $relativePath = $sourceFile.FullName.Substring($sourceRoot.Length).TrimStart([char[]]'\/')",
                "        if ($relativePath.Split([char[]]'\/')[0] -ieq 'data') { continue }",
                "        $destination = Join-Path $targetRoot $relativePath",
                "        $destinationParent = Split-Path -Parent $destination",
                "        New-Item -ItemType Directory -Path $destinationParent -Force | Out-Null",
                "        if (Test-Path -LiteralPath $destination -PathType Leaf) {",
                "            $backupPath = Join-Path $BackupDirectory $relativePath",
                "            New-Item -ItemType Directory -Path (Split-Path -Parent $backupPath) -Force | Out-Null",
                "            Copy-Item -LiteralPath $destination -Destination $backupPath -Force",
                "        } else {",
                "            $createdFiles.Add($destination)",
                "        }",
                "        Copy-Item -LiteralPath $sourceFile.FullName -Destination $destination -Force",
                "    }",
                "    Remove-Item -LiteralPath $FailureMarkerPath -Force -ErrorAction SilentlyContinue",
                "    Start-Process -FilePath $ExecutablePath -WorkingDirectory $targetRoot",
                "} catch {",
                "    $updateError = $_.Exception.ToString()",
                "    try {",
                "        if (Test-Path -LiteralPath $BackupDirectory) {",
                "            foreach ($backupFile in Get-ChildItem -LiteralPath $BackupDirectory -File -Recurse -Force) {",
                "                $relativePath = $backupFile.FullName.Substring($BackupDirectory.Length).TrimStart([char[]]'\/')",
                "                $destination = Join-Path $targetRoot $relativePath",
                "                New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null",
                "                Copy-Item -LiteralPath $backupFile.FullName -Destination $destination -Force",
                "            }",
                "        }",
                "        foreach ($createdFile in $createdFiles) {",
                "            Remove-Item -LiteralPath $createdFile -Force -ErrorAction SilentlyContinue",
                "        }",
                "        New-Item -ItemType Directory -Path (Split-Path -Parent $FailureMarkerPath) -Force | Out-Null",
                "        [IO.File]::WriteAllText($FailureMarkerPath, $ReleaseTag + [Environment]::NewLine + $updateError)",
                "    } catch { }",
                "    try { Start-Process -FilePath $ExecutablePath -WorkingDirectory $targetRoot } catch { }",
                "}"
            }
        )
        File.WriteAllText(scriptPath, script, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True))
        Return scriptPath
    End Function

    Private Shared Sub StartUpdater(
        scriptPath As String,
        stagingDirectory As String,
        backupDirectory As String,
        targetDirectory As String,
        executablePath As String,
        releaseTag As String
    )
        Dim powerShellPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe"
        )
        If Not File.Exists(powerShellPath) Then powerShellPath = "powershell.exe"

        Dim startInfo As New ProcessStartInfo(powerShellPath) With {
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .WorkingDirectory = Path.GetDirectoryName(scriptPath)
        }
        For Each argument In {
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            scriptPath,
            "-ProcessId",
            Process.GetCurrentProcess().Id.ToString(),
            "-SourceDirectory",
            stagingDirectory,
            "-BackupDirectory",
            backupDirectory,
            "-TargetDirectory",
            targetDirectory,
            "-ExecutablePath",
            executablePath,
            "-FailureMarkerPath",
            GetFailureMarkerPath(),
            "-ReleaseTag",
            releaseTag
        }
            startInfo.ArgumentList.Add(argument)
        Next

        Dim updater = Process.Start(startInfo)
        If updater Is Nothing Then
            Throw New InvalidOperationException("Windows could not start the update installer.")
        End If
    End Sub

    Private Shared Function HasFailedForRelease(releaseTag As String) As Boolean
        Try
            Dim markerPath = GetFailureMarkerPath()
            If Not File.Exists(markerPath) Then Return False
            Dim firstLine = File.ReadLines(markerPath).FirstOrDefault()
            Return String.Equals(firstLine, releaseTag, StringComparison.OrdinalIgnoreCase)
        Catch
            Return False
        End Try
    End Function

    Private Shared Sub RecordFailedRelease(releaseTag As String, message As String)
        Try
            Dim markerPath = GetFailureMarkerPath()
            Directory.CreateDirectory(Path.GetDirectoryName(markerPath))
            File.WriteAllText(
                markerPath,
                releaseTag & Environment.NewLine & message
            )
        Catch
        End Try
    End Sub

    Private Shared Function GetFailureMarkerPath() As String
        Return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DietPlanner",
            "update-failure.txt"
        )
    End Function

    Private Shared Function GetUpdatesDirectory() As String
        Return Path.Combine(Path.GetTempPath(), "DietPlanner", "Updates")
    End Function

    Private Shared Sub CleanupStaleUpdates()
        Try
            Dim updatesDirectory = GetUpdatesDirectory()
            If Not Directory.Exists(updatesDirectory) Then Return

            For Each directory In New DirectoryInfo(updatesDirectory).EnumerateDirectories()
                If directory.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-3) Then
                    directory.Delete(recursive:=True)
                End If
            Next
        Catch
        End Try
    End Sub

    Private Class ReleaseInfo
        Public Property TagName As String
        Public Property Archive As ReleaseAsset
        Public Property Checksum As ReleaseAsset
    End Class

    Private Class ReleaseAsset
        Public Property DownloadUrl As Uri
        Public Property Digest As String
    End Class
End Class
