using Microsoft.Extensions.Options;
using PokemonSpritesDump;
using PokemonSpritesDump.Converters;
using PokemonSpritesDump.Services;

Console.WriteLine(DateTime.UtcNow.ToString("R"));
Console.WriteLine(Environment.ProcessId);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<ApiOptions>().BindConfiguration("ApiOptions");
builder.Services.AddOptions<ImageOptions>().BindConfiguration("ImageOptions");
builder.Services.AddSingleton<HttpClient>();

// builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SpriteDownloader>();
builder.Services.AddSingleton<IImageConverter>(provider =>
{
    var imageOptions = provider.GetRequiredService<IOptions<ImageOptions>>().Value;
    return imageOptions.Format switch
    {
        ImageFormat.Png => new ImageConverterPng(
            provider.GetRequiredService<ILogger<ImageConverterPng>>()
        ),
        ImageFormat.Jpg => new ImageConverterJpeg(
            provider.GetRequiredService<ILogger<ImageConverterJpeg>>()
        ),
        ImageFormat.Webp => new ImageConverterWebp(
            provider.GetRequiredService<ILogger<ImageConverterWebp>>()
        ),
        _ => throw new NotSupportedException(
            $"Image format {imageOptions.Format} is not supported."
        ),
    };
});

var host = builder.Build();
await host.RunAsync();
