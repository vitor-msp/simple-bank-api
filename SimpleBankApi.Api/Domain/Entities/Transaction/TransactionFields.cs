using System.Text.Json.Serialization;

namespace Models;

public abstract class TransactionFields
{
    [JsonIgnore]
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