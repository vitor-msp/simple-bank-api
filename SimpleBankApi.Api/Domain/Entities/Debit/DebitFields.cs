namespace Models;

public class DebitFields : TransactionFields
{
    public DebitFields() : base() { }

    private DebitFields(int id, DateTime createdAt) : base(id, createdAt) { }

    public static DebitFields Rebuild(int id, DateTime createdAt, double value)
        => new DebitFields(id, createdAt)
        {
            Value = value
        };
}