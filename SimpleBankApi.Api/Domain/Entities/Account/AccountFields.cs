using System.Text.Json.Serialization;

namespace Models;

public class AccountFields
{
    [JsonIgnore]
    public int Id { get; private set; }
    public int AccountNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    [JsonIgnore]
    public bool Active { get; set; }

    public AccountFields()
    {
        AccountNumber = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());
        CreatedAt = DateTime.Now;
        Active = true;
    }

    private AccountFields(int id, int accountNumber, DateTime createdAt) { }

    public static AccountFields Rebuild(int id, int accountNumber, DateTime createdAt, bool active)
    {
        return new AccountFields(id, accountNumber, createdAt)
        {
            Active = active
        };
    }
}