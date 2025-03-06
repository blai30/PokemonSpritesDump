namespace PokemonSpritesDump;

public record ImageOptions
{
    public int Quality { get; set; } = 100;
    public bool Lossless { get; set; } = true;
}
