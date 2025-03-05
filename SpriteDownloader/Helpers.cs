using System.Collections.Concurrent;
using System.Text.Json;

namespace SpriteDownloader;

public static class Helpers
{
    // Optional memory cache for frequently accessed URLs
    private static readonly ConcurrentDictionary<string, string> _memoryCache = new();

    /// <summary>
    /// Processes a list of API tasks in batches with controlled parallelism.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the API response to</typeparam>
    /// <param name="urls">List of API URLs to process</param>
    /// <param name="getCachedApiResponseAsync">Function to get cached or fresh API responses</param>
    /// <param name="batchSize">Number of concurrent requests per batch</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Ordered enumerable of deserialized objects</returns>
    public static async Task<IEnumerable<T>> ProcessInBatchesAsync<T>(
        IReadOnlyList<string> urls,
        Func<string, Task<string>> getCachedApiResponseAsync,
        int batchSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Create result array with the same size as input
        var resultArray = new T[urls.Count];
        int processedCount = 0;
        var failedUrls = new ConcurrentBag<(string Url, string Error)>();

        for (int batchStart = 0; batchStart < urls.Count; batchStart += batchSize)
        {
            var tasks = new List<Task>();
            int batchEnd = Math.Min(batchStart + batchSize, urls.Count);

            for (int i = batchStart; i < batchEnd; i++)
            {
                int index = i;
                string url = urls[i];

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string json = await getCachedApiResponseAsync(url).ConfigureAwait(false);
                        var result = JsonSerializer.Deserialize<T>(json);

                        if (result != null)
                        {
                            resultArray[index] = result;
                            Interlocked.Increment(ref processedCount);
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
                }, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            int currentBatch = batchStart / batchSize + 1;
            int totalBatches = (int)Math.Ceiling(urls.Count / (double)batchSize);
            Console.WriteLine(
                $"Completed batch {currentBatch} of {totalBatches} ({processedCount}/{urls.Count} processed)");
        }

        if (failedUrls.Count > 0)
        {
            Console.WriteLine($"Failed to process {failedUrls.Count} URLs:");
            foreach (var (url, error) in failedUrls.Take(5))
            {
                Console.WriteLine($"  - {url}: {error}");
            }

            if (failedUrls.Count > 5)
            {
                Console.WriteLine($"  ... and {failedUrls.Count - 5} more");
            }
        }

        // Filter out null entries and return as IEnumerable
        return resultArray.Where(item => !EqualityComparer<T>.Default.Equals(item, default));
    }

    /// <summary>
    /// Extension method for HttpClient that retrieves API response with caching support.
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="url">The API endpoint URL</param>
    /// <param name="cacheDirectory">Directory to store cached responses</param>
    /// <param name="useMemoryCache">Whether to use memory caching</param>
    /// <param name="delayMs">Optional delay between API requests in milliseconds</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The API response as string</returns>
    public static Task<string> GetCachedApiResponseAsync(
        this HttpClient client,
        string url,
        string cacheDirectory = "cache",
        bool useMemoryCache = true,
        int delayMs = 100,
        CancellationToken cancellationToken = default)
    {
        // Check memory cache first if enabled
        if (useMemoryCache && _memoryCache.TryGetValue(url, out string? cachedResponse))
        {
            Console.WriteLine($"Using memory-cached data for: {url}");
            return Task.FromResult(cachedResponse);
        }

        // Ensure cache directory exists
        Directory.CreateDirectory(cacheDirectory);

        // Create a filename from the URL (removing invalid characters)
        string sanitizedUrl = string.Join("_", url.Split(Path.GetInvalidFileNameChars()));
        string cacheFile = Path.Combine(cacheDirectory, $"{sanitizedUrl}.json");

        // Check if we have a cached version on disk
        if (File.Exists(cacheFile))
        {
            Console.WriteLine($"Using disk-cached data for: {url}");
            return LoadAndUpdateMemoryCacheAsync(cacheFile, url, useMemoryCache);
        }

        // If not cached, fetch from API
        Console.WriteLine($"Fetching from API: {url}");
        return FetchAndCacheResponseAsync(client, url, cacheFile, useMemoryCache, delayMs, cancellationToken);
    }

    private static async Task<string> LoadAndUpdateMemoryCacheAsync(string cacheFile, string url, bool useMemoryCache)
    {
        string content = await File.ReadAllTextAsync(cacheFile);

        // Add to memory cache if enabled
        if (useMemoryCache)
        {
            _memoryCache[url] = content;
        }

        return content;
    }

    private static async Task<string> FetchAndCacheResponseAsync(
        HttpClient client,
        string url,
        string cacheFile,
        bool useMemoryCache,
        int delayMs,
        CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        // Save to cache file
        await File.WriteAllTextAsync(cacheFile, content, cancellationToken);

        // Add to memory cache if enabled
        if (useMemoryCache)
        {
            _memoryCache[url] = content;
        }

        // Add a small delay to be nice to the API
        if (delayMs > 0)
        {
            await Task.Delay(delayMs, cancellationToken);
        }

        return content;
    }
}
