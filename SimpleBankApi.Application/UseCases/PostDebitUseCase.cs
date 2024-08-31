using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Utils;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class PostDebitUseCase : IPostDebitUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly ICalculateBalance _calculateBalance;
    private readonly IBankCache _bankCache;

    public PostDebitUseCase(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        ICalculateBalance calculateBalance,
        IBankCache bankCache)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _calculateBalance = calculateBalance;
        _bankCache = bankCache;
    }

    public async Task Execute(int accountNumber, DebitInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        double balance = await _calculateBalance.FromAccount(account);
        if (balance < input.Value) throw new InvalidInputException("Insufficient balance.");

        var debit = input.GetDebit(account);
        await _transactionsRepository.SaveDebit(debit);

        await _bankCache.Delete(CacheKeys.Balance(account));
    }
}