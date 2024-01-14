using System.Text.Json.Serialization;

namespace Models;

public class TransactionFields
{
    [JsonIgnore]
    public int Id { get; private set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; private set; }

    protected TransactionFields()
    {
        CreatedAt = DateTime.Now;
    }

    private TransactionFields(int id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    protected static TransactionFields Rebuild(int id, DateTime createdAt, double value)
    {
        return new TransactionFields(id, createdAt)
        {
            Value = value
        };
    }
}