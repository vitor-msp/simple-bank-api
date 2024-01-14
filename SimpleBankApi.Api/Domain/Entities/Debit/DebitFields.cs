namespace Models;

public class DebitFields : TransactionFields
{
    public DebitFields() : base() { }

    public new static DebitFields Rebuild(int id, DateTime createdAt, double value)
        => (DebitFields)TransactionFields.Rebuild(id, createdAt, value);
}