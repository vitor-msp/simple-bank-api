using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Utils;

public static class CacheKeys
{
    public static string Balance(IAccount account)
        => $"balance-{account.AccountNumber}";
}