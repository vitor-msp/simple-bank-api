using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Extensions;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class GetBalanceUseCase : IGetBalanceUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IBankCache _bankCache;

    public GetBalanceUseCase(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        IBankCache bankCache)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _bankCache = bankCache;
    }

    public async Task<GetBalanceOutput> Execute(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var calculateBalance = new CalculateBalance(_transactionsRepository, _bankCache);
        double balance = await calculateBalance.FromAccount(account);

        return new GetBalanceOutput { Balance = balance.GetBrazilianCurrency() };
    }
}