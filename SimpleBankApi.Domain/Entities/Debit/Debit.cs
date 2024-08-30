namespace SimpleBankApi.Domain.Entities;

public class Debit : Transaction, IDebit
{
    public required IAccount Account { get; init; }

    public Debit() : base() { }

    private Debit(int id, DateTime createdAt) : base(id, createdAt) { }

    public static Debit Rebuild(int id, DateTime createdAt, double value, IAccount account)
        => new(id, createdAt)
        {
            Value = value,
            Account = account,
        };
}