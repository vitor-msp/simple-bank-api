using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Utils;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Services;

public class CalculateBalance : ICalculateBalance
{
    private readonly int _oneDay = 60 * 60 * 24;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IBankCache _bankCache;

    public CalculateBalance(ITransactionsRepository transactionsRepository,
        IBankCache bankCache)
    {
        _transactionsRepository = transactionsRepository;
        _bankCache = bankCache;
    }

    public async Task<double> FromAccount(IAccount account)
    {
        var cacheKey = CacheKeys.Balance(account);
        var balanceCacheValue = await _bankCache.Get(cacheKey);
        if (balanceCacheValue != null && double.TryParse(balanceCacheValue, out double balance))
            return balance;
        balance = await ProcessBalance(account);
        await _bankCache.Set(cacheKey, balance.ToString(), ttlInSeconds: _oneDay);
        return balance;
    }

    private async Task<double> ProcessBalance(IAccount account)
    {
        var transactions = await _transactionsRepository.GetTransactionsFromAccount(account.AccountNumber);
        var balance = transactions.Sum(transaction =>
        {
            if (transaction.TransactionType == TransactionType.Credit)
            {
                var credit = transaction.Credit ?? throw new Exception();
                return credit.Value;
            }
            if (transaction.TransactionType == TransactionType.Debit)
            {
                var debit = transaction.Debit ?? throw new Exception();
                return debit.Value;
            }
            var transfer = transaction.Transfer ?? throw new Exception();
            var value = transfer.Value;
            return transfer.Recipient.Equals(account) ? value : -1 * value;
        });
        return balance;
    }
}