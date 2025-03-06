using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PokemonSpritesDump.Converters;
using PokemonSpritesDump.Models;

namespace PokemonSpritesDump.Services;

public class SpriteDownloader : BackgroundService
{
    private const string SpritesDirectory = "out/sprites";
    private const string CacheDirectory = "out/cache";
    private const int BatchSize = 20;
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private readonly ILogger<SpriteDownloader> _logger;
    private readonly IOptions<ApiOptions> _apiOptions;
    private readonly IOptions<ImageOptions> _imageOptions;
    private readonly HttpClient _httpClient;
    private readonly IImageConverter _imageConverter;

    public SpriteDownloader(
        ILogger<SpriteDownloader> logger,
        IOptions<ApiOptions> apiOptions,
        IOptions<ImageOptions> imageOptions,
        HttpClient httpClient,
        IImageConverter imageConverter
    )
    {
        _logger = logger;
        _apiOptions = apiOptions;
        _imageOptions = imageOptions;
        _httpClient = httpClient;
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

    private async Task<Dictionary<int, List<string>>> BuildSlugMapAsync(
        CancellationToken stoppingToken
    )
    {
        var offset = _apiOptions.Value.Offset;
        var limit = _apiOptions.Value.Limit;
        var cacheFile = Path.Combine(CacheDirectory, $"slug_map_{offset}_{limit}.json");

        // Check cache
        var content = await GetCacheAsync(cacheFile, stoppingToken);
        if (!string.IsNullOrEmpty(content))
            return JsonSerializer.Deserialize<Dictionary<int, List<string>>>(content)!;

        var slugMap = new Dictionary<int, List<string>> { { 0, ["egg"] } };

        var (speciesMap, pokemonMap, forms) = await FetchPokemonDataAsync(stoppingToken);
        _logger.LogInformation("Processing {Count} species", speciesMap.Count);

        // Process each species - simplified approach
        foreach (var form in forms)
        {
            var pokemon = pokemonMap[form.Pokemon!.Name!];
            var specie = speciesMap[pokemon.Species!.Name!];

            if (!slugMap.TryGetValue(specie.Id, out var slugs))
            {
                slugs = [];
                slugMap[specie.Id] = slugs;
            }

            slugs.Add(
                (
                    ((bool)pokemon.IsDefault! && pokemon.Name == form.Name)
                    || (specie.Name == pokemon.Name && (bool)form.IsDefault!)
                        ? specie.Name
                        : form.Name
                )!
            );
        }

        // Write cache
        await WriteCacheAsync(cacheFile, JsonSerializer.Serialize(slugMap), stoppingToken);

        return slugMap;
    }

    private async Task<(
        Dictionary<string, PokemonSpecies> SpeciesMap,
        Dictionary<string, Pokemon> PokemonMap,
        List<PokemonForm> Forms
    )> FetchPokemonDataAsync(CancellationToken stoppingToken)
    {
        var offset = _apiOptions.Value.Offset;
        var limit = _apiOptions.Value.Limit;

        _logger.LogInformation(
            "Fetching species list with offset={Offset}, limit={Limit}",
            offset,
            limit
        );
        var speciesListJson = await FetchFromApiAsync(
            $"https://pokeapi.co/api/v2/pokemon-species?offset={offset}&limit={limit}",
            stoppingToken
        );
        var speciesList = JsonSerializer
            .Deserialize<NamedApiResourceList>(speciesListJson)!
            .Results;

        // Fetch details for each species
        _logger.LogInformation("Fetching species details for {Count} species", speciesList!.Count);
        var speciesUrls = speciesList.Select(specie => specie.Url).ToList();
        var speciesJson = await ProcessInBatchesAsync(
            speciesUrls,
            url => FetchFromApiAsync(url!, stoppingToken),
            BatchSize,
            stoppingToken
        );
        var speciesMap = speciesJson
            .Select(json => JsonSerializer.Deserialize<PokemonSpecies>(json)!)
            .ToDictionary(specie => specie.Name!);

        // Fetch Pokémon details from the species varieties
        _logger.LogInformation("Fetching Pokemon details for {Count} Pokemon", speciesList.Count);
        var pokemonUrls = speciesMap
            .Values.SelectMany(specie => specie.Varieties!.Select(variant => variant.Pokemon!.Url))
            .ToList();
        var pokemonJson = await ProcessInBatchesAsync(
            pokemonUrls,
            url => FetchFromApiAsync(url!, stoppingToken),
            BatchSize,
            stoppingToken
        );
        var pokemonMap = pokemonJson
            .Select(json => JsonSerializer.Deserialize<Pokemon>(json)!)
            .ToDictionary(pokemon => pokemon.Name!);

        // Fetch forms for each Pokémon
        _logger.LogInformation("Fetching forms for {Count} Pokemon", pokemonMap.Count);
        var formUrls = pokemonMap
            .Values.SelectMany(pokemon => pokemon.Forms!.Select(form => form.Url))
            .ToList();
        var formJson = await ProcessInBatchesAsync(
            formUrls,
            url => FetchFromApiAsync(url!, stoppingToken),
            BatchSize,
            stoppingToken
        );
        var forms = formJson
            .Select(json => JsonSerializer.Deserialize<PokemonForm>(json)!)
            .ToList();

        return (speciesMap, pokemonMap, forms);
    }

    private async Task<string> FetchFromApiAsync(string url, CancellationToken stoppingToken)
    {
        var sanitizedUrl = string.Join("_", url.Split(Path.GetInvalidFileNameChars()));
        var cacheFile = Path.Combine(CacheDirectory, $"{sanitizedUrl}.json");

        // Check cache
        var content = await GetCacheAsync(cacheFile, stoppingToken);
        if (!string.IsNullOrEmpty(content))
            return content;

        // Fetch from API
        _logger.LogDebug("Fetching from API: {Url}", url);
        var response = await _httpClient.GetAsync(url, stoppingToken);
        response.EnsureSuccessStatusCode();

        var apiContent = await response.Content.ReadAsStringAsync(stoppingToken);

        // Write cache
        await WriteCacheAsync(cacheFile, apiContent, stoppingToken);

        // Be nice to the API
        await Task.Delay(100, stoppingToken);

        return apiContent;
    }

    /// <summary>
    ///     Processes a collection of items in batches with progress reporting
    /// </summary>
    private async Task<List<TResult>> ProcessInBatchesAsync<TItem, TResult>(
        IEnumerable<TItem> items,
        Func<TItem, Task<TResult>> processor,
        int batchSize,
        CancellationToken stoppingToken = default
    )
    {
        var itemsList = items.ToList();
        var results = new List<TResult>(itemsList.Count);
        var totalBatches = (itemsList.Count + batchSize - 1) / batchSize;

        for (var i = 0; i < itemsList.Count; i += batchSize)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var currentBatch = i / batchSize + 1;
            var itemsInBatch = Math.Min(batchSize, itemsList.Count - i);
            var processedItems = Math.Min(i + itemsInBatch, itemsList.Count);

            _logger.LogInformation(
                "Processing batch {CurrentBatch}/{TotalBatches} ({ProcessedItems}/{TotalItems} items)",
                currentBatch,
                totalBatches,
                processedItems,
                itemsList.Count
            );

            var batch = itemsList.Skip(i).Take(batchSize).ToList();
            var batchTasks = batch.Select(processor);
            var batchResults = await Task.WhenAll(batchTasks);

            results.AddRange(batchResults);
        }

        return results;
    }

