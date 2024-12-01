using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace SemanticKernelApp02
{
    public class Program
    {
        public static async Task Main()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string oaiEndpoint = config["AzureOAIEndpoint"] ?? "";
            string oaiKey = config["AzureOAIKey"] ?? "";
            string oaiModelName = config["AzureOAIModelName"] ?? "";
            string oaiModelId = config["AzureOAIModelId"] ?? ""; // optional

            Console.WriteLine("Semantic Kernel app started.\n");

            //TODO 1.2 - Create a Kernel builder by using the CreateBuilder method of the Kernel object
            IKernelBuilder builder = Kernel.CreateBuilder();

            //TODO 1.3 - Configure access to gpt35 using the AddAzureOpenAIChatCompletion method of the builder's services objectbuilder.
            //builder.AddAzureOpenAIChatCompletion(oaiModelName, oaiEndpoint, oaiKey);  //Alternative notation
            builder.Services.AddAzureOpenAIChatCompletion(oaiModelName, oaiEndpoint, oaiKey);

            // add logging
            builder.Services.AddLogging(configure => configure.AddConsole());
            builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Trace));

            var kernel = builder.Build();

            //await Exercise1(kernel);
            await Exercise2(kernel);


            Console.WriteLine("\nSemantic Kernel app finished.");
        }

        public static async Task Exercise1(Kernel kernel)
        {
            for (; ; )
            {
                //TODO 1.4 - Gather user inputstring input;    
                Console.WriteLine("Do you have a question?");
                string input = Console.ReadLine(); //"Give me a list of breakfast foods with eggs and cheese"

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                //TODO 1.5 - Provide response based on the user input
                var result = await kernel.InvokePromptAsync<string>(input);
                Console.WriteLine(result);
            }
        }

        public static async Task Exercise2(Kernel kernel)
        {
            ChatHistory history = [];

            List<string> choices = ["ContinueConversation", "EndConversation"];

            // Create few-shot examples
            List<ChatHistory> fewShotExamples =
            [
                // TODO 2.1 Create few-shot examples
                [
                    new ChatMessageContent(AuthorRole.User, "Can you send a very quick approval to the marketing team?"),
                    new ChatMessageContent(AuthorRole.System, "Intent:"),
                    new ChatMessageContent(AuthorRole.Assistant, "ContinueConversation")
                ],
                [
                    new ChatMessageContent(AuthorRole.User, "Thanks, I'm done for now."),
                    new ChatMessageContent(AuthorRole.System, "Intent:"),
                    new ChatMessageContent(AuthorRole.Assistant, "EndConversation")
                ]
            ];



            // Create handlebars template for intent
            // TODO 2.2 Add few shot examples template and chat history template

            var getIntent = kernel.CreateFunctionFromPrompt(
                new()
                {
                    Template = @"
                        <message role=""system"">Instructions: What is the intent of this request?
                        Do not explain the reasoning, just reply back with the intent. 
                        If you are unsure, reply with {{choices[0]}}.
                        Choices: {{choices}}.</message>

                        {{#each fewShotExamples}}
                            {{#each this}}
                                <message role=""{{role}}"">{{content}}</message>
                            {{/each}}
                        {{/each}}

                        {{#each chatHistory}}
                            <message role=""{{role}}"">{{content}}</message>
                        {{/each}}

                        <message role=""user"">{{request}}</message>
                        <message role=""system"">Intent:</message>
                        ",
                    TemplateFormat = "handlebars"
                }, 
                new HandlebarsPromptTemplateFactory());

            // TODO 2.3 Create a template for chat by including the history, request, and assistant response
            string chatTemplate = @"{{$history}}    User: {{$request}}    Assistant: ";

            //var chat = ;
            var chat = kernel.CreateFunctionFromPrompt(chatTemplate);

            // Start the chat loop
            while (true)
            {
                Console.Write("User > ");
                var request = Console.ReadLine();
                // Invoke prompt
                var intent = await kernel.InvokeAsync(getIntent,
                                                        new()
                                                        {
                                                        { "request", request },
                                                        { "choices", choices },
                                                        { "history", history },
                                                        { "fewShotExamples", fewShotExamples }
                                                        });

                // End the chat if the intent is "EndConversation"
                if (intent.ToString() == "EndConversation")
                {
                    break;
                }

                // Get chat response
                var chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(chat,
                    new KernelArguments()
                    {
                        { "request", request },
                        { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
                    });

                // Stream the response
                string message = "";
                await foreach (var chunk in chatResult)
                {
                    if (chunk.Role.HasValue)
                        Console.Write(chunk.Role + " > ");

                    message += chunk;
                    Console.Write(chunk);
                }

                Console.WriteLine();
                history.AddUserMessage(request!);

                // TODO 2.4 Append the assistant message to history
                history.AddAssistantMessage(message);
            }
        }
    }
}

