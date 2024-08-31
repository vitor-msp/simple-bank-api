using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Services;

public class CalculateBalance : ICalculateBalance
{
    private readonly ITransactionsRepository _transactionsRepository;

    public CalculateBalance(ITransactionsRepository transactionsRepository)
    {
        _transactionsRepository = transactionsRepository;
    }

    public async Task<double> FromAccount(IAccount account)
    {
        var transactions = await _transactionsRepository.GetTransactionsFromAccount(account.AccountNumber);
        var balance = transactions.Sum(transaction =>
        {
            if (transaction.TransactionType == TransactionType.Credit)
            {
                var credit = transaction.Credit ?? throw new Exception();
                return credit.Value;
            }
            if (transaction.TransactionType == TransactionType.Debit)
            {
                var debit = transaction.Debit ?? throw new Exception();
                return debit.Value;
            }
            var transfer = transaction.Transfer ?? throw new Exception();
            var value = transfer.Value;
            return transfer.Recipient.Equals(account) ? value : -1 * value;
        });
        return balance;
    }
}