using System.Text.Json;
using System.Text.Json.Serialization;
using CommentSystem.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CommentSystem.Presentation.Services
{
    public class CacheService : ICacheService
    {
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrWhiteSpace(cachedValue))
            {
                _logger.LogInformation("Redis cache MISS for key {CacheKey}", key);
                return null;
            }

            _logger.LogInformation("Redis cache HIT for key {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, SerializerOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default) where T : class
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);

            var expiration = absoluteExpirationRelativeToNow ?? DefaultExpiration;
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _distributedCache.SetStringAsync(key, json, options, cancellationToken);
            _logger.LogInformation("Redis cache SET for key {CacheKey} with TTL {TtlSeconds}s", key, expiration.TotalSeconds);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Redis cache REMOVE for key {CacheKey}", key);
        }
    }
}
