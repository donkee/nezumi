using Windows.Win32.Foundation;
using Microsoft.Extensions.Caching.Memory;

namespace Nezumi.Utilities;

public static class Cache
{
    private static readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());

    private static readonly MemoryCacheEntryOptions MemoryCacheOptions =
        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));

    /// <summary>
    /// Sets a window's class nname in the cache.
    /// </summary>
    /// <param name="key">The key to set the value with.</param>
    /// <param name="value">The value to set.</param>
    public static void Set(HWND key, string value)
    {
        MemoryCache.Set(key, value, MemoryCacheOptions);
    }

    /// <summary>
    /// Gets a window's class name from the cache, or sets it if it doesn't exist.
    /// </summary>
    /// <param name="key">The key to get the value with.</param>
    /// <returns>The value from the cache.</returns>
    public static string? Get(HWND key)
    {
        return MemoryCache.GetOrCreate(key, entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(10));

            return key.WindowClassName();
        });
    }
}