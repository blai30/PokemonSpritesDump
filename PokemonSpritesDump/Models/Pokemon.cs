using System.Text.Json.Serialization;

namespace PokemonSpritesDump.Models;

public record Pokemon
{
    [JsonPropertyName("abilities")]
    public List<Abilities>? Abilities { get; init; }

    [JsonPropertyName("base_experience")]
    public int? BaseExperience { get; init; }

    [JsonPropertyName("cries")]
    public Cries? Cries { get; init; }

    [JsonPropertyName("forms")]
    public List<NamedApiResource> Forms { get; init; }

    [JsonPropertyName("game_indices")]
    public List<NamedApiResource>? GameIndices { get; init; }

    [JsonPropertyName("height")]
    public int? Height { get; init; }

    [JsonPropertyName("held_items")]
    public List<NamedApiResource>? HeldItems { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; init; }

    [JsonPropertyName("location_area_encounters")]
    public string? LocationAreaEncounters { get; init; }

    [JsonPropertyName("moves")]
    public List<Moves>? Moves { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("order")]
    public int? Order { get; init; }

    [JsonPropertyName("past_abilities")]
    public List<NamedApiResource>? PastAbilities { get; init; }

    [JsonPropertyName("past_types")]
    public List<NamedApiResource>? PastTypes { get; init; }

    [JsonPropertyName("species")]
    public NamedApiResource Species { get; init; }

    [JsonPropertyName("sprites")]
    public Sprites? Sprites { get; init; }

    [JsonPropertyName("stats")]
    public List<Stats>? Stats { get; init; }

    [JsonPropertyName("types")]
    public List<Types> Types { get; init; }

    [JsonPropertyName("weight")]
    public int? Weight { get; init; }
}

public record Abilities
{
    [JsonPropertyName("ability")]
    public NamedApiResource? Ability { get; init; }

    [JsonPropertyName("is_hidden")]
    public bool IsHidden { get; init; }

    [JsonPropertyName("slot")]
    public int Slot { get; init; }
}

public record Cries
{
    [JsonPropertyName("latest")]
    public string? Latest { get; init; }

    [JsonPropertyName("legacy")]
    public string? Legacy { get; init; }
}

public record Moves
{
    [JsonPropertyName("move")]
    public NamedApiResource? Move { get; init; }

    [JsonPropertyName("version_group_details")]
    public List<VersionGroupDetails> VersionGroupDetails { get; init; }
}

public record VersionGroupDetails
{
    [JsonPropertyName("level_learned_at")]
    public int LevelLearnedAt { get; init; }

    [JsonPropertyName("move_learn_method")]
    public NamedApiResource? MoveLearnMethod { get; init; }

    [JsonPropertyName("version_group")]
    public NamedApiResource? VersionGroup { get; init; }
}

public record Sprites
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }

    [JsonPropertyName("other")]
    public Other? Other { get; init; }

    [JsonPropertyName("versions")]
    public Versions? Versions { get; init; }
}

public record Other
{
    [JsonPropertyName("dream_world")]
    public DreamWorld? DreamWorld { get; init; }

    [JsonPropertyName("home")]
    public Home? Home { get; init; }

    [JsonPropertyName("official-artwork")]
    public OfficialArtwork? OfficialArtwork { get; init; }

    [JsonPropertyName("showdown")]
    public Showdown? Showdown { get; init; }
}

public record DreamWorld
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }
}

public record Home
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record OfficialArtwork
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }
}

public record Showdown
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record Versions
{
    [JsonPropertyName("generation-i")]
    public GenerationI? GenerationI { get; init; }

    [JsonPropertyName("generation-ii")]
    public GenerationIi? GenerationIi { get; init; }

    [JsonPropertyName("generation-iii")]
    public GenerationIii? GenerationIii { get; init; }

    [JsonPropertyName("generation-iv")]
    public GenerationIv? GenerationIv { get; init; }

    [JsonPropertyName("generation-v")]
    public GenerationV? GenerationV { get; init; }

    [JsonPropertyName("generation-vi")]
    public GenerationVi? GenerationVi { get; init; }

    [JsonPropertyName("generation-vii")]
    public GenerationVii? GenerationVii { get; init; }

    [JsonPropertyName("generation-viii")]
    public GenerationViii? GenerationViii { get; init; }
}

