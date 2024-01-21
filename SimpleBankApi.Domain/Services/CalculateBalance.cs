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

    public async Task<double> FromAccount(IAccount account)
    {
        double creditSum = (await GetCreditsFromAccount(account)).Sum(credit => credit.GetFields().Value);
        double debitSum = -1 * (await GetDebitsFromAccount(account)).Sum(debit => debit.GetFields().Value);

        var transfers = await GetTransfersFromAccount(account);
        double transferSum = transfers.Sum(transfer
            => transfer.Sender != null && transfer.Sender.Equals(account)
                ? (-1 * transfer.GetFields().Value)
                : transfer.GetFields().Value);

        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<List<ICredit>> GetCreditsFromAccount(IAccount account)
    {
        return await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<IDebit>> GetDebitsFromAccount(IAccount account)
    {
        return await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<ITransfer>> GetTransfersFromAccount(IAccount account)
    {
        return await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
    }
}