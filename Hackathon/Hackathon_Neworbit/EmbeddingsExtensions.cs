namespace Hackathon_Neworbit;

public static class EmbeddingsExtensions
{
    public static float DistanceFrom(this ReadOnlyMemory<float> v1, ReadOnlyMemory<float> v2)
    {
        var sum = 0.0f;
        for (var i = 0; i < v1.Length; i++)
        {
            sum += (v1.Span[i] - v2.Span[i]) * (v1.Span[i] - v2.Span[i]);
        }

        return MathF.Sqrt(sum);
    }
}