using System.Text.Json.Serialization;

namespace SpriteDownloader;

public record NamedApiResourceList
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<NamedApiResource> Results { get; set; }
}

public record NamedApiResource
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public record ApiResource
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
}
