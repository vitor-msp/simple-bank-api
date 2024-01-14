namespace Models;

public class TransferFields : TransactionFields
{
    public TransferFields() : base() { }

    public new static TransferFields Rebuild(int id, DateTime createdAt, double value)
        => (TransferFields)TransactionFields.Rebuild(id, createdAt, value);
}