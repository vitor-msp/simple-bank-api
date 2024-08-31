using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Utils;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class PostDebitUseCase(
    ITransactionsRepository transactionsRepository,
    IAccountsRepository accountsRepository,
    ICalculateBalance calculateBalance,
    IBankCache bankCache) : IPostDebitUseCase
{
    private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;
    private readonly IAccountsRepository _accountsRepository = accountsRepository;
    private readonly ICalculateBalance _calculateBalance = calculateBalance;
    private readonly IBankCache _bankCache = bankCache;

    public async Task Execute(int accountNumber, DebitInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber)
            ?? throw new EntityNotFoundException("Account not found.");

        double balance = await _calculateBalance.FromAccount(account);
        if (balance < input.Value) throw new InvalidInputException("Insufficient balance.");

        var debit = input.GetDebit(account);
        await _transactionsRepository.SaveDebit(debit);

        await _bankCache.Delete(CacheKeys.Balance(account));
    }
}