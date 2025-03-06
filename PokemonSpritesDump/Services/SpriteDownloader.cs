using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PokemonSpritesDump.Models;

namespace PokemonSpritesDump.Services;

public class SpriteDownloader : BackgroundService
{
    private const string SpritesDirectory = "out/sprites";
    private const string CacheDirectory = "out/cache";
    private const int BatchSize = 20;
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpriteDownloader> _logger;
    private readonly IOptions<ApiOptions> _options;
    private readonly IImageConverter _imageConverter;

    public SpriteDownloader(
        ILogger<SpriteDownloader> logger,
        HttpClient httpClient,
        IImageConverter imageConverter,
        IOptions<ApiOptions> options)
    {
        _logger = logger;
        _httpClient = httpClient;
        _options = options;
        _imageConverter = imageConverter;
        _httpClient.Timeout = TimeSpan.FromMinutes(2);

        Directory.CreateDirectory(SpritesDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting sprite download process");
        _logger.LogInformation("Building slug map...");
        var slugMap = await BuildSlugMapAsync(stoppingToken);
        _logger.LogInformation("Slug map built with {Count} entries", slugMap.Count);

        _logger.LogInformation("Downloading all sprites...");
        await DownloadAllSpritesAsync(slugMap, stoppingToken);
        _logger.LogInformation("Sprite download completed");
    }

    private async Task<Dictionary<int, List<string>>> BuildSlugMapAsync(CancellationToken stoppingToken)
    {
        int offset = _options.Value.Offset;
        int limit = _options.Value.Limit;
        string cacheFile = Path.Combine(CacheDirectory, $"slug_map_{offset}_{limit}.json");

        // Check cache first
        if (_cache.TryGetValue(cacheFile, out string? cachedResponse))
        {
            _logger.LogDebug("Using memory-cached data for: {File}", cacheFile);
            return JsonSerializer.Deserialize<Dictionary<int, List<string>>>(cachedResponse)!;
        }

        // Check disk cache
        if (File.Exists(cacheFile))
        {
            _logger.LogDebug("Using disk-cached slugMap: {File}", cacheFile);
            string content = await File.ReadAllTextAsync(cacheFile, stoppingToken);
            _cache[cacheFile] = content;
            return JsonSerializer.Deserialize<Dictionary<int, List<string>>>(content)!;
        }

        var slugMap = new Dictionary<int, List<string>> { { 0, ["egg"] } };

        var (speciesMap, pokemonMap, forms) = await FetchPokemonDataAsync(stoppingToken);
        _logger.LogInformation("Processing {Count} species", speciesMap.Count);

        // Process each species - simplified approach
        foreach (var form in forms)
        {
            var pokemon = pokemonMap[form.Pokemon!.Name];
            var specie = speciesMap[pokemon.Species.Name];

            if (!slugMap.TryGetValue(specie.Id, out var slugs))
            {
                slugs = [];
                slugMap[specie.Id] = slugs;
            }

            slugs.Add((pokemon.IsDefault && pokemon.Name == form.Name) ||
                      (specie.Name == pokemon.Name && form.IsDefault)
                ? specie.Name
                : form.Name);
        }

        // Cache the slugMap for future use
        await File.WriteAllTextAsync(cacheFile, JsonSerializer.Serialize(slugMap), stoppingToken);

        return slugMap;
    }

    private async Task<(Dictionary<string, PokemonSpecies> SpeciesMap, Dictionary<string, Pokemon> PokemonMap,
            List<PokemonForm> Forms)>
        FetchPokemonDataAsync(CancellationToken stoppingToken)
    {
        int offset = _options.Value.Offset;
        int limit = _options.Value.Limit;

        _logger.LogInformation("Fetching species list with offset={Offset}, limit={Limit}", offset, limit);
        string speciesListJson = await FetchFromApiAsync(
            $"https://pokeapi.co/api/v2/pokemon-species?offset={offset}&limit={limit}", stoppingToken);
        var speciesList = JsonSerializer.Deserialize<NamedApiResourceList>(speciesListJson)!.Results;

        // Fetch details for each species
        _logger.LogInformation("Fetching species details for {Count} species", speciesList.Count);
        var speciesUrls = speciesList.Select(specie => specie.Url).ToList();
        var speciesJson =
            await ProcessInBatchesAsync(speciesUrls, url => FetchFromApiAsync(url, stoppingToken), BatchSize,
                stoppingToken);
        var speciesMap = speciesJson
            .Select(json => JsonSerializer.Deserialize<PokemonSpecies>(json)!)
            .ToDictionary(specie => specie.Name);

        // Fetch Pokémon details from the species varieties
        _logger.LogInformation("Fetching Pokemon details for {Count} Pokemon", speciesList.Count);
        var pokemonUrls = speciesMap.Values
            .SelectMany(specie => specie.Varieties!.Select(variant => variant.Pokemon.Url))
            .ToList();
        var pokemonJson =
            await ProcessInBatchesAsync(pokemonUrls, url => FetchFromApiAsync(url, stoppingToken), BatchSize,
                stoppingToken);
        var pokemonMap = pokemonJson
            .Select(json => JsonSerializer.Deserialize<Pokemon>(json)!)
            .ToDictionary(pokemon => pokemon.Name);

        // Fetch forms for each Pokémon
        _logger.LogInformation("Fetching forms for {Count} Pokemon", pokemonMap.Count);
        var formUrls = pokemonMap.Values
            .SelectMany(pokemon => pokemon.Forms.Select(form => form.Url))
            .ToList();
        var formJson = await ProcessInBatchesAsync(formUrls, url => FetchFromApiAsync(url, stoppingToken), BatchSize,
            stoppingToken);
        var forms = formJson
            .Select(json => JsonSerializer.Deserialize<PokemonForm>(json)!)
            .ToList();

        return (speciesMap, pokemonMap, forms);
    }

    private async Task<string> FetchFromApiAsync(string url, CancellationToken cancellationToken)
    {
        // Check cache first
        if (_cache.TryGetValue(url, out string? cachedResponse))
        {
            _logger.LogDebug("Using memory-cached data for: {Url}", url);
            return cachedResponse;
        }

        // Create disk cache filename
        string sanitizedUrl = string.Join("_", url.Split(Path.GetInvalidFileNameChars()));
        string cacheFile = Path.Combine(CacheDirectory, $"{sanitizedUrl}.json");

        // Check disk cache
        if (File.Exists(cacheFile))
        {
            _logger.LogDebug("Using disk-cached data for: {Url}", url);
            string content = await File.ReadAllTextAsync(cacheFile, cancellationToken);
            _cache[url] = content;
            return content;
        }

        // Fetch from API
        _logger.LogDebug("Fetching from API: {Url}", url);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        string apiContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Save to cache
        await File.WriteAllTextAsync(cacheFile, apiContent, cancellationToken);
        _cache[url] = apiContent;

        // Be nice to the API
        await Task.Delay(100, cancellationToken);

        return apiContent;
    }

    /// <summary>
    ///     Processes a collection of items in batches with progress reporting
    /// </summary>
    private async Task<List<TResult>> ProcessInBatchesAsync<TItem, TResult>(
        IEnumerable<TItem> items,
        Func<TItem, Task<TResult>> processor,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();
        var results = new List<TResult>(itemsList.Count);
        int totalBatches = (itemsList.Count + batchSize - 1) / batchSize;

        for (int i = 0; i < itemsList.Count; i += batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int currentBatch = i / batchSize + 1;
            int itemsInBatch = Math.Min(batchSize, itemsList.Count - i);
            int processedItems = Math.Min(i + itemsInBatch, itemsList.Count);

            _logger.LogInformation(
                "Processing batch {CurrentBatch}/{TotalBatches} ({ProcessedItems}/{TotalItems} items)", currentBatch,
                totalBatches, processedItems, itemsList.Count);

            var batch = itemsList.Skip(i).Take(batchSize).ToList();
            var batchTasks = batch.Select(processor);
            var batchResults = await Task.WhenAll(batchTasks);

            results.AddRange(batchResults);
        }

        return results;
    }

    private List<DownloadItem> CreateDownloadItems(Dictionary<int, List<string>> slugMap)
    {
        int offset = _options.Value.Offset;
        int limit = _options.Value.Limit;
        bool bruteForce = _options.Value.BruteForce;

        var result = Enumerable.Range(offset, limit)
            .Where(slugMap.ContainsKey)
            .SelectMany(dexId =>
                Enumerable.Range(0, bruteForce ? 100 : slugMap[dexId].Count)
                    .SelectMany(formId =>
                        Enumerable.Range(0, bruteForce ? 10 : 1)
                            .Select(styleId => new DownloadItem(dexId, formId, styleId))
                    )
            )
            .ToList();

        return result;
    }

    private async Task DownloadAllSpritesAsync(
        Dictionary<int, List<string>> slugMap,
        CancellationToken stoppingToken)
    {
        var downloadItems = CreateDownloadItems(slugMap);
        _logger.LogInformation("Created {Count} download items", downloadItems.Count);

        // Use the universal batch processing method with void result (using Task<bool>)
        await ProcessInBatchesAsync(
            downloadItems,
            async item =>
            {
                await DownloadSpriteAsync(
                    item.DexNum, item.FormNum, item.StyleNum, slugMap, stoppingToken);
                return true;
            },
            BatchSize,
            stoppingToken);
    }

    private async Task DownloadSpriteAsync(
        int dexNum, int formNum, int styleNum,
        Dictionary<int, List<string>> slugMap,
        CancellationToken cancellationToken)
    {
        string dexId = dexNum.ToString("D4");
        string formId = formNum.ToString("D2");
        string styleId = styleNum.ToString("D1");

        string imageUrl =
            $"https://resource.pokemon-home.com/battledata/img/pokei128/icon{dexId}_f{formId}_s{styleId}.png";

        string formSlug = formNum < slugMap[dexNum].Count ? slugMap[dexNum][formNum] : formId;
        string fileName = formNum == 0
            ? Path.Combine(SpritesDirectory, $"sprite_{dexId}_s{styleId}.webp")
            : Path.Combine(SpritesDirectory, $"sprite_{dexId}_{formSlug}_s{styleId}.webp");

        // Skip if file exists
        if (File.Exists(fileName))
            return;

        try
        {
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                byte[] imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                // await File.WriteAllBytesAsync(fileName, imageData, cancellationToken);
                await _imageConverter.SaveAsAsync(fileName, imageData, cancellationToken: cancellationToken);
                _logger.LogInformation("Downloaded {FileName}", fileName);
            }
        }
        catch
        {
            _logger.LogTrace("Failed to download {FileName}", fileName);
        }
    }

    private record DownloadItem(int DexNum, int FormNum, int StyleNum);
}
