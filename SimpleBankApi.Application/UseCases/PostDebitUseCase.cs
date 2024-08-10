using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class PostDebitUseCase : IPostDebitUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IBankCache _bankCache;

    public PostDebitUseCase(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        IBankCache bankCache)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _bankCache = bankCache;
    }

    public async Task Execute(int accountNumber, DebitInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var calculateBalance = new CalculateBalance(_transactionsRepository, _bankCache);
        double balance = await calculateBalance.FromAccount(account);
        if (balance < input.Value) throw new InvalidInputException("Insufficient balance.");

        var debit = new Debit(input.GetFiels()) { Account = account };
        await _transactionsRepository.SaveDebit(debit);
    }
}