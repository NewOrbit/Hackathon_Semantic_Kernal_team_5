using System.Collections.ObjectModel;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;

#pragma warning disable SKEXP0001

namespace Hackathon_Neworbit;

public class EmbeddingsManager
{
    private const string FileName = "./embeddings.txt";

    // in memory list of embeddings, persisted to file
    private readonly List<Embedding> _db = new();
    public ITextEmbeddingGenerationService Embeddings { get; set; }

    public EmbeddingsManager(ITextEmbeddingGenerationService embeddings)
    {
        Console.WriteLine("Loading saved embeddings...");
        this.Embeddings = embeddings;
        if (File.Exists(FileName))
        {
            var json = File.ReadAllText(FileName);
            _db = JsonSerializer.Deserialize<List<Embedding>>(json) ?? new List<Embedding>();
        }
    }

    public async Task ProgramEmbedding(string id, string textToEmbed)
    {
        // find if Id already in _db
        var existingIndex = _db.FindIndex(x => x.Id == id);
        var existing = existingIndex >= 0 ? _db[existingIndex] : null;

        if (existing is not null && existing.Text == textToEmbed)
        {
            // don't do anything if already embedded by the same text
            return;
        }

        if (existing is null)
        {
            Console.WriteLine($"Embedding {id} as: {textToEmbed}");
        }
        else
        {
            Console.WriteLine($"Re-embedding {id} as: {textToEmbed}");
        }

        var vector = await this.Embeddings.GenerateEmbeddingAsync(textToEmbed);

        // update or add new embedding
        if (existing is not null)
        {
            _db.RemoveAt(existingIndex);
        }

        _db.Add(new Embedding(id, textToEmbed, vector));

        // persist _db to file
        var json = JsonSerializer.Serialize(_db);
        await File.WriteAllTextAsync(FileName, json);
    }

    public async Task<IEnumerable<IdWithDistance>> Search(string searchText)
    {
        var searchVector = await this.Embeddings.GenerateEmbeddingAsync(searchText);

        var distances = _db.Select(embedding => new IdWithDistance(
                embedding.Id,
                embedding.Vector.DistanceFrom(searchVector)))
            .OrderBy(x => x.Distance)
            .ToArray();

        return distances;
    }
}

record Embedding(string Id, string Text, ReadOnlyMemory<float> Vector);

public record IdWithDistance(string Id, float Distance);