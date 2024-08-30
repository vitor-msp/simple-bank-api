using SimpleBankApi.Domain.Dto;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Services;

public class PrepareStatement
{
    private readonly List<ICredit> _credits;
    private readonly List<IDebit> _debits;
    private readonly List<ITransfer> _transfers;
    private readonly IAccount _account;
    private List<TransactionDto> _sortedTransactions = new();
    private List<long> _creditTimestamps = new(), _debitTimestamps = new(), _transferTimestamps = new();
    private int _max, _creditIndex, _debitIndex, _transferIndex;
    private long _creditTimestamp, _debitTimestamp, _transferTimestamp;

    public PrepareStatement(List<ICredit> credits, List<IDebit> debits, List<ITransfer> transfers, IAccount account)
    {
        _credits = credits;
        _debits = debits;
        _transfers = transfers;
        _account = account;
    }

    public StatementDto SortTransactionsByDateTime()
    {
        _creditIndex = 0;
        _debitIndex = 0;
        _transferIndex = 0;
        _max = _credits.Count() + _debits.Count() + _transfers.Count();

        _creditTimestamps = GetCreditTimestamps();
        _debitTimestamps = GetDebitTimestamps();
        _transferTimestamps = GetTransferTimestamps();

        _creditTimestamps.Add(long.MaxValue);
        _debitTimestamps.Add(long.MaxValue);
        _transferTimestamps.Add(long.MaxValue);

        for (int index = 0; index < _max; index++)
            ProcessStep();

        return new StatementDto() { Transactions = _sortedTransactions };
    }

    private List<long> GetCreditTimestamps()
    {
        return _credits.Select(credit => new DateTimeOffset(credit.CreatedAt).ToUnixTimeMilliseconds()).ToList();
    }

    private List<long> GetDebitTimestamps()
    {
        return _debits.Select(debit => new DateTimeOffset(debit.CreatedAt).ToUnixTimeMilliseconds()).ToList();
    }

    private List<long> GetTransferTimestamps()
    {
        return _transfers.Select(transfer => new DateTimeOffset(transfer.CreatedAt).ToUnixTimeMilliseconds()).ToList();
    }

    private void ProcessStep()
    {
        _creditTimestamp = _creditTimestamps.ElementAt(_creditIndex);
        _debitTimestamp = _debitTimestamps.ElementAt(_debitIndex);
        _transferTimestamp = _transferTimestamps.ElementAt(_transferIndex);

        if (CreditIsOldest())
        {
            var credit = _credits.ElementAt(_creditIndex);
            _sortedTransactions.Add(TransactionDto.BuildFromCredit(credit));
            _creditIndex++;
        }
        else if (DebitIsOlderThanTransfer())
        {
            var debit = _debits.ElementAt(_debitIndex);
            _sortedTransactions.Add(TransactionDto.BuildFromDebit(debit));
            _debitIndex++;
        }
        else
        {
            var transfer = _transfers.ElementAt(_transferIndex);
            _sortedTransactions.Add(TransactionDto.BuildFromTransfer(transfer, _account));
            _transferIndex++;
        }
    }

    private bool CreditIsOldest() => _creditTimestamp <= _debitTimestamp && _creditTimestamp <= _transferTimestamp;

    private bool DebitIsOlderThanTransfer() => _debitTimestamp <= _transferTimestamp;
}