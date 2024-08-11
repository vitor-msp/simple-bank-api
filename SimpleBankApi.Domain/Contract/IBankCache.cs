namespace SimpleBankApi.Domain.Contract;

public interface IBankCache
{
    Task Set(string key, string value, int ttlInSeconds);
    Task<string?> Get(string key);
    Task Delete(string key);
}