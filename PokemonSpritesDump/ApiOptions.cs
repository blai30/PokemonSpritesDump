namespace PokemonSpritesDump;

public record ApiOptions
{
    public int Limit { get; set; } = 151;
    public int Offset { get; set; } = 0;
    public bool BruteForce { get; set; } = false;
}
