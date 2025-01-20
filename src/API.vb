Imports System.Net.Http
Imports System.Text
Imports System.Text.Json.Nodes
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module API
    Dim BaseUrl As String = "https://gs.makeme.it/v2"

    Public Async Function ScrapeNutritionals(url As String) As Task(Of Meal)
        'Post Request
        Dim body As New Dictionary(Of String, Object) From {
            {"url", url},
            {"prompts", New Object() {
                New With {
                    .role = "system",
                    .content = "Extract all the nutritional ingredients, calories and prep/cook time from the following page, and format them as such {""Nutritionals"": {""Protein"": ""10"", ...etc}, ""Times"": {""Cook"": 31, ""Prep"": 10}, ""Calories"": 102, ""Name"": ""Meal Name Here""}}\ndo not add ```json``` or any other code block, just the dictionary. value must always be numeric (i.e protein: 3 NOT protein 3g). ENSURE THE VALUES ARE ACCURATE JUST LIKE ON THE WEBSITE. FLOATS ARE OK"
                }
            }}
        }

        Dim jsonBody As String = JsonConvert.SerializeObject(body)

        Using client As New HttpClient()
            client.DefaultRequestHeaders.Add("X-RapidAPI-Proxy-Secret", ApiKey)
            Dim content As New StringContent(jsonBody, Encoding.UTF8, "application/json")
            Dim response As HttpResponseMessage = Await client.PostAsync(BaseUrl & "/scrape_with_ai", content)

            If response.IsSuccessStatusCode Then
                Dim result As String = Await response.Content.ReadAsStringAsync()
                ' Process the result here
                Dim parsed = JObject.Parse(result)
                Return New Meal(
                    parsed("Name").ToString(),
                    parsed("Calories").ToString(),
                    parsed("Nutritionals").ToObject(Of Dictionary(Of String, Double)),
                    url,
                    parsed("Times")("Prep").ToString(),
                    parsed("Times")("Cook").ToString()
                )
            Else
                ' Handle error
                Debug.WriteLine("Error: " & response.StatusCode)
                Return Nothing
            End If
        End Using
    End Function
End Module
