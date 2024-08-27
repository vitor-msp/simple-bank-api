using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Entities;

public class AccountFields
{
    public int Id { get; private set; }
    public int AccountNumber { get; private set; }
    public string? PasswordHash { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; set; }
    public Role Role { get; set; }

    public AccountFields()
    {
        AccountNumber = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());
        CreatedAt = DateTime.Now;
        Active = true;
        Role = Role.Customer;
    }

    private AccountFields(int id, int accountNumber, DateTime createdAt)
    {
        Id = id;
        AccountNumber = accountNumber;
        CreatedAt = createdAt;
    }

    public static AccountFields Rebuild(int id, int accountNumber, DateTime createdAt, bool active,
        Role role, string? passwordHash, string? refreshToken, DateTime? refreshTokenExpiration)
        => new(id, accountNumber, createdAt)
        {
            PasswordHash = passwordHash,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
            Active = active,
            Role = role,
        };
}