    private List<DownloadItem> CreateDownloadItems(Dictionary<int, List<string>> slugMap)
    {
        var offset = _apiOptions.Value.Offset;
        var limit = _apiOptions.Value.Limit;
        var bruteForce = _apiOptions.Value.BruteForce;

        var result = Enumerable
            .Range(offset, limit)
            .Where(slugMap.ContainsKey)
            .SelectMany(dexId =>
                Enumerable
                    .Range(0, bruteForce ? 100 : slugMap[dexId].Count)
                    .SelectMany(formId =>
                        Enumerable
                            .Range(0, bruteForce ? 10 : 1)
                            .Select(styleId => new DownloadItem(dexId, formId, styleId))
                    )
            )
            .ToList();

        return result;
    }

    private async Task DownloadAllSpritesAsync(
        Dictionary<int, List<string>> slugMap,
        CancellationToken stoppingToken
    )
    {
        var downloadItems = CreateDownloadItems(slugMap);
        _logger.LogInformation("Created {Count} download items", downloadItems.Count);

        // Use the universal batch processing method with void result (using Task<bool>)
        await ProcessInBatchesAsync(
            downloadItems,
            async item =>
            {
                await DownloadSpriteAsync(
                    item.DexNum,
                    item.FormNum,
                    item.StyleNum,
                    slugMap,
                    stoppingToken
                );
                return true;
            },
            BatchSize,
            stoppingToken
        );
    }

