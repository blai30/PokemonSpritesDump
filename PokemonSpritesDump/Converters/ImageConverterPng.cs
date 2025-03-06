namespace PokemonSpritesDump.Converters;

public class ImageConverterPng(ILogger<ImageConverterPng> logger) : IImageConverter
{
    public string GetFileExtension() => ".png";

    public Task<byte[]> ConvertToAsync(byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken stoppingToken = default)
    {
        return Task.FromResult(sourceData);
    }

    public async Task SaveAsAsync(string fileName, byte[] imageData, int quality = 75, bool lossless = false,
        CancellationToken stoppingToken = default)
    {
        await File.WriteAllBytesAsync(fileName, imageData, stoppingToken);
        logger.LogDebug("Saved png image to: {FileName}", fileName);
    }
}
