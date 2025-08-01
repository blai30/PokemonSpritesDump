using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PokemonSpritesDump.Converters;
using PokemonSpritesDump.Models;

namespace PokemonSpritesDump.Services;

public class SpriteDownloader : BackgroundService
{
    private const string SpritesDirectory = "out/sprites";
    private const string ItemsDirectory = "out/items";
    private const string CacheDirectory = "out/cache";
    private const int BatchSize = 20;
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private readonly ILogger<SpriteDownloader> _logger;
    private readonly IOptions<ApiOptions> _apiOptions;
    private readonly IOptions<ImageOptions> _imageOptions;
    private readonly HttpClient _httpClient;
    private readonly IImageConverter _imageConverter;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    // id to species
    private Dictionary<int, PokemonSpecies> _speciesIdMap = [];

    // slug to species
    private Dictionary<string, PokemonSpecies> _speciesMap = [];

    // pokemon slug to pokemon
    private Dictionary<string, Pokemon> _pokemonMap = [];

    // form slug to pokemon form
    private Dictionary<string, PokemonForm> _forms = [];

    // species slug to pokemon list
    private Dictionary<string, List<Pokemon>> _speciesPokemonMap = [];

    // pokemon slug to form list
    private Dictionary<string, List<PokemonForm>> _pokemonFormsMap = [];

    private List<Task> _downloadTasks = [];

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
        Directory.CreateDirectory(ItemsDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await FetchPokemonDataAsync(stoppingToken);
        _logger.LogInformation("Starting sprite download process");
        var slugMap = BuildSlugMapAsync(stoppingToken);

        _logger.LogInformation("Downloading all sprites...");
        await DownloadAllSpritesAsync(slugMap, stoppingToken);
        _logger.LogInformation("Sprite download completed");

        _logger.LogInformation("Downloading items...");
        var itemSlugs = MapSorter.ItemSlugs;
        await DownloadItemsAsync(itemSlugs, stoppingToken);
        _logger.LogInformation("Item download completed");

        // _logger.LogInformation("Downloading images with original filenames...");
        // await DownloadAllOriginalSpritesAsync(stoppingToken);
        // await DownloadOriginalItemsAsync(stoppingToken);
    }

    private Dictionary<int, List<string>> BuildSlugMapAsync(CancellationToken stoppingToken)
    {
        // Build a map with dex number as key and a list of form slugs as value.
        var slugMap = new Dictionary<int, List<string>> { { 0, ["egg"] } };
        foreach (var form in _forms.Values)
        {
            var pokemon = _pokemonMap[form.Pokemon!.Name!];
            var species = _speciesMap[pokemon.Species!.Name!];
            var dexNum = species.Id;

            if (!slugMap.ContainsKey(dexNum))
            {
                slugMap[dexNum] = [];
            }

            // if ((bool)pokemon.IsDefault! && (bool)form.IsDefault!)
            // {
            //     slugMap[dexNum].Add(species.Name!);
            // }
            // else
            // {
            slugMap[dexNum].Add(form.Name!);
            // }
        }

        // Use MapSorter.SlugOrdering to ensure correct order of applicable form slugs.
        foreach (var (dexNum, slugs) in MapSorter.PokemonSlugs)
        {
            if (slugMap.TryGetValue(dexNum, out var existingSlugs))
            {
                // Ensure the order of slugs matches the predefined order
                slugMap[dexNum] = slugs.ToList();
            }
        }

        // Write the slug map to a file for debugging
        var slugMapJson = JsonSerializer.Serialize(slugMap, _jsonSerializerOptions);
        File.WriteAllText("out/slugMap.json", slugMapJson);

        _logger.LogInformation("Slug map built with {Count} entries", slugMap.Count);
        return slugMap;
    }

    private async Task FetchPokemonDataAsync(CancellationToken stoppingToken)
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
        _speciesMap = speciesJson
            .Select(json => JsonSerializer.Deserialize<PokemonSpecies>(json)!)
            .ToDictionary(specie => specie.Name!);