    private async Task DownloadSpriteAsync(
        int dexNum,
        int formNum,
        int styleNum,
        Dictionary<int, List<string>> slugMap,
        CancellationToken stoppingToken
    )
    {
        var dexId = dexNum.ToString("D4");
        var formId = formNum.ToString("D2");
        var styleId = styleNum.ToString("D1");

        var imageUrl =
            $"https://resource.pokemon-home.com/battledata/img/pokei128/icon{dexId}_f{formId}_s{styleId}.png";
        var imageCacheFile = Path.Combine(CacheDirectory, $"icon{dexId}_f{formId}_s{styleId}.png");

        var formSlug = formNum < slugMap[dexNum].Count ? slugMap[dexNum][formNum] : formId;
        var extension = _imageConverter.GetFileExtension();
        var fileName =
            formNum == 0
                ? Path.Combine(SpritesDirectory, $"sprite_{dexId}_s{styleId}{extension}")
                : Path.Combine(
                    SpritesDirectory,
                    $"sprite_{dexId}_{formSlug}_s{styleId}{extension}"
                );

        if (File.Exists(fileName))
        {
            _logger.LogDebug("File already exists, skipping: {FileName}", fileName);
            return;
        }

        try
        {
            byte[] imageData;

            // Check if image exists in cache
            var cachedImage = await GetCacheAsync(imageCacheFile, stoppingToken);
            if (!string.IsNullOrEmpty(cachedImage))
            {
                _logger.LogDebug("Using cached image: {CacheFile}", imageCacheFile);
                imageData = Convert.FromBase64String(cachedImage);
            }
            else
            {
                // Download the image
                var response = await _httpClient.GetAsync(imageUrl, stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogTrace(
                        "Failed to download image: {Url} (Status: {Status})",
                        imageUrl,
                        response.StatusCode
                    );
                    return;
                }

                imageData = await response.Content.ReadAsByteArrayAsync(stoppingToken);

                // Cache the downloaded image
                await WriteCacheAsync(
                    imageCacheFile,
                    Convert.ToBase64String(imageData),
                    stoppingToken
                );
                _logger.LogDebug("Cached image: {CacheFile}", imageCacheFile);
            }

            // Convert and save as WebP
            var quality = _imageOptions.Value.Quality;
            var lossless = _imageOptions.Value.Lossless;
            await _imageConverter.SaveAsAsync(
                fileName,
                imageData,
                quality,
                lossless,
                stoppingToken: stoppingToken
            );
            _logger.LogInformation("Processed {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {FileName}", fileName);
        }
    }

    private async Task<string> GetCacheAsync(string path, CancellationToken stoppingToken)
    {
        // Check memory cache
        if (_cache.TryGetValue(path, out var cachedResponse))
        {
            _logger.LogDebug("Using memory-cached data for: {File}", path);
            return cachedResponse;
        }

        // Check disk cache
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        _logger.LogDebug("Using disk-cached data for: {File}", path);
        var content = await File.ReadAllTextAsync(path, stoppingToken);
        _cache[path] = content;

        return content;
    }

    private async Task WriteCacheAsync(string path, string content, CancellationToken stoppingToken)
    {
        _cache[path] = content;
        await File.WriteAllTextAsync(path, content, stoppingToken);
    }

    private record DownloadItem(int DexNum, int FormNum, int StyleNum);
}
