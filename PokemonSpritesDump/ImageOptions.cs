namespace PokemonSpritesDump;

public record ImageOptions
{
    public ImageFormat Format { get; set; } = ImageFormat.Webp;
    public int Quality { get; set; } = 100;
    public bool Lossless { get; set; } = true;
}

public enum ImageFormat
{
    Png,
    Jpg,
    Webp,
}
