using Microsoft.Extensions.Configuration;
//dotnet add package Microsoft.SemanticKernel --version 1.2.0
using Microsoft.SemanticKernel;
//dotnet add package Microsoft.SemanticKernel.Plugins.Core --version 1.2.0-alpha
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelApp.Plugins;
using Azure;
using Humanizer.Bytes;
using Humanizer;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Buffers.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;
using System.Collections.Generic;
using Azure.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelApp.Plugins.TodoList;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SemanticKernelApp.Plugins.ConvertCurrency;
using System.Text;

/* Built-in plugins 
ConversationSummaryPlugin - Summarizes conversation
FileIOPlugin - Reads and writes to the filesystem
HttpPlugin - Makes requests to HTTP endpoints
MathPlugin - Performs mathematical operations
TextPlugin - Performs text manipulation
TimePlugin - Gets time and date information
WaitPlugin - Pauses execution for a specified amount of time */

#pragma warning disable SKEXP0050


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

                Console.WriteLine("Semantic Kernel app started.\n");

                var builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(oaiModelName, oaiEndpoint, oaiKey);
                var kernel = builder.Build();

                await Excuses(kernel);
                //await Jokes(kernel);
                //await AITravelAgentV2(kernel);
                //await AITravelAgent(kernel);
                //await GetNeededRecipeIngredients(kernel);
                //await AddSong(kernel);
                //await GetConcertRecommendationV2(kernel);
                // Result: Based on your recently played music, I recommend you listen to "Kids" by MGMT.
                // It has a similar vibe to the songs you mentioned and I think you'll enjoy it!
                //await GetConcertRecommendation(kernel);
                //await SuggestNextSong(kernel);
                //await suggestRecipesV2(kernel);
                //await suggestRecipes(kernel);
                //await CallNativeFunctionsV2(kernel);
                //await CallNativeFunctions(kernel);
                //await TellJoke(kernel);
                //await RecommendTrip(kernel);
                //await RecommendsChordsV2(kernel);
                //await RecommendsChords(kernel);
                //string request = @"I have a vacation from June 1 to July 22. I want to go to Greece. I live in Chicago.";
                //await ExtractTravelerData(kernel, request);
                //await UseChatHistory(kernel);
                //string history = "I'm traveling with my kids and one of them has a gluten allergy.";
                //await TravelAssistant(kernel, history, "Portugees");
                //await CreatelistHelpfulPhrasesV2(kernel, history, "Spanish");
                //string history = "I'm traveling with my kids and one of them has a peanut allergy.";
                //await CreatelistHelpfulPhrasesV2(kernel, history, "French");
                //await CreatelistHelpfulPhrases(kernel, "French");
                //await CreatelistHelpfulPhrases(kernel, "Spanish");
                //await GetRecommendations(kernel);
                //await TextProcessing(kernel);
                //await GetTime(kernel);
                //await GetBreakfastFoods(kernel);
                //await GetConcertRecommendation(kernel);

                Console.WriteLine("\nSemantic Kernel app finished.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task Excuses(Kernel kernel)
        {
            var plugins = kernel.CreatePluginFromPromptDirectory("Prompts/FunPlugin");

            Console.WriteLine("What kind of event?");
            string input = Console.ReadLine();

            var result = await kernel.InvokeAsync<string>(
                plugins["Excuses"],
                new KernelArguments()
                {
                    { "input", input },
                });

            Console.WriteLine(result);
        }

        /* Tell me a joke about a criminal that became president.
        Why did the criminal become president? Because he wanted to steal the hearts of the nation!

        Tell me a joke about a president that became a criminal.
        Why did the president become a criminal? Because he wanted to steal the hearts of the nation... and their tax dollars too!

        Tell me a joke about a politician who turned xenophobic, even though his ancestors were immigrants.
        Why did the politician, who had immigrant ancestors, turn xenophobic? Because he wanted to build a wall to keep his own family out!


        Tell me a joke about a xenophobic politician who painted his black hair blonde, just to appear more appealing to people.
        Why did the xenophobic politician paint his black hair blonde? Because he thought it would make him look more "golden" to his supporters! */
        public static async Task Jokes(Kernel kernel)
        {
            var plugins = kernel.CreatePluginFromPromptDirectory("Prompts/FunPlugin");

            StringBuilder chatHistory = new StringBuilder();
            string input;

            //Jokes style
            string style = FileReader.ReadLinesFromFile("./data/jokes.txt", 1, 100);

            Console.WriteLine("What kind of joke do you want to hear?");
            input = Console.ReadLine();

            while (!string.IsNullOrWhiteSpace(input))
            {
                var result = await kernel.InvokeAsync<string>(
                    plugins["Joke"],
                    new KernelArguments()
                    {
                        { "style", style },
                        { "input", input },
                        { "history", chatHistory }
                    });

                chatHistory.AppendLine("User:" + input);
                chatHistory.AppendLine("Assistant:" + result);

                Console.WriteLine(result);

                Console.WriteLine("What kind of joke do you want to hear?");
                input = Console.ReadLine();
            }
        }

        //Use the conversation history to provide context to the large language model (LLM).
        public static async Task AITravelAgentV2(Kernel kernel)
        {
            string input;
            StringBuilder chatHistory = new();
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            kernel.ImportPluginFromType<ConversationSummaryPlugin>();
            kernel.ImportPluginFromType<CurrencyConverter>();
            var prompts = kernel.ImportPluginFromPromptDirectory("Prompts");

            do
            {
                Console.WriteLine("What would you like to do?");
                input = Console.ReadLine();

                var intent = await kernel.InvokeAsync<string>(
                      prompts["GetIntent"],
                      new()
                      {
                        { "input", input }
                      }
                  );

                Console.WriteLine($"intent: {intent}");

                /* In this code, you use the AutoInvokeKernelFunctions setting to automatically call functions 
                 * and prompts that are referenced in your kernel. If the user's intent is to convert currency, 
                 * the CurrencyConverter plugin performs its task. */
                switch (intent)
                {
                    case "ConvertCurrency":
                        var currencyText = await kernel.InvokeAsync<string>(
                            prompts["GetTargetCurrencies"],
                            new() { { "input", input } }
                        );

                        var currencyInfo = currencyText!.Split("|");
                        var result = await kernel.InvokeAsync(
                            "CurrencyConverter", "ConvertAmount",
                            new()
                            {
                                {"targetCurrencyCode", currencyInfo[0]},
                                {"baseCurrencyCode", currencyInfo[1]},
                                {"amount", currencyInfo[2]},
                            }
                        );
                        Console.WriteLine(result);
                        break;
                    case "SuggestDestinations":
                        chatHistory.AppendLine("User:" + input);
                        var recommendations = await kernel.InvokePromptAsync(input!);
                        Console.WriteLine(recommendations);
                        break;
                    case "SuggestActivities":
                        var chatSummary = await kernel.InvokeAsync(
                            "ConversationSummaryPlugin",
                            "SummarizeConversation",
                            new() 
                            { 
                                { "input", chatHistory.ToString() } 
                            });

                        var activities = await kernel.InvokePromptAsync(
                            input!,
                            new() 
                            {
                                {"input", input},
                                {"history", chatSummary},
                                {"ToolCallBehavior", ToolCallBehavior.AutoInvokeKernelFunctions}
                            });

                        chatHistory.AppendLine("User:" + input);
                        chatHistory.AppendLine("Assistant:" + activities.ToString());

                        Console.WriteLine(activities);
                        break;
                    case "HelpfulPhrases":
                    case "Translate":
                        var autoInvokeResult = await kernel.InvokePromptAsync(input!, new(settings));
                        Console.WriteLine(autoInvokeResult);
                        break;
                    default:
                        //Console.WriteLine("Other intent detected");
                        Console.WriteLine("Sure, I can help with that.");
                        var otherIntentResult = await kernel.InvokePromptAsync(input!);
                        Console.WriteLine(otherIntentResult);
                        break;
                }

            }
            while (!string.IsNullOrWhiteSpace(input));
        }

        public static async Task AITravelAgent(Kernel kernel)
        {
            kernel.ImportPluginFromType<CurrencyConverter>();
            var prompts = kernel.ImportPluginFromPromptDirectory("Prompts");

            //var result = await kernel.InvokeAsync(
            //    "CurrencyConverter", "ConvertAmount",
            //    new KernelArguments() 
            //    {
            //        {"targetCurrencyCode", "USD"},
            //        {"amount", "52000"},
            //        {"baseCurrencyCode", "BMD"}
            //    });

            //Console.WriteLine(result);

            //var prompts = kernel.ImportPluginFromPromptDirectory("Prompts");

            //var result = await kernel.InvokeAsync(prompts["GetTargetCurrencies"],
            //    new KernelArguments() 
            //    {
            //        {"input", "How many Australian Dollars is 140,000 Korean Won worth?"}
            //    }
            //);

            //Console.WriteLine(result);

            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            string input = string.Empty;

            do
            {

                Console.WriteLine("What would you like to do?");
                input = Console.ReadLine();

                var intent = await kernel.InvokeAsync<string>(
                    prompts["GetIntent"],
                    new() 
                    { 
                        { "input", input } 
                    }
                );

                Console.WriteLine($"intent: {intent}");

                /* In this code, you use the AutoInvokeKernelFunctions setting to automatically call functions 
                 * and prompts that are referenced in your kernel. If the user's intent is to convert currency, 
                 * the CurrencyConverter plugin performs its task. */
                switch (intent)
                {
                    case "ConvertCurrency":
                        var currencyText = await kernel.InvokeAsync<string>(
                            prompts["GetTargetCurrencies"],
                            new() { { "input", input } }
                        );

                        var currencyInfo = currencyText!.Split("|");
                        var result = await kernel.InvokeAsync(
                            "CurrencyConverter", "ConvertAmount",
                            new() 
                            {
                                {"targetCurrencyCode", currencyInfo[0]},
                                {"baseCurrencyCode", currencyInfo[1]},
                                {"amount", currencyInfo[2]},
                            }
                        );
                        Console.WriteLine(result);
                        break;
                    case "SuggestDestinations":
                    case "SuggestActivities":
                    case "HelpfulPhrases":
                    case "Translate":
                        var autoInvokeResult = await kernel.InvokePromptAsync(input!, new(settings));
                        Console.WriteLine(autoInvokeResult);
                        break;
                    default:
                        //Console.WriteLine("Other intent detected");
                        Console.WriteLine("Sure, I can help with that.");
                        var otherIntentResult = await kernel.InvokePromptAsync(input!, new(settings));
                        Console.WriteLine(otherIntentResult);
                        break;
                }
            }
            while (!string.IsNullOrWhiteSpace(input));
        }

        public static async Task GetNeededRecipeIngredients(Kernel kernel)
        {
            kernel.ImportPluginFromType<IngredientsPlugin>();
            kernel.ImportPluginFromPromptDirectory("Prompts/IngredientPrompts");

            //var result = await kernel.InvokeAsync<string>(
            //    "IngredientsPlugin", "GetIngredients");
            //Console.WriteLine(result + "\n");

            //var recipe = "Roasted Asparagus";
            //var result = await kernel.InvokeAsync<string>(
            //    "IngredientsPlugin", "GetRecipe",
            //    new KernelArguments()
            //    {
            //        { "recipe",  recipe }
            //    });

            // Set the ToolCallBehavior property
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            string prompt = @"What ingredients am I missing from my current list of ingredients 
                to make a recipe for Curried Lentils and Rice?";

            // Use the settings to automatically invoke plugins based on the prompt
            var result = await kernel.InvokePromptAsync(prompt, new(settings));

            Console.WriteLine(result);
            //Roasted Asparagus
            //- None. You have all the necessary ingredients.

            //Curried Lentils and Rice
            //Based on the recipe for Curried Lentils and Rice, the ingredients you are missing from your current list are:
            //-1 quart of beef broth
            //- 1 / 2 cup of basmati rice
        }

        public static async Task SuggestNextSong(Kernel kernel)
        {
            kernel.ImportPluginFromType<MusicLibraryPlugin>();

            string prompt = @"This is a list of music available to the user:
                {{MusicLibraryPlugin.GetMusicLibrary}} 

                This is a list of music the user has recently played:
                {{MusicLibraryPlugin.GetRecentPlays}}

                Based on their recently played music, suggest a song from
                the list to play next";

            var result = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine(result);
        }

        /* In this example, the prompt calls ConversationSummaryPlugin.SummarizeConversation on the provided $history input. 
         * The function takes the user's background information and summarizes it, and the result is used to retrieve the 
         * list of relevant recipes. The ConversationSummaryPlugin plugin must be added to the kernel builder for the prompt 
         * to work correctly. */
        public static async Task suggestRecipesV2(Kernel kernel)
        {
            kernel.ImportPluginFromType<ConversationSummaryPlugin>();

            string history = @"In the heart of my bustling kitchen, I have embraced the challenge 
                of satisfying my family's diverse taste buds and navigating their unique tastes. 
                With a mix of picky eaters and allergies, my culinary journey revolves around 
                exploring a plethora of vegetarian recipes.

                One of my kids is a picky eater with an aversion to anything green, while another 
                has a peanut allergy that adds an extra layer of complexity to meal planning. 
                Armed with creativity and a passion for wholesome cooking, I've embarked on a 
                flavorful adventure, discovering plant-based dishes that not only please the 
                picky palates but are also heathy and delicious.";

            string prompt = @"User information: 
                {{ConversationSummaryPlugin.SummarizeConversation $history}}

                Given this user's background information, provide a list of relevant recipes.";

            var result = await kernel.InvokePromptAsync(prompt, 
                new KernelArguments() 
                {
                    { "history", history }
                });

            Console.WriteLine(result);
        }

        public static async Task suggestRecipes(Kernel kernel)
        {
            string history = @"In the heart of my bustling kitchen, I have embraced the challenge 
                of satisfying my family's diverse taste buds and navigating their unique tastes. 
                With a mix of picky eaters and allergies, my culinary journey revolves around 
                exploring a plethora of vegetarian recipes.

                One of my kids is a picky eater with an aversion to anything green, while another 
                has a peanut allergy that adds an extra layer of complexity to meal planning. 
                Armed with creativity and a passion for wholesome cooking, I've embarked on a 
                flavorful adventure, discovering plant-based dishes that not only please the 
                picky palates but are also heathy and delicious.";

            string prompt = @"This is some information about the user's background: {{$history}} 
                Given this user's background, provide a list of relevant recipes.";

            var result = await kernel.InvokePromptAsync(prompt, 
                new KernelArguments
                {
                    { "history", history }
                });

            Console.WriteLine(result);
        }

        public static async Task CallNativeFunctionsV2(Kernel kernel)
        {
            kernel.ImportPluginFromType<MusicLibraryPlugin>();

            var result = await kernel.InvokeAsync<string>(
                "MusicLibraryPlugin", "GetRecentlyPlayedSongs");
            Console.WriteLine(result + "\n");


            result = await kernel.InvokeAsync<string>(
                "MusicLibraryPlugin", "AddToRecentlyPlayed",
                new KernelArguments()
                {
                    ["artist"] = "Tiara",
                    ["song"] = "Danse",
                    ["genre"] = "French pop, electropop, pop"
                });

            Console.WriteLine(result + "\n");

            result = await kernel.InvokeAsync<string>(
                "MusicLibraryPlugin", "GetRecentlyPlayedSongs");
            Console.WriteLine(result + "\n");
        }

        public static async Task CallNativeFunctions(Kernel kernel)
        {
            kernel.ImportPluginFromType<TodoListPlugin>();

            var result = await kernel.InvokeAsync<string>(
                "TodoListPlugin",
                "GetTasks");
            Console.WriteLine(result + "\n");

            result = await kernel.InvokeAsync<string>(
                "TodoListPlugin",
                "CompleteTask",
              new KernelArguments 
              { 
                  { "task", "Buy groceries" } 
              });
            Console.WriteLine(result + "\n");

            result = await kernel.InvokeAsync<string>(
                "TodoListPlugin", 
                "GetTasks");
            Console.WriteLine(result + "\n");
        }

        public static async Task TellJoke(Kernel kernel)
        {
            //Sense of humor
            string history = FileReader.ReadLinesFromFile("./data/jokes.txt", 1, 100);

            history += "Why don't scientists trust atoms? Because they make up everything!\n";
            history += "Why don't skeletons fight each other? They don't have the guts!\n";
            history += "Why don't scientists trust atoms anymore? Because they make up everything, but they never share their electrons!\n";

            Console.WriteLine(history);

            var prompts = kernel.ImportPluginFromPromptDirectory("Prompts");

            var result = await kernel.InvokeAsync<string>(prompts["TellJoke"],
                new KernelArguments() { { "history", history } });

            Console.WriteLine(result);
        }

        public static async Task RecommendTrip(Kernel kernel)
        {
            //List<ChatHistory> history = new List<ChatHistory>();
            //You also use a ChatHistory object to store the user's conversation.
            ChatHistory history = []; 

            var prompts = kernel.ImportPluginFromPromptDirectory("Prompts/TravelPlugins");

            string input = @"I'm planning an anniversary trip with my spouse. We like hiking, mountains, and beaches. Our travel budget is $15000";

            var result = await kernel.InvokeAsync<string>(prompts["SuggestDestinations"],
                new KernelArguments() { { "input", input } });

            Console.WriteLine(result);
            history.AddUserMessage(input);
            history.AddAssistantMessage(result);

            string destination = "Cambodia";

            result = await kernel.InvokeAsync<string>(prompts["SuggestActivities"],
                new KernelArguments() 
                {
                    { "history", history },
                    { "destination", destination },
                }
            );

            Console.WriteLine(result);
        }

        /* In this example, CreatePluginFromPromptDirectory returns a KernelPlugin object. 
         * This object represents a collection of functions. CreatePluginFromPromptDirectory 
         * accepts the path of your designated plugin directory, and each subfolder's name is used as a function name.
         *
         * For example, if you nested 'SuggestChords' inside a folder called 'ChordProgressions,' 
         * you would use the prompt directory 'Prompts/ChordProgressions' and the function name would stay the same. 
         * Alternatively, you could use the 'Prompt' directory and reference 'ChordProgressions/SuggestChords' 
         * as the function name.         */
        public static async Task RecommendsChordsV2(Kernel kernel)
        {
            var plugins = kernel.CreatePluginFromPromptDirectory("Prompts/ChordProgressions");
            string input = "G, C";

            var result = await kernel.InvokeAsync(
                plugins["SuggestChords"],
                new() { { "startingChords", input } });

            Console.WriteLine(result);
        }

        /* In this example, the temperature is a parameter that controls how much to randomize the generated text. 
         * The values must be between 0 and 2. A lower temperature results in more focused and precise output, 
         * and a higher temperature results in more diverse and creative output. */
        public static async Task RecommendsChords(Kernel kernel)
        {
            var plugins = kernel.CreatePluginFromPromptDirectory("Prompts");
            string input = "G, C";

            var result = await kernel.InvokeAsync(
                plugins["SuggestChords"],
                new() { { "startingChords", input } });

            Console.WriteLine(result);
        }

        /* Tips for crafting prompts
        
        1) Specific Inputs Yield Specific Outputs: LLMs respond based on the input they receive.
        Crafting clear and specific prompts is crucial to get the desired output.

        2) Experimentation is Key: You may need to iterate and experiment with different prompts 
        to understand how the model interprets and generates responses.
        Small tweaks can lead to significant changes in outcomes.
        
        3) Context Matters: LLMs consider the context provided in the prompt. You should ensure 
        that the context is well-defined and relevant to obtain accurate and coherent responses.
        
        4) Handle Ambiguity: Bear in mind that LLMs may struggle with ambiguous queries. Provide 
        context or structure to avoid vague or unexpected results.

        5) Length of Prompts: While LLMs can process both short and long prompts, you should consider 
        the trade-off between brevity and clarity.Experimenting with prompt length can help you find the optimal balance. */
        public static async Task GetRecommendations(Kernel kernel)
        {
            string background = @"In the heart of my bustling kitchen, I have embraced 
                the challenge of satisfying my family's diverse taste buds and 
                navigating their unique tastes. With a mix of picky eaters and 
                allergies, my culinary journey revolves around exploring a plethora 
                of vegetarian recipes.

                One of my kids is a picky eater with an aversion to anything green, 
                while another has a peanut allergy that adds an extra layer of complexity 
                to meal planning. Armed with creativity and a passion for wholesome 
                cooking, I've embarked on a flavorful adventure, discovering plant-based 
                dishes that not only please the picky palates but are also heathy and 
                delicious.";

            string prompt = @"This is some information about the user's background: 
                {{$history}}

                Given this user's background, provide a list of relevant recipes.";

            var result = await kernel.InvokePromptAsync(prompt,
                new KernelArguments() 
                { 
                    { "history", background } 
                });

            Console.WriteLine(result);
        }

        public static async Task ExtractTravelerData(Kernel kernel, string request)
        {
            string prompt = @$"
                <message role=""system"">Instructions: Identify the from and to destinations 
                and dates from the user's request</message>

                <message role=""user"">Can you give me a list of flights from Seattle to Tokyo? 
                I want to travel from March 11 to March 18.</message>

                <message role=""assistant"">Seattle|Tokyo|11-03-2024|18-03-2024</message>

                <message role=""user"">{request}</message>";

            Console.WriteLine(prompt + "\n\n");
            var result = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine(result);
        }

        public static async Task TravelAssistant(Kernel kernel, string background, string language)
        {
            // Assign a persona to the prompt
            string prompt = @$"
                You are a travel assistant. You are helpful, creative, and very friendly. 
                Consider the traveler's background:
                {background}

                Create a list of helpful phrases and words in {language} a traveler would find useful.

                Group phrases by category. Include common direction words. 
                Display the phrases in the following format: 
                Hello - Ciao [chow]

                Begin with: 'Here are some phrases in {language} you may find helpful:' 
                and end with: 'I hope this helps you on your trip!'";

            Console.WriteLine(prompt +"\n\n");
            var result = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine(result);
        }

        public static async Task UseChatHistory(Kernel kernel)
        {
            //string input = "I'm a vegan in search of new recipes. I love spicy food! Can you give me a list of breakfast recipes that are vegan friendly?";
            string input = "I'm planning an anniversary trip with my spouse. We like hiking, mountains, and beaches. Our travel budget is $15000";

            string prompt = @$"
                The following is a conversation with an AI travel assistant. 
                The assistant is helpful, creative, and very friendly.

                <message role=""user"">Can you give me some travel destination suggestions?</message>

                <message role=""assistant"">Of course! Do you have a budget or any specific 
                activities in mind?</message>

                <message role=""user"">{input}</message>";

            Console.WriteLine(prompt + "\n\n");

            var result = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine(result);
        }

        public static async Task CreatelistHelpfulPhrasesV2(Kernel kernel, string background, string language)
        {
            string prompt = "Consider the traveler's background: {{$history}} " +
                "Create a list of helpful phrases and words in {{$language}} a traveler would find useful." +
                "Group phrases by category. Include common direction words. " +
                "Display the phrases in the following format: Hello - Ciao [chow]";

            var result = await kernel.InvokePromptAsync(prompt,
                new KernelArguments()
                {
                    { "language", language },
                    { "history", background },
                });

            Console.WriteLine(result);
        }

        public static async Task CreatelistHelpfulPhrases(Kernel kernel, string language)
        {
            string prompt = $"Create a list of helpful phrases and words in {language} a traveler would find useful.";

            var result = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine(result);

            /* Same call as above using different syntax. */
            string prompt2 = "Create a list of helpful phrases and words in {{$language}} a traveler would find useful.";

            var result2 = await kernel.InvokePromptAsync(prompt2,
                new KernelArguments()
                {
                    { "language", language }
                });

            Console.WriteLine(result2);
        }

        public static async Task TextProcessing(Kernel kernel)
        {
            kernel.Plugins.AddFromType<ConversationSummaryPlugin>();

            string input = @"I'm a vegan in search of new recipes. I love spicy food! Can you give me a list of breakfast recipes that are vegan friendly?";

            var result = await kernel.InvokeAsync(
                "ConversationSummaryPlugin",
                "GetConversationTopics",
                new() { { "input", input } });

            Console.WriteLine(result);
        }

        public static async Task GetTime(Kernel kernel)
        {
            kernel.ImportPluginFromType<TimePlugin>();
            var currentDay = await kernel.InvokeAsync("TimePlugin", "DayOfWeek");
            Console.WriteLine(currentDay);

            var currentTime = await kernel.InvokeAsync("TimePlugin", "Now");
            Console.WriteLine(currentTime);
        }

        public static async Task GetBreakfastFoods(Kernel kernel)
        {
            var result = await kernel.InvokePromptAsync("Give me a list of breakfast foods with eggs and cheese");
            Console.WriteLine(result);
        }

        public static async Task AddSong(Kernel kernel)
        {
            kernel.ImportPluginFromType<MusicLibraryPlugin>();

            //The AutoInvokeKernelFunctions setting allows the semantic kernel to automatically call
            //functions and prompts that are added to your kernel. 
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            string prompt = @"Add this song to the recently played songs list:  title: 'Touch', artist: 'My Cat's Eye', genre: 'Pop'";

            var result = await kernel.InvokePromptAsync(prompt, new(settings));

            Console.WriteLine(result);
        }

        public static async Task GetConcertRecommendationV2(Kernel kernel)
        {
            kernel.ImportPluginFromType<MusicLibraryPlugin>();

            //The AutoInvokeKernelFunctions setting allows the semantic kernel to automatically call
            //functions and prompts that are added to your kernel. 
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            //Inline prompt 
            KernelFunction songSuggesterFunction = kernel.CreateFunctionFromPrompt(
                promptTemplate: @"Based on the user's recently played music: {{$recentlyPlayedSongs}}
                                recommend a song to the user from the music library: {{$musicLibrary}}",
                functionName: "SuggestSong",
                description: "Recommend a song from the library");

            kernel.Plugins.AddFromFunctions("SuggestSong", [songSuggesterFunction]);

            string prompt = "Can you recommend a song from the music library?";

            var result = await kernel.InvokePromptAsync(prompt, new(settings));

            Console.WriteLine(result);
        }

        /* This module introduces a setting to automatically invoke functions using the Semantic Kernel SDK. 
        Learn how to use the Semantic Kernel to automatically invoke functions to complete a user's request. 
        The AutoInvokeKernelFunctions setting allows the semantic kernel to automatically call functions and 
        prompts that are added to your kernel. This tool can empower you to create dynamic, robust applications 
        using less code. */
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

