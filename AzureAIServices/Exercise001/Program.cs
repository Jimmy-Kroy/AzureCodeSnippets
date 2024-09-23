
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Azure;

namespace OpenAI_Chat
{      
    class Program      
    {        
        public static void Main()          
        {              
            try              
            {                  
                IConfiguration config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();                
                
                string oaiEndpoint = config["AzureOAIEndpoint"] ?? "";                
                string oaiKey = config["AzureOAIKey"] ?? "";                
                string oaiModelName = config["AzureOAIModelName"] ?? "";

                // Initialize the Azure OpenAI client
                OpenAIClient client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));

                var functions = new Dictionary<int, Action<OpenAIClient, string>> 
                {                    
                    { 1, ValidatePoC },                     
                    { 2, CompanyChatbot },                     
                    { 3, DeveloperTasks }                 
                };                
                
                while (true) 
                {                    
                    Console.WriteLine("1: Validate PoC\n" +
                        "2: Company chatbot\n" +
                        "3: Developer tasks\n" +
                        "\'quit\' to exit the program\n");                    
                    
                    string userInput = (Console.ReadLine() ?? "").Trim().ToLower();
                    
                    if (userInput == "quit") 
                    {
                        Console.WriteLine("Exiting program!");
                        break;                    
                    }                                        
                    
                    int inputKey = int.Parse(userInput);                    
                    if (functions.ContainsKey(inputKey)) 
                    {                        
                        functions[inputKey](client, oaiModelName);                    
                    }                    
                    else 
                    {                        
                        Console.WriteLine("Invalid input. Please enter number 1,2, or 3.");                    
                    }                
                }             
            }              
            catch (Exception ex)              
            {                  
                Console.WriteLine(ex);              
            }          
        }
        
        static void ValidatePoC(OpenAIClient client, string oaiModelName)
        {
            var promptInput = "What can a generative AI model do? Give me a short answer.";

            var options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestUserMessage(promptInput) 
                },
                DeploymentName = oaiModelName
            };

            //Response<ChatCompletions> response = await client.GetChatCompletionsAsync(options);
            Response<ChatCompletions> response = client.GetChatCompletions(options);
            ChatCompletions completions = response.Value;
            string completion = completions.Choices[0].Message.Content;
            Console.WriteLine("Response: " + completion + "\n");            
        }        
        
        static void CompanyChatbot(OpenAIClient client, string oaiModelName)
        {
            var systemInput = "You are an AI assistant that helps people find information. Each response must be in a casual tone and end with \"Hope that helps! Thanks for using Contoso, Ltd.\"";

            var options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestSystemMessage(systemInput),
                    new ChatRequestUserMessage("Where can I find the company phone number?"),
                    new ChatRequestAssistantMessage("You can find it on the footer of every page on our website. Hope that helps! Thanks for using Contoso, Ltd."),
                    new ChatRequestUserMessage("What is the best way to find if a company is hiring? Please answer in both English and Spanish."),
                },
                Temperature = 0.5f,
                MaxTokens = 1000,
                DeploymentName = oaiModelName
            };

            ChatCompletions response = client.GetChatCompletions(options).Value;
            Console.WriteLine("Response: " + response.Choices[0].Message.Content + "\n");
        }       

        static void DeveloperTasks(OpenAIClient client, string oaiModelName) 
        {
            var systemInput = "You are a programming assistant helping write code.";
            var promptInput01 = "Write a function in python that takes a character and a string as input, and returns how many times the character appears in the string.";
            var promptInput02 = System.IO.File.ReadAllText("./prompt-code.txt"); 
            var promptInput03 = System.IO.File.ReadAllText("./prompt-unit-tests.txt");

            var options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestSystemMessage(systemInput),
                    new ChatRequestUserMessage(promptInput03)
                },
                DeploymentName = oaiModelName
            };

            ChatCompletions response = client.GetChatCompletions(options).Value;
            string content = response.Choices[0].Message.Content;
            //System.IO.File.WriteAllText("./response-code.txt", content);
            //System.IO.File.WriteAllText("./response-unit-tests.txt", content);
            Console.WriteLine("Response: " + content + "\n");
        }
    }
}