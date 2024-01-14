namespace Models;

public class CreditFields : TransactionFields
{
    public CreditFields() : base() { }

    public new static CreditFields Rebuild(int id, DateTime createdAt, double value)
        => (CreditFields)TransactionFields.Rebuild(id, createdAt, value);
}