public record GenerationI
{
    [JsonPropertyName("red-blue")]
    public RedBlue? RedBlue { get; init; }

    [JsonPropertyName("yellow")]
    public Yellow? Yellow { get; init; }
}

public record RedBlue
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_gray")]
    public string? BackGray { get; init; }

    [JsonPropertyName("back_transparent")]
    public string? BackTransparent { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_gray")]
    public string? FrontGray { get; init; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; init; }
}

public record Yellow
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_gray")]
    public string? BackGray { get; init; }

    [JsonPropertyName("back_transparent")]
    public string? BackTransparent { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_gray")]
    public string? FrontGray { get; init; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; init; }
}

public record GenerationIi
{
    [JsonPropertyName("crystal")]
    public Crystal? Crystal { get; init; }

    [JsonPropertyName("gold")]
    public Gold? Gold { get; init; }

    [JsonPropertyName("silver")]
    public Silver? Silver { get; init; }
}

public record Crystal
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_transparent")]
    public string? BackShinyTransparent { get; init; }

    [JsonPropertyName("back_transparent")]
    public string? BackTransparent { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_transparent")]
    public string? FrontShinyTransparent { get; init; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; init; }
}

public record Gold
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; init; }
}

public record Silver
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; init; }
}

public record GenerationIii
{
    [JsonPropertyName("emerald")]
    public Emerald? Emerald { get; init; }

    [JsonPropertyName("firered-leafgreen")]
    public FireredLeafgreen? FireredLeafgreen { get; init; }

    [JsonPropertyName("ruby-sapphire")]
    public RubySapphire? RubySapphire { get; init; }
}

public record Emerald
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }
}

public record FireredLeafgreen
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }
}

public record RubySapphire
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }
}

public record GenerationIv
{
    [JsonPropertyName("diamond-pearl")]
    public DiamondPearl? DiamondPearl { get; init; }

    [JsonPropertyName("heartgold-soulsilver")]
    public HeartgoldSoulsilver? HeartgoldSoulsilver { get; init; }

    [JsonPropertyName("platinum")]
    public Platinum? Platinum { get; init; }
}

public record DiamondPearl
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record HeartgoldSoulsilver
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record Platinum
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record GenerationV
{
    [JsonPropertyName("black-white")]
    public BlackWhite? BlackWhite { get; init; }
}

public record BlackWhite
{
    [JsonPropertyName("animated")]
    public Animated? Animated { get; init; }

    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record Animated
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; init; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; init; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; init; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; init; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record GenerationVi
{
    [JsonPropertyName("omegaruby-alphasapphire")]
    public OmegarubyAlphasapphire? OmegarubyAlphasapphire { get; init; }

    [JsonPropertyName("x-y")]
    public XY? XY { get; init; }
}

public record OmegarubyAlphasapphire
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record XY
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record GenerationVii
{
    [JsonPropertyName("icons")]
    public Icons? Icons { get; init; }

    [JsonPropertyName("ultra-sun-ultra-moon")]
    public UltraSunUltraMoon? UltraSunUltraMoon { get; init; }
}

public record Icons
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }
}

public record UltraSunUltraMoon
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; init; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; init; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; init; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; init; }
}

public record GenerationViii
{
    [JsonPropertyName("icons")]
    public Icons? Icons { get; init; }
}

public record Stats
{
    [JsonPropertyName("base_stat")]
    public int BaseStat { get; init; }

    [JsonPropertyName("effort")]
    public int Effort { get; init; }

    [JsonPropertyName("stat")]
    public NamedApiResource? Stat { get; init; }
}

public record Types
{
    [JsonPropertyName("slot")]
    public int Slot { get; init; }

    [JsonPropertyName("type")]
    public NamedApiResource? Type { get; init; }
}
