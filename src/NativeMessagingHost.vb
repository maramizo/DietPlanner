Imports System.Buffers.Binary
Imports System.Collections.Concurrent
Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public NotInheritable Class NativeMessagingHost
    Private Const MaximumMessageBytes As Integer = 64 * 1024 * 1024
    Private Shared ReadOnly StrictUtf8 As New UTF8Encoding(
        encoderShouldEmitUTF8Identifier:=False,
        throwOnInvalidBytes:=True
    )

    Private Sub New()
    End Sub

    Public Shared Function IsNativeMessagingInvocation(
        arguments As IEnumerable(Of String)
    ) As Boolean
        Dim values = If(arguments, Enumerable.Empty(Of String)()).ToList()
        Return values.Any(
            Function(value) String.Equals(
                value,
                "--native-messaging-host",
                StringComparison.OrdinalIgnoreCase
            )
        ) OrElse values.Any(
            Function(value) String.Equals(
                value,
                BrowserExtensionInstaller.AllowedExtensionOrigin,
                StringComparison.OrdinalIgnoreCase
            )
        )
    End Function

    Public Shared Async Function RunAsync(
        input As Stream,
        output As Stream,
        Optional cancellationToken As Threading.CancellationToken = Nothing
    ) As Task(Of Integer)
        If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
        If output Is Nothing Then Throw New ArgumentNullException(NameOf(output))

        Dim writer As New NativeMessageWriter(output)
        Dim jobs As New ConcurrentDictionary(Of Guid, Task)()
        Try
            Do
                Dim messageText = Await ReadMessageAsync(
                    input,
                    cancellationToken
                )
                If messageText Is Nothing Then Exit Do

                Dim message As JObject = Nothing
                Try
                    message = JObject.Parse(messageText)
                Catch ex As JsonException
                End Try
                If message Is Nothing Then
                    Await writer.TryWriteAsync(
                        CreateStatusMessage(
                            String.Empty,
                            String.Empty,
                            "failed",
                            "DietPlanner received an invalid browser-extension message."
                        ),
                        cancellationToken
                    )
                    Continue Do
                End If

                Dim messageType = If(
                    message.Value(Of String)("type"),
                    String.Empty
                )
                If String.Equals(
                    messageType,
                    "ping",
                    StringComparison.OrdinalIgnoreCase
                ) Then
                    Await writer.TryWriteAsync(
                        New JObject(
                            New JProperty("type", "pong"),
                            New JProperty(
                                "requestId",
                                message.Value(Of String)("requestId")
                            )
                        ),
                        cancellationToken
                    )
                    Continue Do
                End If

                If Not String.Equals(
                    messageType,
                    "add_recipe",
                    StringComparison.OrdinalIgnoreCase
                ) Then
                    Await writer.TryWriteAsync(
                        CreateStatusMessage(
                            message.Value(Of String)("jobId"),
                            message.Value(Of String)("url"),
                            "failed",
                            "DietPlanner does not recognize this extension request."
                        ),
                        cancellationToken
                    )
                    Continue Do
                End If

                Dim job = ParseJob(message)
                If job.ValidationError <> String.Empty Then
                    Await writer.TryWriteAsync(
                        CreateStatusMessage(
                            job.JobId,
                            job.Url,
                            "failed",
                            job.ValidationError
                        ),
                        cancellationToken
                    )
                    Continue Do
                End If

                Dim taskKey = Guid.NewGuid()
                Dim jobTask = ProcessRecipeJobAsync(
                    job,
                    writer,
                    cancellationToken
                )
                jobs.TryAdd(taskKey, jobTask)
                Dim cleanupTask = jobTask.ContinueWith(
                    Sub(completedTask)
                        Dim removedTask As Task = Nothing
                        jobs.TryRemove(taskKey, removedTask)
                    End Sub,
                    Threading.CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default
                )
            Loop

            Await Task.WhenAll(jobs.Values.ToArray())
            Return 0
        Catch ex As OperationCanceledException
            Return 0
        Catch ex As EndOfStreamException
            Return 1
        Catch ex As InvalidDataException
            Return 1
        End Try
    End Function

    Private Shared Function ParseJob(message As JObject) As BrowserRecipeJob
        Dim job As New BrowserRecipeJob With {
            .JobId = If(message.Value(Of String)("jobId"), String.Empty).Trim(),
            .Url = If(message.Value(Of String)("url"), String.Empty).Trim(),
            .Html = If(message.Value(Of String)("html"), String.Empty)
        }

        Dim parsedJobId As Guid
        If Not Guid.TryParse(job.JobId, parsedJobId) Then
            job.ValidationError = "The extension supplied an invalid job identifier."
        ElseIf String.IsNullOrWhiteSpace(job.Url) Then
            job.ValidationError = "The current page does not have a usable URL."
        ElseIf String.IsNullOrWhiteSpace(job.Html) Then
            job.ValidationError = "The browser could not read this page's HTML."
        End If
        Return job
    End Function

    Private Shared Async Function ProcessRecipeJobAsync(
        job As BrowserRecipeJob,
        writer As NativeMessageWriter,
        cancellationToken As Threading.CancellationToken
    ) As Task
        Await writer.TryWriteAsync(
            CreateStatusMessage(job.JobId, job.Url, "in_progress", Nothing),
            cancellationToken
        )

        Dim finalStatus As JObject
        Try
            Dim meal = Await API.ScrapeNutritionalsFromHtml(job.Url, job.Html)
            Dim addResult = MealRepository.AddIfMissing(meal)
            Dim message = If(
                addResult.Added,
                "Added " & addResult.Meal.Name & ".",
                "Already in DietPlanner as " & addResult.Meal.Name & "."
            )
            finalStatus = New JObject(
                New JProperty("type", "status"),
                New JProperty("jobId", job.JobId),
                New JProperty("url", job.Url),
                New JProperty("status", "completed"),
                New JProperty("recipeName", addResult.Meal.Name),
                New JProperty("alreadyExists", Not addResult.Added),
                New JProperty("message", message)
            )
        Catch ex As Exception
            finalStatus = CreateStatusMessage(
                job.JobId,
                job.Url,
                "failed",
                GetSafeErrorMessage(ex)
            )
        End Try
        Await writer.TryWriteAsync(finalStatus, cancellationToken)
    End Function

    Private Shared Function CreateStatusMessage(
        jobId As String,
        url As String,
        status As String,
        errorMessage As String
    ) As JObject
        Return New JObject(
            New JProperty("type", "status"),
            New JProperty("jobId", If(jobId, String.Empty)),
            New JProperty("url", If(url, String.Empty)),
            New JProperty("status", status),
            New JProperty("error", If(errorMessage, String.Empty))
        )
    End Function

    Private Shared Function GetSafeErrorMessage(exception As Exception) As String
        Dim message = If(
            exception?.GetBaseException()?.Message,
            "DietPlanner could not add this recipe."
        ).Trim()
        If message.Length > 2_000 Then message = message.Substring(0, 2_000)
        Return message
    End Function

    Private Shared Async Function ReadMessageAsync(
        input As Stream,
        cancellationToken As Threading.CancellationToken
    ) As Task(Of String)
        Dim lengthBuffer(3) As Byte
        Dim firstRead = Await input.ReadAsync(
            lengthBuffer.AsMemory(0, lengthBuffer.Length),
            cancellationToken
        )
        If firstRead = 0 Then Return Nothing
        Await ReadRemainingAsync(
            input,
            lengthBuffer,
            firstRead,
            cancellationToken
        )

        Dim messageLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer)
        If messageLength <= 0 OrElse messageLength > MaximumMessageBytes Then
            Throw New InvalidDataException(
                "The browser-extension message was too large or invalid."
            )
        End If

        Dim messageBytes(messageLength - 1) As Byte
        Await ReadRemainingAsync(
            input,
            messageBytes,
            0,
            cancellationToken
        )
        Return StrictUtf8.GetString(messageBytes)
    End Function

    Private Shared Async Function ReadRemainingAsync(
        input As Stream,
        buffer As Byte(),
        offset As Integer,
        cancellationToken As Threading.CancellationToken
    ) As Task
        While offset < buffer.Length
            Dim bytesRead = Await input.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset),
                cancellationToken
            )
            If bytesRead = 0 Then
                Throw New EndOfStreamException(
                    "The browser closed an incomplete native message."
                )
            End If
            offset += bytesRead
        End While
    End Function

    Private NotInheritable Class BrowserRecipeJob
        Public Property JobId As String
        Public Property Url As String
        Public Property Html As String
        Public Property ValidationError As String = String.Empty
    End Class

    Private NotInheritable Class NativeMessageWriter
        Private ReadOnly _output As Stream
        Private ReadOnly _writeLock As New Threading.SemaphoreSlim(1, 1)

        Public Sub New(output As Stream)
            _output = output
        End Sub

        Public Async Function TryWriteAsync(
            message As JObject,
            cancellationToken As Threading.CancellationToken
        ) As Task
            Dim messageBytes = StrictUtf8.GetBytes(
                message.ToString(Formatting.None)
            )
            Dim lengthBuffer(3) As Byte
            BinaryPrimitives.WriteInt32LittleEndian(
                lengthBuffer,
                messageBytes.Length
            )

            Await _writeLock.WaitAsync(cancellationToken)
            Try
                Await _output.WriteAsync(lengthBuffer, cancellationToken)
                Await _output.WriteAsync(messageBytes, cancellationToken)
                Await _output.FlushAsync(cancellationToken)
            Catch ex As IOException
            Catch ex As ObjectDisposedException
            Finally
                _writeLock.Release()
            End Try
        End Function
    End Class
End Class
