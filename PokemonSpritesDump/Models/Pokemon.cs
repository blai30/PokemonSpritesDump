using System.Text.Json.Serialization;

namespace PokemonSpritesDump.Models;

public record Pokemon
{
    [JsonPropertyName("abilities")]
    public List<Abilities>? Abilities { get; set; }

    [JsonPropertyName("base_experience")]
    public int? BaseExperience { get; set; }

    [JsonPropertyName("cries")]
    public Cries? Cries { get; set; }

    [JsonPropertyName("forms")]
    public List<NamedApiResource> Forms { get; set; }

    [JsonPropertyName("game_indices")]
    public List<NamedApiResource>? GameIndices { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("held_items")]
    public List<NamedApiResource>? HeldItems { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("location_area_encounters")]
    public string? LocationAreaEncounters { get; set; }

    [JsonPropertyName("moves")]
    public List<Moves>? Moves { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("past_abilities")]
    public List<NamedApiResource>? PastAbilities { get; set; }

    [JsonPropertyName("past_types")]
    public List<NamedApiResource>? PastTypes { get; set; }

    [JsonPropertyName("species")]
    public NamedApiResource Species { get; set; }

    [JsonPropertyName("sprites")]
    public Sprites? Sprites { get; set; }

    [JsonPropertyName("stats")]
    public List<Stats>? Stats { get; set; }

    [JsonPropertyName("types")]
    public List<Types> Types { get; set; }

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }
}

public record Abilities
{
    [JsonPropertyName("ability")]
    public NamedApiResource? Ability { get; set; }

    [JsonPropertyName("is_hidden")]
    public bool IsHidden { get; set; }

    [JsonPropertyName("slot")]
    public int Slot { get; set; }
}

public record Cries
{
    [JsonPropertyName("latest")]
    public string? Latest { get; set; }

    [JsonPropertyName("legacy")]
    public string? Legacy { get; set; }
}

public record Moves
{
    [JsonPropertyName("move")]
    public NamedApiResource? Move { get; set; }

    [JsonPropertyName("version_group_details")]
    public List<VersionGroupDetails> VersionGroupDetails { get; set; }
}

public record VersionGroupDetails
{
    [JsonPropertyName("level_learned_at")]
    public int LevelLearnedAt { get; set; }

    [JsonPropertyName("move_learn_method")]
    public NamedApiResource? MoveLearnMethod { get; set; }

    [JsonPropertyName("version_group")]
    public NamedApiResource? VersionGroup { get; set; }
}

public record Sprites
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }

    [JsonPropertyName("other")]
    public Other? Other { get; set; }

    [JsonPropertyName("versions")]
    public Versions? Versions { get; set; }
}

public record Other
{
    [JsonPropertyName("dream_world")]
    public DreamWorld? DreamWorld { get; set; }

    [JsonPropertyName("home")]
    public Home? Home { get; set; }

    [JsonPropertyName("official-artwork")]
    public OfficialArtwork? OfficialArtwork { get; set; }

    [JsonPropertyName("showdown")]
    public Showdown? Showdown { get; set; }
}

public record DreamWorld
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }
}

public record Home
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record OfficialArtwork
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }
}

public record Showdown
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record Versions
{
    [JsonPropertyName("generation-i")]
    public GenerationI? GenerationI { get; set; }

    [JsonPropertyName("generation-ii")]
    public GenerationIi? GenerationIi { get; set; }

    [JsonPropertyName("generation-iii")]
    public GenerationIii? GenerationIii { get; set; }

    [JsonPropertyName("generation-iv")]
    public GenerationIv? GenerationIv { get; set; }

    [JsonPropertyName("generation-v")]
    public GenerationV? GenerationV { get; set; }

    [JsonPropertyName("generation-vi")]
    public GenerationVi? GenerationVi { get; set; }

    [JsonPropertyName("generation-vii")]
    public GenerationVii? GenerationVii { get; set; }

    [JsonPropertyName("generation-viii")]
    public GenerationViii? GenerationViii { get; set; }
}

public record GenerationI
{
    [JsonPropertyName("red-blue")]
    public RedBlue? RedBlue { get; set; }

