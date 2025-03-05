using System.Text.Json.Serialization;

namespace PokemonSpritesDump.Models;

public record PokemonForm
{
    [JsonPropertyName("form_name")]
    public string FormName { get; set; }

    [JsonPropertyName("form_names")]
    public List<Names>? FormNames { get; set; }

    [JsonPropertyName("form_order")]
    public int? FormOrder { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("is_battle_only")]
    public bool IsBattleOnly { get; set; }

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("is_mega")]
    public bool IsMega { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("names")]
    public List<Names>? Names { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("pokemon")]
    public NamedApiResource? Pokemon { get; set; }

    [JsonPropertyName("sprites")]
    public Sprites? Sprites { get; set; }

    [JsonPropertyName("types")]
    public List<Types>? Types { get; set; }

    [JsonPropertyName("version_group")]
    public NamedApiResource? VersionGroup { get; set; }
}
