Imports System.IO
Imports System.Text

Public Module Program
    <STAThread>
    Public Sub Main()
        Dim arguments = Environment.GetCommandLineArgs().Skip(1).ToArray()
        If NativeMessagingHost.IsNativeMessagingInvocation(arguments) Then
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            Directory.SetCurrentDirectory(AppContext.BaseDirectory)
            Environment.ExitCode = NativeMessagingHost.RunAsync(
                Console.OpenStandardInput(),
                Console.OpenStandardOutput()
            ).GetAwaiter().GetResult()
            Return
        End If

        My.Application.Run(arguments)
    End Sub
End Module
