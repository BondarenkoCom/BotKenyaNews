using BotKenyaNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BotKenyaNews.Helpers
{
    public class GPTDriver
    {
        public async Task<string> RewritePost(string contentFromNewsWebSite)
        {
            string apiKey = JsonReader.GetValues().openApiKey;
            string endpoint = "https://api.openai.com/v1/chat/completions";
            //string typeContent = string.Empty;

            List<Message> messages = new List<Message>();
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var greetingMessage = new Message()
            { Role = "user",
              Content = "Transform the following news article into a concise, clear, and engaging message, suitable for a Telegram post:" };
            messages.Add(greetingMessage);

            var message = new Message() { Role = "user", Content = contentFromNewsWebSite };
            messages.Add(message);

            var requestData = new Request()
            {
                ModelId = "gpt-3.5-turbo",
                Messages = messages
            };

            using var response = await httpClient.PostAsJsonAsync(endpoint, requestData);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
                return null;
            }

            ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>();
            var choices = responseData?.Choices ?? new List<Choice>();
            if (choices.Count == 0)
            {
                Console.WriteLine("No choices were returned by the API");
                return null;
            }

            var choice = choices[0];
            var responseMessage = choice.Message;
            messages.Add(responseMessage);
            var responseText = responseMessage.Content.Trim();

            string formattedResponse =
                $"\n\n{responseText}\n\n";

            Console.WriteLine($"ChatGPT: {responseText}");
            return formattedResponse;

        }
    }
}