        _speciesIdMap = _speciesMap
            .ToDictionary(specie => specie.Value.Id, specie => specie.Value);

        // Fetch Pokémon details from the species varieties
        _logger.LogInformation("Fetching Pokemon details for {Count} Pokemon", speciesList.Count);
        var pokemonUrls = _speciesMap
            .Values.SelectMany(specie => specie.Varieties!.Select(variant => variant.Pokemon!.Url))
            .ToList();
        var pokemonJson = await ProcessInBatchesAsync(
            pokemonUrls,
            url => FetchFromApiAsync(url!, stoppingToken),
            BatchSize,
            stoppingToken
        );
        _pokemonMap = pokemonJson
            .Select(json => JsonSerializer.Deserialize<Pokemon>(json)!)
            .ToDictionary(pokemon => pokemon.Name!);

        // Fetch forms for each Pokémon
        _logger.LogInformation("Fetching forms for {Count} Pokemon", _pokemonMap.Count);
        var formUrls = _pokemonMap
            .Values.SelectMany(pokemon => pokemon.Forms!.Select(form => form.Url))
            .ToList();
        var formJson = await ProcessInBatchesAsync(
            formUrls,
            url => FetchFromApiAsync(url!, stoppingToken),
            BatchSize,
            stoppingToken
        );
        _forms = formJson
            .Select(json => JsonSerializer.Deserialize<PokemonForm>(json)!)
            .ToDictionary(form => form.Name!);

