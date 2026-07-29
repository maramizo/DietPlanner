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
            Return
        End If

        Using executableIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
            If executableIcon IsNot Nothing Then
                form.Icon = DirectCast(executableIcon.Clone(), Icon)
            End If
        End Using
    End Sub
End Module
