using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Repository.Database.Schema;

public class AccountDB
{
    [Key]
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public string? PasswordHash { get; set; } = "";
    public string? RefreshToken { get; set; } = "";
    public DateTime? RefreshTokenExpiration { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
    public string Role { get; set; }
    public CustomerDB? Owner { get; set; }

    public AccountDB() { }

    public AccountDB(IAccount account)
    {
        Hydrate(account);
    }

    public IAccount GetEntity()
    {
        Enum.TryParse(Role, ignoreCase: true, out Role role);
        return new Account(AccountFields.Rebuild(
            Id, AccountNumber, CreatedAt, Active, role, PasswordHash, RefreshToken, RefreshTokenExpiration));
    }

    public void Hydrate(IAccount account)
    {
        var fields = account.GetFields();
        Id = fields.Id;
        AccountNumber = fields.AccountNumber;
        Active = fields.Active;
        PasswordHash = fields.PasswordHash;
        RefreshToken = fields.RefreshToken;
        RefreshTokenExpiration = fields.RefreshTokenExpiration;
        Role = fields.Role.ToString();
    }
}