using CsvHelper;
using Hackathon_Neworbit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings()
{
    Args = args
});

// don't forget to add your api key / endpoint / deployment name/and model id ( deployments found here: https://oai.azure.com/ )
builder.Services
    .AddAzureOpenAIChatCompletion(
        "hack-2024-gpt-4o",
        "https://hack-2024-oai.openai.azure.com/",
        "4705922971104f64bab77a51cb68b37d",
        modelId: "gpt-4o");

// How to do embeddings
builder.Services
    .AddAzureOpenAITextEmbeddingGeneration(
        "hack-2024-embeddings",
        "https://hack-2024-oai.openai.azure.com/",
        "4705922971104f64bab77a51cb68b37d",
        modelId: "gpt-4o");

var app = builder.Build();

var chat = app.Services.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

// create embeddings
var embedder = new EmbeddingsManager(app.Services.GetRequiredService<ITextEmbeddingGenerationService>());

// 1. programming all leaflets
Console.WriteLine("Fetching leaflet data...");

// 2. summarise leaflets and embed
var dict = new Dictionary<string, string>();
using (var reader = new StreamReader("meds3.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<LeafletDO>();
    foreach (var record in records)
    {
        dict.Add(record.ProductName, record.Dosage);
    }
}


Console.WriteLine("Programming new embeddings...");
await embedder.ProgramEmbeddingsFromDictionary(dict);


Kernel kernel = new();
kernel.Plugins.AddFromFunctions("time_plugin",
[
    KernelFunctionFactory.CreateFromMethod(
        method: async (string searchText) =>
        {
            Console.WriteLine($"using lookup (plugin) with search text: {searchText}");

            var res = await embedder.Search(searchText);
            return res.Select(x => x.Id).Take(2);
        },
        functionName: "get_matching_leaflets",
        description: "Gets the leaflets that match the search text based on ailments",
        returnParameter: new KernelReturnParameterMetadata()
        {
            Description =
                "List of records contianing the ID as product name and a leaflet text, ordered by distance (vector search)",
        }
    ),
]);


chatHistory.AddSystemMessage("""
                             You are an interactive health chatbot helping users find suitable over-the-counter medications based on their symptoms and personal circumstances.
                              
                             Start by asking the user to describe their main symptom. Once the user responds, ask clarifying questions one by one to gather more specific details about the symptom.
                              
                             Next, ask about any personal factors one by one, such as pregnancy, driving, alcohol consumption, allergies, or other medications. Use the product leaflets to check for contraindications and interactions.
                              
                             Finally, recommend appropriate medications based on the user’s responses, providing brief explanations for each recommendation and any necessary warnings.
                              
                             Your task is to:

                             1. Begin with an open-ended question about the user’s symptoms.

                             2. Follow up with clarifying questions, asking one at a time.

                             3. Inquire about personal circumstances, asking each question individually.

                             4. Recommend over-the-counter medications based on the user’s symptoms and personal situation.

                             5. Provide explanations for your recommendations and note any warnings.

                             6. Optionally, ask if the user would like additional information, such as availability or pricing of the medications.
                              
                             For example:

                             - Ask: "What symptom are you experiencing?"

                             - Wait for the user’s response, then ask: "How long have you had this symptom?"

                             - After that, ask: "Are you taking any other medications?"
                              
                             Output should include a list of suitable medications and reasons why they are recommended, but remember to ask questions one at a time and adjust based on the user’s responses.
                             """);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Chat GP: Hello!. What ails you?");

AzureOpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

async Task<ChatMessageContent> Talk(string question)
{
    var success = false;
    while (!success)
    {
        try
        {
            var response = await chat.GetChatMessageContentsAsync(chatHistory, settings, kernel);
            success = true;
            return response.Last();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Retrying...");
            await Task.Delay(1000);
        }
    }

    throw new Exception();
}

while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Patient: ");
    var prompt = Console.ReadLine();
    chatHistory.AddUserMessage(prompt!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Chat GP: ");
    var lastMessage = await Talk(prompt!);
    Console.WriteLine(lastMessage);
}