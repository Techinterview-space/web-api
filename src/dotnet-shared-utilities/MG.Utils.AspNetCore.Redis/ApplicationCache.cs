using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MG.Utils.Abstract;
using Microsoft.Extensions.Caching.Distributed;

namespace MG.Utils.AspNetCore.Redis
{
    public class ApplicationCache : ICache
    {
        private readonly IDistributedCache _cache;

        public ApplicationCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetAsync(string key, string value, TimeSpan duration)
        {
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(duration);

            await _cache.SetAsync(key, Encoding.UTF8.GetBytes(value), options);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan duration)
        {
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(duration);

            var json = JsonSerializer.SerializeToUtf8Bytes(value);
            await _cache.SetAsync(key, json, options);
        }

        public async Task<string> GetAsync(string key)
        {
            key.ThrowIfNullOrEmpty(nameof(key));

            return await _cache.GetStringAsync(key);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            var serialized = await _cache.GetAsync(key);

            return serialized is not null ? JsonSerializer.Deserialize<T>(utf8Json: serialized) : default;
        }

        public async Task RemoveAsync(string key)
        {
            key.ThrowIfNullOrEmpty(nameof(key));

            await _cache.RemoveAsync(key);
        }
    }
}