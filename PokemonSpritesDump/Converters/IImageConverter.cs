namespace PokemonSpritesDump.Converters;

public interface IImageConverter
{
    string GetFileExtension();

    Task<byte[]> ConvertToAsync(
        byte[] sourceData,
        int quality = 100,
        bool lossless = true,
        CancellationToken stoppingToken = default
    );

    Task SaveAsAsync(
        string outputPath,
        byte[] sourceData,
        int quality = 100,
        bool lossless = true,
        CancellationToken stoppingToken = default
    );
}
