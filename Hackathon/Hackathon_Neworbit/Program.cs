using Hackathon_Neworbit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

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
Console.WriteLine("Programming new embeddings...");
await embedder.ProgramEmbedding("paracetamol", "headache, fever, cough");
await embedder.ProgramEmbedding("cat", "cat");
await embedder.ProgramEmbedding("house", "house");

// 2. AI: What ails you? 
//    User: I have a headache

// 3. Fetch drugs from embeddingsManager

// 4. Summarise leaflets to what to look out for

// 5. pass summarised leaflets to SystemMessage


// while (true)
// {
//     Console.ForegroundColor = ConsoleColor.Yellow;
//     Console.Write("Guess: ");
//     var prompt = Console.ReadLine();
//     var result = await embedder.Search(prompt);
//
//     Console.ForegroundColor = ConsoleColor.Green;
//     
//     Console.WriteLine("Result by distance: ");
//     foreach (var item in result)
//     {
//         Console.WriteLine($"{item.Distance} - {item.Id}");
//     }
// }


chatHistory.AddSystemMessage(
   $"""
    You are a pharmacist trying to help choose the right medication for a patient. 
    The recommended drugs are: with leaflets:
    Make sure that the user can take the drugs, ask them questions to make sure they can take the drugs.
    
    Paracetmol leaflet summary:
    .
    
    
    .
    .
    .
    .
    
    
    
    Ibuprofen leaflet:
    ..
    .
    .
    .
    
    .
    .
    
    
    now go help them
    """);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Chat GP: Hello!. What ails you?");

chatHistory.AddSystemMessage(
    "You are a pharmacist trying to help choose the right medication for a patient. The patient");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Chat GP: Hello!. What ails you?");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Jay: ");
    var prompt = Console.ReadLine();
    chatHistory.AddUserMessage(prompt!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("AI: ");
    var response = await chat.GetChatMessageContentsAsync(chatHistory);
    var lastMessage = response.Last();
    Console.WriteLine(lastMessage);
}