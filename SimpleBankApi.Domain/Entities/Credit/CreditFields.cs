namespace SimpleBankApi.Domain.Entities;

public class CreditFields : TransactionFields
{
    public CreditFields() : base() { }

    private CreditFields(int id, DateTime createdAt) : base(id, createdAt) { }

    public static CreditFields Rebuild(int id, DateTime createdAt, double value)
        => new CreditFields(id, createdAt)
        {
            Value = value
        };
}