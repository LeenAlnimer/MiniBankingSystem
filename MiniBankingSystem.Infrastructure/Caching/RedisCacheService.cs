using MiniBankingSystem.Application.Interfaces;
using StackExchange.Redis;

namespace MiniBankingSystem.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<string?> GetAsync(string key)
    {
        var value = await _database.StringGetAsync(key);

        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetAsync(
        string key,
        string value,
        TimeSpan expiration)
    {
        await _database.StringSetAsync(
            key,
            value,
            expiration);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}