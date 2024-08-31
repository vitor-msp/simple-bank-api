using Microsoft.Extensions.Options;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Infra.Configuration;
using StackExchange.Redis;

namespace SimpleBankApi.Infra.Implementations;

public class BankCacheRedis : IBankCache
{
    private readonly IDatabase _redis;

    public BankCacheRedis(IOptions<RedisConfiguration> options)
    {
        var redisConnectionString = options.Value.ConnectionString
            ?? throw new Exception("Redis connection string is invalid.");

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

    public async Task Delete(string key) => await _redis.KeyDeleteAsync(key);
}