namespace SimpleBankApi.Domain.Entities;

public abstract class TransactionFields
{
    public int Id { get; private set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; private set; }

    protected TransactionFields()
    {
        CreatedAt = DateTime.Now;
    }

    protected TransactionFields(int id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }
}