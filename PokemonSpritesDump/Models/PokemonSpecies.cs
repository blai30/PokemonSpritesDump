using System.Text.Json.Serialization;

namespace PokemonSpritesDump.Models;

public record PokemonSpecies
{
    [JsonPropertyName("base_happiness")]
    public int? BaseHappiness { get; init; }

    [JsonPropertyName("capture_rate")]
    public int? CaptureRate { get; init; }

    [JsonPropertyName("color")]
    public NamedApiResource? Color { get; init; }

    [JsonPropertyName("egg_groups")]
    public List<NamedApiResource> EggGroups { get; init; }

    [JsonPropertyName("evolution_chain")]
    public ApiResource? EvolutionChain { get; init; }

    [JsonPropertyName("evolves_from_species")]
    public NamedApiResource? EvolvesFromSpecies { get; init; }

    [JsonPropertyName("flavor_text_entries")]
    public List<FlavorTextEntries>? FlavorTextEntries { get; init; }

    [JsonPropertyName("form_descriptions")]
    public List<FormDescriptions>? FormDescriptions { get; init; }

    [JsonPropertyName("forms_switchable")]
    public bool FormsSwitchable { get; init; }

    [JsonPropertyName("gender_rate")]
    public int? GenderRate { get; init; }

    [JsonPropertyName("genera")]
    public List<Genera>? Genera { get; init; }

    [JsonPropertyName("generation")]
    public NamedApiResource? Generation { get; init; }

    [JsonPropertyName("growth_rate")]
    public NamedApiResource? GrowthRate { get; init; }

    [JsonPropertyName("habitat")]
    public NamedApiResource? Habitat { get; init; }

    [JsonPropertyName("has_gender_differences")]
    public bool HasGenderDifferences { get; init; }

    [JsonPropertyName("hatch_counter")]
    public int? HatchCounter { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("is_baby")]
    public bool IsBaby { get; init; }

    [JsonPropertyName("is_legendary")]
    public bool IsLegendary { get; init; }

    [JsonPropertyName("is_mythical")]
    public bool IsMythical { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("names")]
    public List<Names>? Names { get; init; }

    [JsonPropertyName("order")]
    public int? Order { get; init; }

    [JsonPropertyName("pal_park_encounters")]
    public List<NamedApiResource>? PalParkEncounters { get; init; }

    [JsonPropertyName("pokedex_numbers")]
    public List<PokedexNumbers>? PokedexNumbers { get; init; }

    [JsonPropertyName("shape")]
    public NamedApiResource? Shape { get; init; }

    [JsonPropertyName("varieties")]
    public List<Varieties>? Varieties { get; init; }
}

public record FlavorTextEntries
{
    [JsonPropertyName("flavor_text")]
    public string? FlavorText { get; init; }

    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; init; }

    [JsonPropertyName("version")]
    public NamedApiResource? Version { get; init; }
}

public record FormDescriptions
{
    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; init; }
}

public record Genera
{
    [JsonPropertyName("genus")]
    public string Genus { get; init; }

    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; init; }
}

public record Names
{
    [JsonPropertyName("language")]
    public NamedApiResource? Language { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}

public record PokedexNumbers
{
    [JsonPropertyName("entry_number")]
    public int EntryNumber { get; init; }

    [JsonPropertyName("pokedex")]
    public NamedApiResource? Pokedex { get; init; }
}

public record Varieties
{
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; init; }

    [JsonPropertyName("pokemon")]
    public NamedApiResource Pokemon { get; init; }
}
