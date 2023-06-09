using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using RestSharp;

public class Program
{
  private static readonly HttpClient client = new HttpClient();

  public static async Task Main(string[] args)
  {
    Console.WriteLine("main 1"); // testing the first console log, it is working
    var response = await client.GetAsync("https://api.trello.com/1/cards/64823e0ae015b3e2013c7fe8/attachments?key=fe91479f8115fc056c49911580effd95&token=ATTA6cff2e9c9ca4b4b103bb93ed6180651b0cb9a138113d3e73da4fc9f2bf646d7c9B79F36B");

    if (response.IsSuccessStatusCode)
    {
      var jsonString = await response.Content.ReadAsStringAsync();

      var jsonDoc = JsonDocument.Parse(jsonString);
      var root = jsonDoc.RootElement;

      if (root.ValueKind == JsonValueKind.Array) // If the root is an array
      {
        foreach (var item in root.EnumerateArray())
        {
          if (item.TryGetProperty("name", out var nameProperty))
          {
            var name = nameProperty.GetString();

            Console.WriteLine(name); // the api from trello is connected and is returning the output of all commit messages from that specific card

            // Create a new RestClient and specify the OpenAI endpoint.
            var openAiClient = new RestClient("https://api.openai.com");

            // Create a new RestRequest and specify the path and method.
            var openAiRequest = new RestRequest("v1/engines/text-davinci-003/completions", Method.Post);


            // Add the OpenAI API key to the request headers.
            openAiRequest.AddHeader("Authorization", "Bearer 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");

            // Add the Content-Type header.
            openAiRequest.AddHeader("Content-Type", "application/json");

            // Define the data to send in the request body.
            openAiRequest.AddJsonBody(new
            {
              prompt = name,
              max_tokens = 60 // You can adjust this number based on how long you want the summaries to be.
            });

            // Execute the request and get the response.
            var openAiResponse = await openAiClient.ExecuteAsync(openAiRequest);
            Console.WriteLine(openAiResponse.StatusCode);


            if (!string.IsNullOrEmpty(openAiResponse.Content))
            {
              // Parse the response.
              var openAiResponseJson = JsonDocument.Parse(openAiResponse.Content);
              var openAiResponseRoot = openAiResponseJson.RootElement;

              // Extract the generated text from the response.
              if (openAiResponseRoot.TryGetProperty("choices", out var choicesProperty))
              {
                foreach (var choice in choicesProperty.EnumerateArray())
                {
                  if (choice.TryGetProperty("text", out var textProperty))
                  {
                    var summarizedText = textProperty.GetString();

                    // Print the summarized text.
                    Console.WriteLine(summarizedText);
                  }
                }
              }
            }
            else
            {
              // Handle the case where the response content is null.
              // You might want to log an error message or throw an exception.
            }
          }
        }
      }
    }
  }
}
