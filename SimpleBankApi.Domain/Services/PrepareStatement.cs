using SimpleBankApi.Domain.Dto;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Services;

public class PrepareStatement
{
    public PrepareStatement() { }

    public StatementDto SortTransactionsByDateTime(
        List<ICredit> credits, List<IDebit> debits, List<ITransfer> transfers, IAccount account)
    {
        var sortedTransactions = new List<TransactionDto>();
        int creditIndex = 0, debitIndex = 0, transferIndex = 0;
        int max = credits.Count() + debits.Count() + transfers.Count();

        List<long> creditTimestamps = credits
            .Select(c => new DateTimeOffset(c.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> debitTimestamps = debits
            .Select(d => new DateTimeOffset(d.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> transferTimestamps = transfers
            .Select(t => new DateTimeOffset(t.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();

        creditTimestamps.Add(long.MaxValue);
        debitTimestamps.Add(long.MaxValue);
        transferTimestamps.Add(long.MaxValue);

        for (int index = 0; index < max; index++)
        {
            var creditTimestamp = creditTimestamps.ElementAt(creditIndex);
            var debitTimestamp = debitTimestamps.ElementAt(debitIndex);
            var transferTimestamp = transferTimestamps.ElementAt(transferIndex);

            if (creditTimestamp <= debitTimestamp && creditTimestamp <= transferTimestamp)
            {
                var credit = credits.ElementAt(creditIndex);
                sortedTransactions.Add(TransactionDto.BuildFromCredit(credit));
                creditIndex++;
            }
            else if (debitTimestamp <= transferTimestamp)
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(TransactionDto.BuildFromDebit(debit));
                debitIndex++;
            }
            else
            {
                var transfer = transfers.ElementAt(transferIndex);
                sortedTransactions.Add(TransactionDto.BuildFromTransfer(transfer, account));
                transferIndex++;
            }
        }
        return new StatementDto() { Transactions = sortedTransactions };
    }
}