using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ChatGPTExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //store the the api key in a string from the directory location
            string keyLocation = "D:\\Coding\\Projects\\Starbright\\C#\\chatbot-with-openai\\chatbot-with-openai\\resources\\Starbright\\APIKey.txt";
            string apiKey = File.ReadAllText(keyLocation);
            //Bring prompt from the directory location and store it in a string
            string aiVersion = "D:\\Coding\\Projects\\Starbright\\C#\\chatbot-with-openai\\chatbot-with-openai\\resources\\Starbright\\SB v0.2.5.txt";
            string prompt = File.ReadAllText(aiVersion);
            //get api instructions from the directory location and store it in a string
            string instructionSet = "D:\\Coding\\Projects\\Starbright\\C#\\chatbot-with-openai\\chatbot-with-openai\\resources\\Starbright\\instructions.txt";
            string instructions = File.ReadAllText(instructionSet);
            //Create a new http client
            HttpClient httpClient = new HttpClient();

            //put api site in string
            string baseUrl = "https://api.openai.com/v1/completions";

            //Create a bool to continue the chat
            bool continueChat = true;

            //Create a log file
            StreamWriter logFile = new StreamWriter("D:\\Coding\\Projects\\Starbright\\C#\\chatbot-with-openai\\chatbot-with-openai\\resources\\Starbright\\log.txt", true);

            // The maximum number of interactions you want to store
            int maxInteractionCount = 1000; 

            List<string> interactionHistory = new List<string>();

            //chat loop
            while (continueChat)
            {
                //Get user input to send to the API
                Console.WriteLine("Please enter your prompt or type 'exit' to quit:");

                string userInput = Console.ReadLine();

                //Exit the chat if the user types 'exit'
                if (userInput.ToLower() == "exit")
                {
                    continueChat = false;
                    break;
                }

                //append the user input to the interactionHistory
                interactionHistory.Add("User: " + userInput);

                if (interactionHistory.Count > maxInteractionCount * 2)
                {
                    interactionHistory.RemoveAt(0);
                    interactionHistory.RemoveAt(0);
                }

                prompt = File.ReadAllText(aiVersion) + "\n" + string.Join("\n", interactionHistory);

                // Load instructions from the instruction set file
                JObject instructionData = JObject.Parse(instructions);

                //Send the request to the API
                var requestData = new
                {
                    model = instructionData["model"],
                    prompt = prompt,
                    temperature = instructionData["temperature"],
                    max_tokens = instructionData["max_tokens"],
                    top_p = instructionData["top_p"],
                    best_of = instructionData["best_of"],
                    frequency_penalty = instructionData["frequency_penalty"],
                    presence_penalty = instructionData["presence_penalty"]
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
                string responseText = responseData["choices"][0]["text"].ToString().Trim();

                Console.WriteLine($"Starbright: {responseText}\n");

                interactionHistory.Add("Starbright: " + responseText);

                // Write the user's response and AI's response to the log file
                logFile.WriteLine($"User: {userInput}");
                logFile.WriteLine($"Starbright: {responseText}\n");
            }

            // Close the log file
            logFile.Close();
        }
    }
}
