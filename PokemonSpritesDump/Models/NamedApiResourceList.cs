using System.Text.Json.Serialization;

namespace PokemonSpritesDump.Models;

public record NamedApiResourceList
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("next")]
    public string? Next { get; init; }

    [JsonPropertyName("previous")]
    public string? Previous { get; init; }

    [JsonPropertyName("results")]
    public List<NamedApiResource>? Results { get; init; }
}

public record NamedApiResource
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

public record ApiResource
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}
