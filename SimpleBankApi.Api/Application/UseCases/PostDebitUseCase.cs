using Application.Exceptions;
using Dto;
using Models;

namespace Application;

public class PostDebitUseCase : IPostDebitUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;

    public PostDebitUseCase(ITransactionsRepository transactionsRepository, IAccountsRepository accountsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber, DebitDto debitDto)
    {
        Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var calculateBalance = new CalculateBalance(_transactionsRepository);
        double balance = await calculateBalance.FromAccount(account);
        if (balance < debitDto.Value) throw new InvalidInputException("Insufficient balance.");

        var debit = new Debit(new DebitFields() { Value = debitDto.Value }) { Account = account };
        await _transactionsRepository.SaveDebit(debit);
    }
}