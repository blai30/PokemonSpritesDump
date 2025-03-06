using SixLabors.ImageSharp;

namespace PokemonSpritesDump.Converters;

public class ImageConverterWebp(ILogger<ImageConverterWebp> logger) : IImageConverter
{
    public string GetFileExtension() => ".webp";

    public async Task<byte[]> ConvertToAsync(byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken stoppingToken = default)
    {
        using var image = Image.Load(sourceData);
        using var outputStream = new MemoryStream();

        var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder
        {
            Quality = quality,
            FileFormat = lossless
                ? SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossless
                : SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossy
        };

        await image.SaveAsWebpAsync(outputStream, encoder, stoppingToken);
        return outputStream.ToArray();
    }

    public async Task SaveAsAsync(string outputPath, byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken stoppingToken = default)
    {
        using var image = Image.Load(sourceData);

        var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder
        {
            Quality = quality,
            FileFormat = lossless
                ? SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossless
                : SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossy
        };

        await image.SaveAsWebpAsync(outputPath, encoder, stoppingToken);
        logger.LogDebug("Saved webp image to {FilePath}", outputPath);
    }
}
