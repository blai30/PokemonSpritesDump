using System.Text.Json;
using SpriteDownloader;

// Create HttpClient with optimized settings.
var client = new HttpClient(new HttpClientHandler { MaxConnectionsPerServer = 20 })
{
    Timeout = TimeSpan.FromMinutes(5)
};

// Ensure directories exist.
Directory.CreateDirectory("sprites");
Directory.CreateDirectory("cache");

// Initialize slug map.
var slugMap = new Dictionary<int, List<string>>
{
    { 0, ["egg"] }
};

// const int offset = 200; // Unown
// const int offset = 385; // Deoxys
// const int offset = 422; // Gastrodon
const int offset = 0;
const int limit = 1025;
const bool bruteForce = false;

Console.WriteLine("Fetching Pokemon species list...");
string speciesListJson =
    await client.GetCachedApiResponseAsync($"https://pokeapi.co/api/v2/pokemon-species?offset={offset}&limit={limit}");
var speciesList = (JsonSerializer.Deserialize<NamedApiResourceList>(speciesListJson))!.Results;

// Fetch species data in parallel batches.
Console.WriteLine("Fetching Pokemon species details...");
var speciesUrls = speciesList.Select(specie => specie.Url).ToList();
var species =
    await Helpers.ProcessInBatchesAsync<PokemonSpecies>(speciesUrls, url => client.GetCachedApiResponseAsync(url));
var speciesMap = species.ToDictionary(specie => specie.Name);

var pokemonUrls = species.SelectMany(specie => specie.Varieties!.Select(variant => variant.Pokemon.Url)).ToList();
var pokemons = await Helpers.ProcessInBatchesAsync<Pokemon>(pokemonUrls, url => client.GetCachedApiResponseAsync(url));
var pokemonMap = pokemons.ToDictionary(pokemon => pokemon.Name);

var formUrls = pokemons.SelectMany(pokemon => pokemon.Forms.Select(form => form.Url)).ToList();
var forms = await Helpers.ProcessInBatchesAsync<PokemonForm>(formUrls, url => client.GetCachedApiResponseAsync(url));

// Process species data.
foreach (var form in forms)
{
    var pokemon = pokemonMap[form.Pokemon!.Name];
    var speciesData = speciesMap[pokemon.Species.Name];
    Console.WriteLine($"Species: {speciesData.Name}, Pokemon: {pokemon.Name}, Form: {form.Name}");

    if (slugMap.TryGetValue(speciesData.Id, out var slugs))
    {
        slugs.Add((pokemon.IsDefault && pokemon.Name == form.Name) ||
                  (speciesData.Name == pokemon.Name && form.IsDefault)
            ? speciesData.Name
            : form.Name);
    }
    else
    {
        slugMap[speciesData.Id] =
        [
            (pokemon.IsDefault && pokemon.Name == form.Name) ||
            (speciesData.Name == pokemon.Name && form.IsDefault)
                ? speciesData.Name
                : form.Name
        ];
    }
}

// Cache the slugMap for future use.
await File.WriteAllTextAsync(Path.Combine("cache", $"slug_map_{offset}_{limit}.json"),
    JsonSerializer.Serialize(slugMap, new JsonSerializerOptions { WriteIndented = true }));

// Download sprites in parallel with better control and tracking.
Console.WriteLine("Downloading sprites...");
int totalCount = 0;
int downloadedCount = 0;
int skippedCount = 0;
int failedCount = 0;

// Create batches of work to download.
var downloadBatches = new List<(int DexId, int FormId, int StyleId)>();

// Prepare all the combinations we need to download.
for (int i = offset; i < offset + speciesList.Count + 1; i++)
{
    // Skip processing if we're out of range or no form slugs available.
    if (!bruteForce && !slugMap.TryGetValue(i, out var formSlugs))
        continue;

    // Only process valid form IDs for this species.
    for (int j = 0; j < (bruteForce ? 100 : Math.Min(formSlugs.Count, 99)); j++)
    {
        // Process all style variants.
        for (int k = 0; k < (bruteForce ? 10 : 1); k++)
        {
            downloadBatches.Add((i, j, k));
            totalCount++;
        }
    }
}

// Process in controlled batches.
const int batchSize = 30;
for (int batchStart = 0; batchStart < downloadBatches.Count; batchStart += batchSize)
{
    var currentBatchTasks = new List<Task>();
    int batchEnd = Math.Min(batchStart + batchSize, downloadBatches.Count);

    for (int i = batchStart; i < batchEnd; i++)
    {
        (int dexId, int formId, int styleId) = downloadBatches[i];
        currentBatchTasks.Add(DownloadSpriteAsync(dexId, formId, styleId, client, slugMap));
    }

    await Task.WhenAll(currentBatchTasks);

    int currentBatch = batchStart / batchSize + 1;
    int totalBatches = (int)Math.Ceiling(downloadBatches.Count / (double)batchSize);
    Console.WriteLine($"Completed sprite batch {currentBatch}/{totalBatches} " +
                      $"(Downloaded: {downloadedCount}, Skipped: {skippedCount}, Failed: {failedCount})");
}

Console.WriteLine(
    $"Download complete. Total: {totalCount}, Downloaded: {downloadedCount}, Skipped: {skippedCount}, Failed: {failedCount}");
return;

// Separated sprite download method for better organization.
async Task DownloadSpriteAsync(int dexNum, int formNum, int styleNum, HttpClient httpClient,
    Dictionary<int, List<string>> slugDictionary)
{
    try
    {
        string dexId = dexNum.ToString("D4");
        string formId = formNum.ToString("D2");
        string styleId = styleNum.ToString("D1");

        string imageUrl =
            $"https://resource.pokemon-home.com/battledata/img/pokei128/icon{dexId}_f{formId}_s{styleId}.png";

        // Get form slug for file naming.
        string formSlug = formNum < slugDictionary[dexNum].Count
            ? slugDictionary[dexNum][formNum]
            : formId;

        // Determine output filename.
        string fileName = formNum == 0
            ? Path.Combine("sprites", $"sprite_{dexId}_s{styleId}.png")
            : Path.Combine("sprites", $"sprite_{dexId}_{formSlug}_s{styleId}.png");

        // Check if file already exists.
        if (File.Exists(fileName))
        {
            Interlocked.Increment(ref skippedCount);
            return;
        }

        // Image cache file path.
        string imageCacheFile = Path.Combine("cache", $"icon{dexId}_f{formId}_s{styleId}.png");
        byte[] imageBytes;

        // Check image cache first.
        if (File.Exists(imageCacheFile))
        {
            imageBytes = await File.ReadAllBytesAsync(imageCacheFile);
        }
        else
        {
            using var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

            // Skip if not found (404) or other error.
            if (!response.IsSuccessStatusCode)
            {
                Interlocked.Increment(ref failedCount);
                return;
            }

            imageBytes = await response.Content.ReadAsByteArrayAsync();

            // Only cache successful responses.
            if (imageBytes.Length > 0)
            {
                await File.WriteAllBytesAsync(imageCacheFile, imageBytes);
            }
        }

        // Save to disk if we have valid image data.
        if (imageBytes.Length > 0)
        {
            await File.WriteAllBytesAsync(fileName, imageBytes);
            Interlocked.Increment(ref downloadedCount);
            Console.WriteLine($"Downloaded: {fileName}");
        }
        else
        {
            Interlocked.Increment(ref failedCount);
        }
    }
    catch (Exception ex)
    {
        Interlocked.Increment(ref failedCount);
        Console.WriteLine($"Error downloading sprite {dexNum}/{formNum}/{styleNum}: {ex.Message}");
    }
}
