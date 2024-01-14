namespace Models;

public class TransferFields : TransactionFields
{
    public TransferFields() : base() { }

    private TransferFields(int id, DateTime createdAt) : base(id, createdAt) { }

    public static TransferFields Rebuild(int id, DateTime createdAt, double value)
        => new TransferFields(id, createdAt)
        {
            Value = value
        };
}