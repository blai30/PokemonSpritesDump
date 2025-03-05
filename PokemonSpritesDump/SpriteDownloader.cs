using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PokemonSpritesDump.Models;

namespace PokemonSpritesDump;

/// <summary>
/// A background service that downloads Pokémon sprites from Pokémon HOME API.
/// Manages caching of API responses and image files to minimize redundant network requests.
/// </summary>
public class SpriteDownloader : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpriteDownloader> _logger;
    private readonly ConcurrentDictionary<string, string> _memoryCache = new();
    private readonly IOptions<ApiOptions> _options;

    // Stats tracking
    private int _downloadedCount;
    private int _failedCount;
    private int _skippedCount;

    private const int DefaultBatchSize = 30;
    private const string SpritesDirectory = "out/sprites";
    private const string CacheDirectory = "out/cache";

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteDownloader"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording operation information and errors.</param>
    /// <param name="httpClient">HTTP client for API requests and sprite downloads.</param>
    /// <param name="options">Configuration options for the download process.</param>
    public SpriteDownloader(ILogger<SpriteDownloader> logger, HttpClient httpClient, IOptions<ApiOptions> options)
    {
        _logger = logger;
        _httpClient = httpClient;
        _options = options;

        // Configure the HttpClient
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        // Ensure directories exist
        Directory.CreateDirectory(SpritesDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }

    /// <summary>
    /// Executes the sprite download process as a background service.
    /// Builds a slug map and downloads all sprites according to configuration.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the background service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting sprite download service at: {time}", DateTimeOffset.Now);

            var config = new DownloadConfig(
                Offset: _options.Value.Offset ?? 0,
                Limit: _options.Value.Limit ?? 1025,
                BruteForce: _options.Value.BruteForce ?? false
            );

            var slugMap = await BuildSlugMapAsync(config, stoppingToken);
            await DownloadAllSpritesAsync(slugMap, config, stoppingToken);

            _logger.LogInformation(
                "Download complete. Downloaded: {Downloaded}, Skipped: {Skipped}, Failed: {Failed}",
                _downloadedCount, _skippedCount, _failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in sprite download service");
        }
    }

    /// <summary>
    /// Builds a mapping of Pokémon IDs to form slugs used in sprite naming.
    /// </summary>
    /// <param name="config">Download configuration settings.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A dictionary mapping Pokémon IDs to their form slug names.</returns>
    private async Task<Dictionary<int, List<string>>> BuildSlugMapAsync(DownloadConfig config,
        CancellationToken stoppingToken)
    {
        // Initialize slug map
        var slugMap = new Dictionary<int, List<string>> { { 0, ["egg"] } };

        var speciesList = await FetchSpeciesListAsync(config, stoppingToken);

        // Fetch and process all required data
        var (species, pokemons, forms) = await FetchPokemonDataAsync(speciesList, stoppingToken);

        // Build slug map from the fetched data
        ProcessFormsIntoSlugMap(species, pokemons, forms, slugMap);

        // Cache the slugMap for future use
        await CacheSlugMapAsync(slugMap, config, stoppingToken);

        return slugMap;
    }

    /// <summary>
    /// Fetches the list of Pokémon species from the PokeAPI.
    /// </summary>
    /// <param name="config">Download configuration defining offset and limit.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A list of Pokémon species resources.</returns>
    private async Task<List<NamedApiResource>> FetchSpeciesListAsync(DownloadConfig config,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fetching Pokemon species list with offset={offset} and limit={limit}",
            config.Offset, config.Limit);

        string speciesListJson = await GetCachedApiResponseAsync(
            $"https://pokeapi.co/api/v2/pokemon-species?offset={config.Offset}&limit={config.Limit}", stoppingToken);

        return JsonSerializer.Deserialize<NamedApiResourceList>(speciesListJson)!.Results;
    }

    /// <summary>
    /// Fetches detailed Pokémon data including species, base Pokémon data, and forms.
    /// </summary>
    /// <param name="speciesList">List of species to fetch data for.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A tuple containing mappings of species, Pokémon, and a list of forms.</returns>
    private async Task<(Dictionary<string, PokemonSpecies> Species,
        Dictionary<string, Pokemon> Pokemons,
        List<PokemonForm> Forms)> FetchPokemonDataAsync(
        List<NamedApiResource> speciesList,
        CancellationToken stoppingToken)
    {
        // Fetch species data
        _logger.LogInformation("Fetching Pokemon species details...");
        var speciesUrls = speciesList.Select(species => species.Url).ToList();
        var species = await ProcessInBatchesAsync<PokemonSpecies>(speciesUrls,
            url => GetCachedApiResponseAsync(url, stoppingToken),
            cancellationToken: stoppingToken);

        var speciesMap = species.ToDictionary(s => s.Name);

        // Fetch Pokemon data
        var pokemonUrls = species.SelectMany(s => s.Varieties!.Select(v => v.Pokemon.Url)).ToList();
        var pokemons = await ProcessInBatchesAsync<Pokemon>(pokemonUrls,
            url => GetCachedApiResponseAsync(url, stoppingToken),
            cancellationToken: stoppingToken);

        var pokemonMap = pokemons.ToDictionary(p => p.Name);

        // Fetch form data
        var formUrls = pokemons.SelectMany(p => p.Forms.Select(f => f.Url)).ToList();
        var forms = await ProcessInBatchesAsync<PokemonForm>(formUrls,
            url => GetCachedApiResponseAsync(url, stoppingToken),
            cancellationToken: stoppingToken);

        return (speciesMap, pokemonMap, forms);
    }

    /// <summary>
    /// Processes Pokémon form data to build a comprehensive slug map for sprite naming.
    /// </summary>
    /// <param name="speciesMap">Dictionary mapping species names to their data.</param>
    /// <param name="pokemonMap">Dictionary mapping Pokémon names to their data.</param>
    /// <param name="forms">List of Pokémon form data.</param>
    /// <param name="slugMap">Dictionary to populate with ID to slug mappings.</param>
    private void ProcessFormsIntoSlugMap(
        Dictionary<string, PokemonSpecies> speciesMap,
        Dictionary<string, Pokemon> pokemonMap,
        List<PokemonForm> forms,
        Dictionary<int, List<string>> slugMap)
    {
        foreach (var form in forms)
        {
            var pokemon = pokemonMap[form.Pokemon!.Name];
            var speciesData = speciesMap[pokemon.Species.Name];

            string formName = DetermineFormName(speciesData, pokemon, form);

            if (slugMap.TryGetValue(speciesData.Id, out var slugs))
            {
                slugs.Add(formName);
            }
            else
            {
                slugMap[speciesData.Id] = [formName];
            }
        }
    }

    /// <summary>
    /// Determines the appropriate form name based on species, Pokémon, and form data.
    /// </summary>
    /// <param name="species">The species data.</param>
    /// <param name="pokemon">The Pokémon data.</param>
    /// <param name="form">The form data.</param>
    /// <returns>The appropriate form name to use in sprite filenames.</returns>
    private string DetermineFormName(PokemonSpecies species, Pokemon pokemon, PokemonForm form)
    {
        bool isDefaultForm = (pokemon.IsDefault && pokemon.Name == form.Name) ||
                             (species.Name == pokemon.Name && form.IsDefault);

        return isDefaultForm ? species.Name : form.Name;
    }

    /// <summary>
    /// Caches the generated slug map to disk for future use.
    /// </summary>
    /// <param name="slugMap">The slug mapping to cache.</param>
    /// <param name="config">Download configuration containing offset and limit info.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CacheSlugMapAsync(
        Dictionary<int, List<string>> slugMap,
        DownloadConfig config,
        CancellationToken stoppingToken)
    {
        string cacheFilePath = Path.Combine(CacheDirectory, $"slug_map_{config.Offset}_{config.Limit}.json");
        await File.WriteAllTextAsync(
            cacheFilePath,
            JsonSerializer.Serialize(slugMap, new JsonSerializerOptions { WriteIndented = true }),
            stoppingToken);
    }

    /// <summary>
    /// Coordinates the download of all sprite images based on the slug map.
    /// </summary>
    /// <param name="slugMap">Mapping of Pokémon IDs to form slugs.</param>
    /// <param name="config">Download configuration.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DownloadAllSpritesAsync(
        Dictionary<int, List<string>> slugMap,
        DownloadConfig config,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Preparing to download sprites...");

        // Reset counters
        _downloadedCount = 0;
        _skippedCount = 0;
        _failedCount = 0;

        // Create work items for download
        var downloadBatches = CreateDownloadBatches(slugMap, config);

        await ProcessDownloadBatchesAsync(downloadBatches, slugMap, stoppingToken);
    }

    /// <summary>
    /// Creates download work items based on the slug map and configuration.
    /// </summary>
    /// <param name="slugMap">Mapping of Pokémon IDs to form slugs.</param>
    /// <param name="config">Download configuration including offset, limit, and brute force settings.</param>
    /// <returns>A list of sprite download items to process.</returns>
    private List<SpriteDownloadItem> CreateDownloadBatches(
        Dictionary<int, List<string>> slugMap,
        DownloadConfig config)
    {
        var result = new List<SpriteDownloadItem>();

        for (int dexId = config.Offset; dexId < config.Offset + config.Limit + 1; dexId++)
        {
            if (!slugMap.TryGetValue(dexId, out var formSlugs))
                continue;

            int formCount = config.BruteForce ? 100 : Math.Min(formSlugs.Count, 100);
            int styleCount = config.BruteForce ? 10 : 1;

            for (int formId = 0; formId < formCount; formId++)
            {
                for (int styleId = 0; styleId < styleCount; styleId++)
                {
                    result.Add(new SpriteDownloadItem(dexId, formId, styleId));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Processes download items in batches to manage system resources.
    /// </summary>
    /// <param name="downloadItems">The list of items to download.</param>
    /// <param name="slugMap">Mapping of Pokémon IDs to form slugs.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessDownloadBatchesAsync(
        List<SpriteDownloadItem> downloadItems,
        Dictionary<int, List<string>> slugMap,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Downloading {count} sprites in batches...", downloadItems.Count);

        int totalBatches = (int)Math.Ceiling(downloadItems.Count / (double)DefaultBatchSize);

        for (int batchIndex = 0; batchIndex < totalBatches && !stoppingToken.IsCancellationRequested; batchIndex++)
        {
            int batchStart = batchIndex * DefaultBatchSize;
            int batchSize = Math.Min(DefaultBatchSize, downloadItems.Count - batchStart);

            var currentBatch = downloadItems
                .Skip(batchStart)
                .Take(batchSize)
                .ToList();

            await ProcessDownloadBatchAsync(currentBatch, slugMap, stoppingToken);

            _logger.LogInformation("Completed sprite batch {Current}/{Total} " +
                                   "(Downloaded: {Downloaded}, Skipped: {Skipped}, Failed: {Failed})",
                batchIndex + 1, totalBatches, _downloadedCount, _skippedCount, _failedCount);
        }
    }

    /// <summary>
    /// Processes a single batch of sprite download items.
    /// </summary>
    /// <param name="batch">The batch of items to download.</param>
    /// <param name="slugMap">Mapping of Pokémon IDs to form slugs.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessDownloadBatchAsync(
        List<SpriteDownloadItem> batch,
        Dictionary<int, List<string>> slugMap,
        CancellationToken stoppingToken)
    {
        var downloadTasks = batch.Select(item =>
            DownloadSpriteAsync(item.DexNum, item.FormNum, item.StyleNum, slugMap, stoppingToken));

        await Task.WhenAll(downloadTasks);
    }

    /// <summary>
    /// Downloads a single sprite image for a specific Pokémon form and style.
    /// </summary>
    /// <param name="dexNum">The Pokedex number of the Pokemon.</param>
    /// <param name="formNum">The form number/index.</param>
    /// <param name="styleNum">The style number/index.</param>
    /// <param name="slugDictionary">Dictionary mapping Pokémon IDs to form slugs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DownloadSpriteAsync(
        int dexNum, int formNum, int styleNum,
        Dictionary<int, List<string>> slugDictionary,
        CancellationToken cancellationToken)
    {
        try
        {
            string dexId = dexNum.ToString("D4");
            string formId = formNum.ToString("D2");
            string styleId = styleNum.ToString("D1");

            string imageUrl =
                $"https://resource.pokemon-home.com/battledata/img/pokei128/icon{dexId}_f{formId}_s{styleId}.png";
            string fileName = BuildSpriteFileName(dexId, formId, styleId, formNum, slugDictionary, dexNum);

            // Check if file already exists
            if (File.Exists(fileName))
            {
                Interlocked.Increment(ref _skippedCount);
                return;
            }

            byte[] imageBytes = await FetchImageBytesAsync(dexId, formId, styleId, imageUrl, cancellationToken);

            if (imageBytes.Length > 0)
            {
                await File.WriteAllBytesAsync(fileName, imageBytes, cancellationToken);
                Interlocked.Increment(ref _downloadedCount);
                _logger.LogInformation("Downloaded: {FileName}", Path.GetFileName(fileName));
            }
            else
            {
                Interlocked.Increment(ref _failedCount);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failedCount);
            _logger.LogError(ex, "Error downloading sprite {DexNum}/{FormNum}/{StyleNum}", dexNum, formNum, styleNum);
        }
    }

    /// <summary>
    /// Builds a filename for a sprite based on Pokémon data and form information.
    /// </summary>
    /// <param name="dexId">The formatted Pokedex ID.</param>
    /// <param name="formId">The formatted form ID.</param>
    /// <param name="styleId">The formatted style ID.</param>
    /// <param name="formNum">The form number/index.</param>
    /// <param name="slugDictionary">Dictionary mapping Pokémon IDs to form slugs.</param>
    /// <param name="dexNum">The Pokedex number.</param>
    /// <returns>The full path for the sprite file.</returns>
    private string BuildSpriteFileName(
        string dexId, string formId, string styleId,
        int formNum, Dictionary<int, List<string>> slugDictionary, int dexNum)
    {
        string formSlug = formNum < slugDictionary[dexNum].Count
            ? slugDictionary[dexNum][formNum]
            : formId;

        return formNum == 0
            ? Path.Combine(SpritesDirectory, $"sprite_{dexId}_s{styleId}.png")
            : Path.Combine(SpritesDirectory, $"sprite_{dexId}_{formSlug}_s{styleId}.png");
    }

    /// <summary>
    /// Fetches image bytes for a sprite, with caching support.
    /// </summary>
    /// <param name="dexId">The formatted Pokedex ID.</param>
    /// <param name="formId">The formatted form ID.</param>
    /// <param name="styleId">The formatted style ID.</param>
    /// <param name="imageUrl">The URL to fetch the image from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The image bytes or empty array if not found.</returns>
    private async Task<byte[]> FetchImageBytesAsync(
        string dexId, string formId, string styleId,
        string imageUrl, CancellationToken cancellationToken)
    {
        // Image cache file path
        string imageCacheFile = Path.Combine(CacheDirectory, $"icon{dexId}_f{formId}_s{styleId}.png");

        // Check image cache first
        if (File.Exists(imageCacheFile))
        {
            return await File.ReadAllBytesAsync(imageCacheFile, cancellationToken);
        }

        // Fetch from URL if not cached
        using var response = await _httpClient.GetAsync(imageUrl,
            HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Return empty array if not found (404) or other error
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        // Cache successful responses
        if (imageBytes.Length > 0)
        {
            await File.WriteAllBytesAsync(imageCacheFile, imageBytes, cancellationToken);
        }

        return imageBytes;
    }

    /// <summary>
    /// Gets API response with caching (memory and disk).
    /// </summary>
    /// <param name="url">The API URL to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API response as a string.</returns>
    private async Task<string> GetCachedApiResponseAsync(string url, CancellationToken cancellationToken = default)
    {
        // Check memory cache first
        if (_memoryCache.TryGetValue(url, out string? cachedResponse))
        {
            _logger.LogDebug("Using memory-cached data for: {Url}", url);
            return cachedResponse;
        }

        // Create a filename from the URL (removing invalid characters)
        string sanitizedUrl = string.Join("_", url.Split(Path.GetInvalidFileNameChars()));
        string cacheFile = Path.Combine(CacheDirectory, $"{sanitizedUrl}.json");

        // Check if we have a cached version on disk
        if (File.Exists(cacheFile))
        {
            _logger.LogDebug("Using disk-cached data for: {Url}", url);
            string cachedContent = await File.ReadAllTextAsync(cacheFile, cancellationToken);
            _memoryCache[url] = cachedContent;
            return cachedContent;
        }

        // If not cached, fetch from API
        _logger.LogInformation("Fetching from API: {Url}", url);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        string apiContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Save to cache file
        await File.WriteAllTextAsync(cacheFile, apiContent, cancellationToken);

        // Add to memory cache
        _memoryCache[url] = apiContent;

        // Add a small delay to be nice to the API
        await Task.Delay(100, cancellationToken);

        return apiContent;
    }

    /// <summary>
    /// Processes API requests in batches to avoid overwhelming the API.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the API response into.</typeparam>
    /// <param name="urls">The list of URLs to process.</param>
    /// <param name="getCachedApiResponseAsync">Function to get API responses with caching.</param>
    /// <param name="batchSize">Size of each processing batch (default: 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of deserialized objects from the API responses.</returns>
    private async Task<List<T>> ProcessInBatchesAsync<T>(
        IReadOnlyList<string> urls,
        Func<string, Task<string>> getCachedApiResponseAsync,
        int batchSize = 20,
        CancellationToken cancellationToken = default)
    {
        var resultArray = new T[urls.Count];
        int processedCount = 0;
        var failedUrls = new ConcurrentBag<(string Url, string Error)>();
        int totalBatches = (int)Math.Ceiling(urls.Count / (double)batchSize);

        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            int batchStart = batchIndex * batchSize;
            int currentBatchSize = Math.Min(batchSize, urls.Count - batchStart);

            var tasks = CreateBatchTasks(urls, resultArray, failedUrls,
                batchStart, currentBatchSize, getCachedApiResponseAsync, cancellationToken);

            await Task.WhenAll(tasks);
            processedCount += currentBatchSize - failedUrls.Count(f =>
                Array.IndexOf(urls.ToArray(), f.Url) >= batchStart &&
                Array.IndexOf(urls.ToArray(), f.Url) < batchStart + currentBatchSize);

            LogBatchProgress(batchIndex + 1, totalBatches, processedCount, urls.Count);
        }

        LogFailedUrls(failedUrls);

        // Filter out null entries and return as List
        return resultArray.Where(item => !EqualityComparer<T>.Default.Equals(item, default!)).ToList();
    }

    /// <summary>
    /// Creates a batch of tasks for processing API URLs.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the API response into.</typeparam>
    /// <param name="urls">The list of URLs to process.</param>
    /// <param name="resultArray">Array to store the results in.</param>
    /// <param name="failedUrls">Collection to track failed URL operations.</param>
    /// <param name="batchStart">Starting index for the batch.</param>
    /// <param name="batchSize">Size of the batch.</param>
    /// <param name="getCachedApiResponseAsync">Function to get API responses with caching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of tasks for the batch.</returns>
    private List<Task> CreateBatchTasks<T>(
        IReadOnlyList<string> urls,
        T[] resultArray,
        ConcurrentBag<(string Url, string Error)> failedUrls,
        int batchStart,
        int batchSize,
        Func<string, Task<string>> getCachedApiResponseAsync,
        CancellationToken cancellationToken)
    {
        return urls
            .Skip(batchStart)
            .Take(batchSize)
            .Select((url, i) =>
            {
                int index = batchStart + i;
                return Task.Run(async () =>
                {
                    try
                    {
                        string json = await getCachedApiResponseAsync(url);
                        var result = JsonSerializer.Deserialize<T>(json);

                        if (result != null)
                        {
                            resultArray[index] = result;
                        }
                        else
                        {
                            failedUrls.Add((url, "Deserialization resulted in null"));
                        }
                    }
                    catch (Exception ex)
                    {
                        failedUrls.Add((url, ex.Message));
                    }
                }, cancellationToken);
            })
            .ToList();
    }

    /// <summary>
    /// Logs progress information for batch processing.
    /// </summary>
    /// <param name="currentBatch">The index of the current batch.</param>
    /// <param name="totalBatches">The total number of batches.</param>
    /// <param name="processedCount">The number of items processed so far.</param>
    /// <param name="totalCount">The total number of items to process.</param>
    private void LogBatchProgress(int currentBatch, int totalBatches, int processedCount, int totalCount)
    {
        _logger.LogInformation(
            "Completed batch {CurrentBatch} of {TotalBatches} ({Processed}/{Total} processed)",
            currentBatch, totalBatches, processedCount, totalCount);
    }

    /// <summary>
    /// Logs information about URLs that failed to process.
    /// </summary>
    /// <param name="failedUrls">Collection of URLs that failed with error messages.</param>
    private void LogFailedUrls(ConcurrentBag<(string Url, string Error)> failedUrls)
    {
        if (failedUrls.IsEmpty) return;

        _logger.LogWarning("Failed to process {Count} URLs:", failedUrls.Count);
        foreach ((string url, string error) in failedUrls.Take(5))
        {
            _logger.LogWarning("  - {Url}: {Error}", url, error);
        }

        if (failedUrls.Count > 5)
        {
            _logger.LogWarning("  ... and {Count} more", failedUrls.Count - 5);
        }
    }

    /// <summary>
    /// Represents configuration for the download process.
    /// </summary>
    /// <param name="Offset">Starting offset for Pokemon IDs.</param>
    /// <param name="Limit">Maximum number of Pokémon to process.</param>
    /// <param name="BruteForce">Whether to attempt downloading all possible form variations.</param>
    private record DownloadConfig(int Offset, int Limit, bool BruteForce);

    /// <summary>
    /// Represents a work item for downloading a specific sprite.
    /// </summary>
    /// <param name="DexNum">The Pokedex number.</param>
    /// <param name="FormNum">The form number/index.</param>
    /// <param name="StyleNum">The style number/index.</param>
    private record SpriteDownloadItem(int DexNum, int FormNum, int StyleNum);
}
