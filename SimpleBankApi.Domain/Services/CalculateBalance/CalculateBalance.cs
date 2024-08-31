using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Utils;

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
        double creditSum = await GetCreditSum(account);
        double debitSum = await GetDebitSum(account);
        var transferSum = await GetTransferSum(account);

        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<double> GetCreditSum(IAccount account)
    {
        var credits = await _transactionsRepository.GetCreditsFromAccount(account.AccountNumber);
        double creditSum = credits.Sum(credit => credit.Value);
        return creditSum;
    }

    private async Task<double> GetDebitSum(IAccount account)
    {
        var debits = await _transactionsRepository.GetDebitsFromAccount(account.AccountNumber);
        double debitSum = -1 * debits.Sum(debit => debit.Value);
        return debitSum;
    }

    private async Task<double> GetTransferSum(IAccount account)
    {
        var transfers = await _transactionsRepository.GetTransfersFromAccount(account.AccountNumber);
        double transferSum = transfers.Sum(transfer =>
        {
            var value = transfer.Value;
            return transfer.Sender != null && transfer.Sender.Equals(account) ? (-1 * value) : value;
        });
        return transferSum;
    }
}