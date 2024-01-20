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

        double balance = await CalculateBalanceFromAccount(account);
        if (balance < debitDto.Value) throw new InvalidInputException("Insufficient balance.");

        var debit = new Debit(new DebitFields() { Value = debitDto.Value }) { Account = account };
        await _transactionsRepository.SaveDebit(debit);
    }

    private async Task<double> CalculateBalanceFromAccount(Account account)
    {
        double creditSum = (await GetCreditsFromAccount(account)).Sum(c => c.GetFields().Value);
        double debitSum = -1 * (await GetDebitsFromAccount(account)).Sum(d => d.GetFields().Value);
        var transfers = await GetTransfersFromAccount(account);

        double transferSum = transfers.Sum(t => t.Sender.Equals(account) ? (-1 * t.GetFields().Value) : t.GetFields().Value);
        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<List<Credit>> GetCreditsFromAccount(Account account)
    {
        var credits = await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
        return credits;
    }

    private async Task<List<Debit>> GetDebitsFromAccount(Account account)
    {
        var debits = await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
        return debits;
    }

    private async Task<List<Transfer>> GetTransfersFromAccount(Account account)
    {
        var transfers = await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
        return transfers;
    }
}