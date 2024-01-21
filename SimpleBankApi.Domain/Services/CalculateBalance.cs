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
        double creditSum = await GetCreditSum(account);
        double debitSum = await GetDebitSum(account);
        var transferSum = await GetTransferSum(account);

        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<double> GetCreditSum(IAccount account)
    {
        var credits = await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
        double creditSum = credits.Sum(credit => credit.GetFields().Value);
        return creditSum;
    }

    private async Task<double> GetDebitSum(IAccount account)
    {
        var debits = await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
        double debitSum = -1 * debits.Sum(debit => debit.GetFields().Value);
        return debitSum;
    }

    private async Task<double> GetTransferSum(IAccount account)
    {
        var transfers = await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
        double transferSum = transfers.Sum(transfer =>
        {
            var value = transfer.GetFields().Value;
            return transfer.Sender != null && transfer.Sender.Equals(account) ? (-1 * value) : value;
        });
        return transferSum;
    }
}