        _speciesPokemonMap = _speciesMap
            .GroupBy(specie => specie.Value.Name!)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(specie => specie.Value.Varieties!)
                    .Select(variant => _pokemonMap[variant.Pokemon!.Name!])
                    .ToList()
            );

        _pokemonFormsMap = _forms.Values
            .GroupBy(form => form.Pokemon!.Name!)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        // Write maps to JSON files for debugging
        // await File.WriteAllTextAsync(
        //     "out/speciesMap.json",
        //     JsonSerializer.Serialize(_speciesMap, _jsonSerializerOptions),
        //     stoppingToken
        // );
        //
        // await File.WriteAllTextAsync(
        //     "out/speciesIdMap.json",
        //     JsonSerializer.Serialize(_speciesIdMap, _jsonSerializerOptions),
        //     stoppingToken
        // );
        //
        // await File.WriteAllTextAsync(
        //     "out/pokemonMap.json",
        //     JsonSerializer.Serialize(_pokemonMap, _jsonSerializerOptions),
        //     stoppingToken
        // );
        //
        // await File.WriteAllTextAsync(
        //     "out/forms.json",
        //     JsonSerializer.Serialize(_forms, _jsonSerializerOptions),
        //     stoppingToken
        // );
        //
        // await File.WriteAllTextAsync(
        //     "out/speciesPokemonMap.json",
        //     JsonSerializer.Serialize(_speciesPokemonMap, _jsonSerializerOptions),
        //     stoppingToken
        // );
        //
        // await File.WriteAllTextAsync(
        //     "out/pokemonFormsMap.json",
        //     JsonSerializer.Serialize(_pokemonFormsMap, _jsonSerializerOptions),
        //     stoppingToken
        // );
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

    private async Task DownloadAllSpritesAsync(Dictionary<int, List<string>> slugMap, CancellationToken stoppingToken)
    {
        for (var i = slugMap.Keys.Min(); i <= slugMap.Keys.Max(); i++)
        {
            stoppingToken.ThrowIfCancellationRequested();
            if (!slugMap.TryGetValue(i, out var forms))
            {
                _logger.LogWarning("No forms found for dex number {DexNum}", i);
                continue;
            }

            for (var j = 0; j < forms.Count; j++)
            {
                stoppingToken.ThrowIfCancellationRequested();
                var formSlug = forms[j];
                var form = formSlug != "egg" ? _forms[formSlug] : null;
                var pokemon = formSlug != "egg" ? _pokemonMap[form!.Pokemon!.Name!] : null;
                var isDefault = formSlug == "egg" || (bool)pokemon!.IsDefault! && (bool)form!.IsDefault!;

                for (var k = 0; k < 10; k++)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    _downloadTasks.Add(DownloadSpriteAsync(
                        isDefault,
                        i,
                        j,
                        k,
                        slugMap,
                        stoppingToken
                    ));
                }
            }
        }

        await Task.WhenAll(_downloadTasks);
    }

    private async Task DownloadAllOriginalSpritesAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory("out/original-sprites");
        var spriteUrls = new List<(string imageUrl, string cacheFile, string fileName)>();

        for (var i = 0; i < 1040; i++)
        {
            for (var j = 0; j < 30; j++)
            {
                for (var k = 0; k < 4; k++)
                {
                    var imageUrl =
                        $"https://resource.pokemon-home.com/battledata/img/pokei128/icon{i:D4}_f{j:D2}_s{k:D1}.png";
                    var imageCacheFile = Path.Combine(CacheDirectory, $"icon{i:D4}_f{j:D2}_s{k:D1}.png");
                    var extension = _imageConverter.GetFileExtension();
                    var fileName = Path.Combine("out/original-sprites", $"icon{i:D4}_f{j:D2}_s{k:D1}{extension}");
                    spriteUrls.Add((imageUrl, imageCacheFile, fileName));
                }
            }
        }

        await ProcessInBatchesAsync(spriteUrls, async item =>
        {
            await DownloadAndProcessImageAsync(item.imageUrl, item.cacheFile, item.fileName, stoppingToken);
            return true;
        }, BatchSize, stoppingToken);
    }

    private async Task DownloadSpriteAsync(
        bool isDefault,
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
            // formNum == 0
            isDefault
                ? Path.Combine(SpritesDirectory, $"sprite_{dexId}_s{styleId}{extension}")
                : Path.Combine(
                    SpritesDirectory,
                    $"sprite_{dexId}_{formSlug}_s{styleId}{extension}"
                );

        await DownloadAndProcessImageAsync(imageUrl, imageCacheFile, fileName, stoppingToken);
    }

    private async Task DownloadItemsAsync(Dictionary<int, List<string>> slugs, CancellationToken stoppingToken)
    {
        // Use your batch processor to download items in parallel batches
        await ProcessInBatchesAsync(slugs.Keys, async i =>
        {
            await Process(i);
            return true;
        }, BatchSize, stoppingToken);
        return;

        // Create per-item processing function
        async Task Process(int i)
        {
            var url = $"https://resource.pokemon-home.com/battledata/img/item/item_{i:D4}.png";
            var cacheFile = Path.Combine(CacheDirectory, $"item_{i:D4}.png");

            foreach (var fileName in slugs[i].Select(slug => Path.Combine(ItemsDirectory, $"item_{slug}.webp")))
            {
                await DownloadAndProcessImageAsync(url, cacheFile, fileName, stoppingToken);
            }
        }
    }

    private async Task DownloadOriginalItemsAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory("out/original-items");
        // Use your batch processor to download items in parallel batches
        var ids = Enumerable.Range(0, 9999);
        await ProcessInBatchesAsync(ids, async i =>
        {
            await Process(i);
            return true;
        }, BatchSize, stoppingToken);
        return;

        // Create per-item processing function
        async Task Process(int i)
        {
            var url = $"https://resource.pokemon-home.com/battledata/img/item/item_{i:D4}.png";
            var cacheFile = Path.Combine(CacheDirectory, $"item_{i:D4}.png");
            var fileName = Path.Combine("out/original-items", $"item_{i:D4}.png");
            await DownloadAndProcessImageAsync(url, cacheFile, fileName, stoppingToken);
        }
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

    private async Task DownloadAndProcessImageAsync(
        string imageUrl,
        string imageCacheFile,
        string fileName,
        CancellationToken stoppingToken)
    {
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
                using var response = await _httpClient.GetAsync(imageUrl, stoppingToken);

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
}
