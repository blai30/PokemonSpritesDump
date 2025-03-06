using SixLabors.ImageSharp;

namespace PokemonSpritesDump.Converters;

public class ImageConverterJpeg(ILogger<ImageConverterJpeg> logger) : IImageConverter
{
    public string GetFileExtension() => ".jpg";

    public async Task<byte[]> ConvertToAsync(byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken stoppingToken = default)
    {
        using var image = Image.Load(sourceData);
        using var outputStream = new MemoryStream();

        var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
        {
            Quality = quality,
        };

        await image.SaveAsJpegAsync(outputStream, encoder, stoppingToken);
        return outputStream.ToArray();
    }

    public async Task SaveAsAsync(string outputPath, byte[] sourceData, int quality = 100, bool lossless = true,
        CancellationToken stoppingToken = default)
    {
        using var image = Image.Load(sourceData);

        var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
        {
            Quality = quality,
        };

        await image.SaveAsJpegAsync(outputPath, encoder, stoppingToken);
        logger.LogDebug("Saved jpeg image to {FilePath}", outputPath);
    }
}
