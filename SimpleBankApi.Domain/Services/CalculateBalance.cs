using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Services;

public class CalculateBalance
{
    private readonly ITransactionsRepository _transactionsRepository;

    public CalculateBalance(ITransactionsRepository transactionsRepository)
    {
        _transactionsRepository = transactionsRepository;
    }

    public async Task<double> FromAccount(Account account)
    {
        double creditSum = (await GetCreditsFromAccount(account)).Sum(c => c.GetFields().Value);
        double debitSum = -1 * (await GetDebitsFromAccount(account)).Sum(d => d.GetFields().Value);

        var transfers = await GetTransfersFromAccount(account);
        double transferSum = transfers.Sum(transfer
            => transfer.Sender != null && transfer.Sender.Equals(account)
                ? (-1 * transfer.GetFields().Value)
                : transfer.GetFields().Value);

        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<List<Credit>> GetCreditsFromAccount(Account account)
    {
        return await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<Debit>> GetDebitsFromAccount(Account account)
    {
        return await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<Transfer>> GetTransfersFromAccount(Account account)
    {
        return await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
    }
}