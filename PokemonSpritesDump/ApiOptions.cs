namespace PokemonSpritesDump;

public record ApiOptions
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public bool? BruteForce { get; set; }
}
