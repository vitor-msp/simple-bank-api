using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Entities;

public class Account : IAccount
{
    public int Id { get; }
    public int AccountNumber { get; }
    public DateTime CreatedAt { get; }
    public bool Active { get; private set; }
    public Role Role { get; set; }
    public required ICustomer Owner { get; set; }
    public required string PasswordHash { get; set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiration { get; private set; }

    public Account()
    {
        AccountNumber = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());
        CreatedAt = DateTime.Now.ToUniversalTime();
        Active = true;
        Role = Role.Customer;
    }

    private Account(int id, int accountNumber, DateTime createdAt)
    {
        Id = id;
        AccountNumber = accountNumber;
        CreatedAt = createdAt;
    }

    public void Inactivate()
    {
        Active = false;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (!obj.GetType().Equals(this.GetType())) return false;
        Account accountToCompare = (Account)obj;
        return accountToCompare.AccountNumber == this.AccountNumber;
    }

    public override int GetHashCode() => AccountNumber;

    public void UpdateRefreshToken(string? token, DateTime? expiration)
    {
        RefreshToken = token;
        RefreshTokenExpiration = expiration.HasValue ? expiration.Value.ToUniversalTime() : expiration;
    }

    public static Account Rebuild(int id, int accountNumber, DateTime createdAt, bool active,
        Role role, ICustomer owner, string passwordHash, string? refreshToken, DateTime? refreshTokenExpiration)
        => new(id, accountNumber, createdAt)
        {
            Active = active,
            Role = role,
            Owner = owner,
            PasswordHash = passwordHash,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
        };
}