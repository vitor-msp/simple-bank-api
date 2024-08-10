using Microsoft.Extensions.Options;
using SimpleBankApi.Domain.Contract;
using StackExchange.Redis;

namespace SimpleBankApi.Repository.Cache;

public class BankCacheRedis : IBankCache
{
    private readonly IDatabase _redis;

    public BankCacheRedis(IOptions<RedisConfiguration> options)
    {
        var redisConnectionString = options.Value.ConnectionString;
        if (redisConnectionString == null)
            throw new Exception("Redis connection string is invalid.");
        try
        {
            _redis = ConnectionMultiplexer.Connect(redisConnectionString).GetDatabase();
        }
        catch (Exception)
        {
            throw new Exception("Error to connect to Redis.");
        }
    }

    public async Task<string?> Get(string key)
        => await _redis.StringGetAsync(key);

    public async Task Set(string key, string value, int ttlInSeconds)
        => await _redis.StringSetAsync(key, value, TimeSpan.FromSeconds(ttlInSeconds));
}