using System.Collections;

namespace Models;

public class PrepareStatement
{
    public PrepareStatement() { }

    public ArrayList SortTransactionsByDateTime(
        List<Credit> credits, List<Debit> debits, List<Transfer> transfers, Account account)
    {
        var sortedTransactions = new ArrayList();
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
                sortedTransactions.Add(new TransactionCreditDebitDto(credit.GetDataWithoutAccount()));
                creditIndex++;
            }
            else if (debitTimestamp <= transferTimestamp)
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(new TransactionCreditDebitDto(debit.GetDataWithoutAccount()));
                debitIndex++;
            }
            else
            {
                var transfer = transfers.ElementAt(transferIndex);
                sortedTransactions.Add(transfer.GetData(account));
                transferIndex++;
            }
        }
        return sortedTransactions;
    }
}