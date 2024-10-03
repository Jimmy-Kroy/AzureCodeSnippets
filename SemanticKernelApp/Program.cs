using Microsoft.Extensions.Configuration;
//dotnet add package Microsoft.SemanticKernel --version 1.2.0
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelApp.Plugins;

namespace SemanticKernelApp
{
    public class Program
    {
        public static async Task Main()
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();

                string oaiEndpoint = config["AzureOAIEndpoint"] ?? "";
                string oaiKey = config["AzureOAIKey"] ?? "";
                string oaiModelName = config["AzureOAIModelName"] ?? "";
                string oaiModelId = config["AzureOAIModelId"] ?? ""; // optional

                Console.WriteLine("Semantic Kernel app started.");

                var builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(oaiModelName, oaiEndpoint, oaiKey);
                var kernel = builder.Build();

                //await GetBreakfastFoods(kernel);
                await GetConcertRecommendation(kernel);

                Console.WriteLine("Semantic Kernel app finished.");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task GetBreakfastFoods(Kernel kernel)
        {
            var result = await kernel.InvokePromptAsync("Give me a list of breakfast foods with eggs and cheese");
            Console.WriteLine(result);
        }

        public static async Task GetConcertRecommendation(Kernel kernel)
        {
            kernel.ImportPluginFromType<MusicLibraryPlugin>();
            kernel.ImportPluginFromType<MusicConcertPlugin>();
            kernel.ImportPluginFromPromptDirectory("Prompts/SuggestConcert");

            //The AutoInvokeKernelFunctions setting allows the semantic kernel to automatically call
            //functions and prompts that are added to your kernel. 
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            string prompt = "I live in Portland OR USA. Based on my recently played songs and a list of upcoming concerts, " +
                "which concert do you recommend?";

            var result = await kernel.InvokePromptAsync(prompt, new KernelArguments(settings));

            Console.WriteLine(result);
            /*Based on your recently played songs and your location in Portland, OR, I would recommend the upcoming concert by Shing02. Shing02 has concerts scheduled in both Portland, OR, USA on 9/9/2023 and Seattle, WA, USA on 9/15/2023. Since the Portland concert is closer to your location, you may find it more convenient to attend that one.
            Based on your recently played songs and the list of upcoming concerts, I recommend attending the concert of Clairo. Clairo is an artist whose genre aligns with your recently played songs, as she is classified under alternative and indie music. Clairo has upcoming concerts in New York City, Chicago, Los Angeles, and Barcelona. The concert in New York City is on February 14, 2024, which may be a convenient option for you.*/
        }
    }
}

