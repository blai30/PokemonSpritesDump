using System.Text.Json.Serialization;

namespace SpriteDownloader;

public record PokemonSpecies
{
    [JsonPropertyName("base_happiness")]
    public int? BaseHappiness { get; set; }

    [JsonPropertyName("capture_rate")]
    public int? CaptureRate { get; set; }

    [JsonPropertyName("color")]
    public NamedApiResource? Color { get; set; }

    [JsonPropertyName("egg_groups")]
    public List<NamedApiResource> EggGroups { get; set; }

    [JsonPropertyName("evolution_chain")]
    public ApiResource? EvolutionChain { get; set; }

    [JsonPropertyName("evolves_from_species")]
    public NamedApiResource? EvolvesFromSpecies { get; set; }

    [JsonPropertyName("flavor_text_entries")]
    public List<FlavorTextEntries>? FlavorTextEntries { get; set; }

    [JsonPropertyName("form_descriptions")]
    public List<FormDescriptions>? FormDescriptions { get; set; }

    [JsonPropertyName("forms_switchable")]
    public bool FormsSwitchable { get; set; }

    [JsonPropertyName("gender_rate")]
    public int? GenderRate { get; set; }

    [JsonPropertyName("genera")]
    public List<Genera>? Genera { get; set; }

    [JsonPropertyName("generation")]
    public NamedApiResource? Generation { get; set; }

    [JsonPropertyName("growth_rate")]
    public NamedApiResource? GrowthRate { get; set; }

    [JsonPropertyName("habitat")]
    public NamedApiResource? Habitat { get; set; }

    [JsonPropertyName("has_gender_differences")]
    public bool HasGenderDifferences { get; set; }

    [JsonPropertyName("hatch_counter")]
    public int? HatchCounter { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("is_baby")]
    public bool IsBaby { get; set; }

    [JsonPropertyName("is_legendary")]
    public bool IsLegendary { get; set; }

    [JsonPropertyName("is_mythical")]
    public bool IsMythical { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("names")]
    public List<Names>? Names { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("pal_park_encounters")]
    public List<NamedApiResource>? PalParkEncounters { get; set; }

    [JsonPropertyName("pokedex_numbers")]
    public List<PokedexNumbers>? PokedexNumbers { get; set; }

    [JsonPropertyName("shape")]
    public NamedApiResource? Shape { get; set; }

    [JsonPropertyName("varieties")]
    public List<Varieties>? Varieties { get; set; }
}

public record FlavorTextEntries
{
    [JsonPropertyName("flavor_text")]
    public string? FlavorText { get; set; }

    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; set; }

    [JsonPropertyName("version")]
    public NamedApiResource? Version { get; set; }
}

public record FormDescriptions
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; set; }
}

public record Genera
{
    [JsonPropertyName("genus")]
    public string Genus { get; set; }

    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; set; }
}

public record Names
{
    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public record PokedexNumbers
{
    [JsonPropertyName("entry_number")]
    public int EntryNumber { get; set; }

    [JsonPropertyName("pokedex")]
    public NamedApiResource? Pokedex { get; set; }
}

public record Varieties
{
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("pokemon")]
    public NamedApiResource Pokemon { get; set; }
}
