namespace PokemonSpritesDump.Services;

public interface IImageConverter
{
    Task<byte[]> ConvertToAsync(byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken cancellationToken = default);

    Task SaveAsAsync(string outputPath, byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken cancellationToken = default);
}
