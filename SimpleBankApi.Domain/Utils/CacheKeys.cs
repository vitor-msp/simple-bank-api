using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Utils;

public static class CacheKeys
{
    public static string Balance(IAccount account)
        => $"balance-{account.GetFields().AccountNumber}";
}