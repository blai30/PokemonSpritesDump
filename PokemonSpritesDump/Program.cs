using PokemonSpritesDump;

Console.WriteLine(DateTime.UtcNow.ToString("R"));
Console.WriteLine(Environment.ProcessId);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<ApiOptions>().BindConfiguration("ApiOptions");
builder.Services.AddSingleton<HttpClient>();
// builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SpriteDownloader>();

var host = builder.Build();
await host.RunAsync();
