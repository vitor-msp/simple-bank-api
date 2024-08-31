using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Entities;

public class Credit : Transaction, ICredit
{
    public required IAccount Account { get; init; }

    private readonly double _value;
    public override required double Value
    {
        get => _value;
        init
        {
            if (value <= 0)
                throw new DomainException("The transaction value must be greater than zero.");
            _value = value;
        }
    }

    public Credit() : base() { }

    private Credit(int id, DateTime createdAt) : base(id, createdAt) { }

    public static Credit Rebuild(int id, DateTime createdAt, double value, IAccount account)
        => new(id, createdAt)
        {
            Value = value,
            Account = account,
        };
}