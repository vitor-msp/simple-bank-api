namespace SimpleBankApi.Domain.Entities;

public class AccountFields
{
    public int Id { get; private set; }
    public int AccountNumber { get; private set; }
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; set; }

    public AccountFields()
    {
        AccountNumber = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());
        CreatedAt = DateTime.Now;
        Active = true;
    }

    private AccountFields(int id, int accountNumber, DateTime createdAt)
    {
        Id = id;
        AccountNumber = accountNumber;
        CreatedAt = createdAt;
    }

    public static AccountFields Rebuild(int id, int accountNumber, DateTime createdAt, bool active, string? passwordHash)
    {
        return new AccountFields(id, accountNumber, createdAt)
        {
            PasswordHash = passwordHash,
            Active = active
        };
    }
}