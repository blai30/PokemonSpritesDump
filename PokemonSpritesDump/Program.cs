using PokemonSpritesDump;
using PokemonSpritesDump.Services;

Console.WriteLine(DateTime.UtcNow.ToString("R"));
Console.WriteLine(Environment.ProcessId);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<ApiOptions>().BindConfiguration("ApiOptions");
builder.Services.AddOptions<ImageOptions>().BindConfiguration("ImageOptions");
builder.Services.AddSingleton<HttpClient>();
// builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SpriteDownloader>();
builder.Services.AddSingleton<IImageConverter, WebpImageConverter>();

var host = builder.Build();
await host.RunAsync();
