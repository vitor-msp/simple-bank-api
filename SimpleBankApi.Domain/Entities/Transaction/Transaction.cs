using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Entities;

public abstract class Transaction : ITransaction
{
    public int Id { get; }
    public DateTime CreatedAt { get; }

    private readonly double _value;
    public virtual required double Value
    {
        get => _value;
        init
        {
            if (value <= 0)
                throw new DomainException("The transaction value must be greater than zero.");
            _value = value;
        }
    }

    protected Transaction()
    {
        CreatedAt = DateTime.Now.ToUniversalTime();
    }

    protected Transaction(int id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }
}