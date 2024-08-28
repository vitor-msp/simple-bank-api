using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Repository.Database.Schema;

public class AccountDB
{
    [Key]
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public string PasswordHash { get; set; } = "";
    public string? RefreshToken { get; set; } = "";
    public DateTime? RefreshTokenExpiration { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
    public string Role { get; set; } = "Customer";
    public CustomerDB? Owner { get; set; }

    public AccountDB() { }

    public AccountDB(IAccount account)
    {
        Hydrate(account);
    }

    public IAccount GetEntity(ICustomer owner)
    {
        Enum.TryParse(Role, ignoreCase: true, out Role role);
        return Account.Rebuild(
            Id, AccountNumber, CreatedAt, Active, role, owner, PasswordHash, RefreshToken, RefreshTokenExpiration);
    }

    public void Hydrate(IAccount account)
    {
        Id = account.Id;
        AccountNumber = account.AccountNumber;
        Active = account.Active;
        PasswordHash = account.PasswordHash;
        RefreshToken = account.RefreshToken;
        RefreshTokenExpiration = account.RefreshTokenExpiration;
        Role = account.Role.ToString();
    }
}