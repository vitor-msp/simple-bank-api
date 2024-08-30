namespace SimpleBankApi.Domain.Entities;

public class Credit : Transaction, ICredit
{
    public required IAccount Account { get; init; }

    public Credit() : base() { }

    private Credit(int id, DateTime createdAt) : base(id, createdAt) { }

    public static Credit Rebuild(int id, DateTime createdAt, double value, IAccount account)
        => new(id, createdAt)
        {
            Value = value,
            Account = account,
        };
}