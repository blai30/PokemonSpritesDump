using System.Text.Json.Serialization;

namespace PokemonSpritesDump.Models;

public record PokemonForm
{
    [JsonPropertyName("form_name")]
    public string FormName { get; init; }

    [JsonPropertyName("form_names")]
    public List<Names>? FormNames { get; init; }

    [JsonPropertyName("form_order")]
    public int? FormOrder { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("is_battle_only")]
    public bool IsBattleOnly { get; init; }

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; init; }

    [JsonPropertyName("is_mega")]
    public bool IsMega { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("names")]
    public List<Names>? Names { get; init; }

    [JsonPropertyName("order")]
    public int? Order { get; init; }

    [JsonPropertyName("pokemon")]
    public NamedApiResource? Pokemon { get; init; }

    [JsonPropertyName("sprites")]
    public Sprites? Sprites { get; init; }

    [JsonPropertyName("types")]
    public List<Types>? Types { get; init; }

    [JsonPropertyName("version_group")]
    public NamedApiResource? VersionGroup { get; init; }
}
