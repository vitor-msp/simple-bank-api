using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.ValueObjects;

public class TransactionWrapper
{
    public required TransactionType TransactionType { get; init; }
    public ICredit? Credit { get; init; }
    public IDebit? Debit { get; init; }
    public ITransfer? Transfer { get; init; }
}