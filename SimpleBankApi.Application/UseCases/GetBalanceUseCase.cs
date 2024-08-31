using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Application.Utils;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Extensions;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class GetBalanceUseCase : IGetBalanceUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly ICalculateBalance _calculateBalance;
    private readonly IBankCache _bankCache;
    private readonly int _oneDay = 60 * 60 * 24;

    public GetBalanceUseCase(
        IAccountsRepository accountsRepository,
        ICalculateBalance calculateBalance,
        IBankCache bankCache)
    {
        _accountsRepository = accountsRepository;
        _calculateBalance = calculateBalance;
        _bankCache = bankCache;
    }

    public async Task<GetBalanceOutput> Execute(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var cacheKey = CacheKeys.Balance(account);
        var balanceCacheValue = await _bankCache.Get(cacheKey);

        if (balanceCacheValue != null && double.TryParse(balanceCacheValue, out double balance))
            return new GetBalanceOutput { Balance = balance.GetBrazilianCurrency() };

        balance = await _calculateBalance.FromAccount(account);
        await _bankCache.Set(cacheKey, balance.ToString(), ttlInSeconds: _oneDay);

        return new GetBalanceOutput { Balance = balance.GetBrazilianCurrency() };
    }
}