using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatGPTExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string keyLocation = "/path/to/file.txt";
            string apiKey = File.ReadAllText(keyLocation);
            string aiVersion = "/path/to/file.txt";
            string prompt = File.ReadAllText(aiVersion);

            HttpClient httpClient = new HttpClient();
            string baseUrl = "https://api.openai.com/v1/completions";

            bool continueChat = true;

            StreamWriter logFile = new StreamWriter("log.txt", true);

            while (continueChat)
            {
                Console.WriteLine("Please enter your prompt or type 'exit' to quit:");
                prompt += "\n User:" + Console.ReadLine();

                if (prompt.ToLower() == "exit")
                {
                    continueChat = false;
                    break;
                }

                var requestData = new
                {
                    model = "text-davinci-003",
                    prompt = prompt,
                    temperature = 0.7,
                    max_tokens = 256,
                    top_p = 1,
                    frequency_penalty = 0,
                    presence_penalty = 0
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);

                var httpRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(baseUrl),
                    Headers =
                    {
                        { "Authorization", $"Bearer {apiKey}" }
                    },
                    Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                };

                var httpResponse = await httpClient.SendAsync(httpRequest);
                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

                JObject responseData = JsonConvert.DeserializeObject<JObject>(jsonResponse);
                string responseText = responseData["choices"][0]["text"].ToString();

                Console.WriteLine($"Starbright: {responseText}\n");

                // Write the user's response and AI's response to the log file
                logFile.WriteLine($"User: {prompt}");
                logFile.WriteLine($"Starbright: {responseText}\n");

                // Reset the prompt for the next iteration
                prompt = File.ReadAllText(aiVersion);
            }

            // Close the log file

            // figure out how to add this to the SB v0.2.5.txt instead of creating a new file

            logFile.Close();
        }
    }
}
