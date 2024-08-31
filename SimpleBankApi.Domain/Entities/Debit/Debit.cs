using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Entities;

public class Debit : Transaction, IDebit
{
    public required IAccount Account { get; init; }

    private readonly double _value;
    public override required double Value
    {
        get => _value;
        init
        {
            if (value >= 0)
                throw new DomainException("The transaction value must be less than zero.");
            _value = value;
        }
    }

    public Debit() : base() { }

    private Debit(int id, DateTime createdAt) : base(id, createdAt) { }

    public static Debit Rebuild(int id, DateTime createdAt, double value, IAccount account)
        => new(id, createdAt)
        {
            Account = account,
            Value = value,
        };
}