    [JsonPropertyName("yellow")]
    public Yellow? Yellow { get; set; }
}

public record RedBlue
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_gray")]
    public string? BackGray { get; set; }

    [JsonPropertyName("back_transparent")]
    public string? BackTransparent { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_gray")]
    public string? FrontGray { get; set; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; set; }
}

public record Yellow
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_gray")]
    public string? BackGray { get; set; }

    [JsonPropertyName("back_transparent")]
    public string? BackTransparent { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_gray")]
    public string? FrontGray { get; set; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; set; }
}

public record GenerationIi
{
    [JsonPropertyName("crystal")]
    public Crystal? Crystal { get; set; }

    [JsonPropertyName("gold")]
    public Gold? Gold { get; set; }

    [JsonPropertyName("silver")]
    public Silver? Silver { get; set; }
}

public record Crystal
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_transparent")]
    public string? BackShinyTransparent { get; set; }

    [JsonPropertyName("back_transparent")]
    public string? BackTransparent { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_transparent")]
    public string? FrontShinyTransparent { get; set; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; set; }
}

public record Gold
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; set; }
}

public record Silver
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_transparent")]
    public string? FrontTransparent { get; set; }
}

public record GenerationIii
{
    [JsonPropertyName("emerald")]
    public Emerald? Emerald { get; set; }

    [JsonPropertyName("firered-leafgreen")]
    public FireredLeafgreen? FireredLeafgreen { get; set; }

    [JsonPropertyName("ruby-sapphire")]
    public RubySapphire? RubySapphire { get; set; }
}

public record Emerald
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }
}

public record FireredLeafgreen
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }
}

public record RubySapphire
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }
}

public record GenerationIv
{
    [JsonPropertyName("diamond-pearl")]
    public DiamondPearl? DiamondPearl { get; set; }

    [JsonPropertyName("heartgold-soulsilver")]
    public HeartgoldSoulsilver? HeartgoldSoulsilver { get; set; }

    [JsonPropertyName("platinum")]
    public Platinum? Platinum { get; set; }
}

public record DiamondPearl
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record HeartgoldSoulsilver
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record Platinum
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record GenerationV
{
    [JsonPropertyName("black-white")]
    public BlackWhite? BlackWhite { get; set; }
}

public record BlackWhite
{
    [JsonPropertyName("animated")]
    public Animated? Animated { get; set; }

    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record Animated
{
    [JsonPropertyName("back_default")]
    public string? BackDefault { get; set; }

    [JsonPropertyName("back_female")]
    public string? BackFemale { get; set; }

    [JsonPropertyName("back_shiny")]
    public string? BackShiny { get; set; }

    [JsonPropertyName("back_shiny_female")]
    public string? BackShinyFemale { get; set; }

    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record GenerationVi
{
    [JsonPropertyName("omegaruby-alphasapphire")]
    public OmegarubyAlphasapphire? OmegarubyAlphasapphire { get; set; }

    [JsonPropertyName("x-y")]
    public XY? XY { get; set; }
}

public record OmegarubyAlphasapphire
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record XY
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record GenerationVii
{
    [JsonPropertyName("icons")]
    public Icons? Icons { get; set; }

    [JsonPropertyName("ultra-sun-ultra-moon")]
    public UltraSunUltraMoon? UltraSunUltraMoon { get; set; }
}

public record Icons
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }
}

public record UltraSunUltraMoon
{
    [JsonPropertyName("front_default")]
    public string? FrontDefault { get; set; }

    [JsonPropertyName("front_female")]
    public string? FrontFemale { get; set; }

    [JsonPropertyName("front_shiny")]
    public string? FrontShiny { get; set; }

    [JsonPropertyName("front_shiny_female")]
    public string? FrontShinyFemale { get; set; }
}

public record GenerationViii
{
    [JsonPropertyName("icons")]
    public Icons? Icons { get; set; }
}

public record Stats
{
    [JsonPropertyName("base_stat")]
    public int BaseStat { get; set; }

    [JsonPropertyName("effort")]
    public int Effort { get; set; }

    [JsonPropertyName("stat")]
    public NamedApiResource? Stat { get; set; }
}

public record Types
{
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("type")]
    public NamedApiResource? Type { get; set; }
}
