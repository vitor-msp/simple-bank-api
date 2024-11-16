using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Repository.Database.Schema;

[Index(nameof(AccountNumber), IsUnique = true)]
public class AccountDB
{
    [Key]
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
    public string Role { get; set; } = "Customer";
    public CustomerDB Owner { get; set; }
    public string PasswordHash { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }

#pragma warning disable CS8618
    public AccountDB() { }

    public AccountDB(IAccount account)
    {
        Hydrate(account);
    }
#pragma warning restore CS8618

    public void Hydrate(IAccount account)
    {
        Id = account.Id;
        AccountNumber = account.AccountNumber;
        Active = account.Active;
        CreatedAt = account.CreatedAt;
        PasswordHash = account.PasswordHash;
        RefreshToken = account.RefreshToken;
        RefreshTokenExpiration = account.RefreshTokenExpiration;
        Role = account.Role.ToString();
    }

    public IAccount GetAccount()
    {
        if (!Enum.TryParse(Role, ignoreCase: true, out Role role))
            throw new Exception("Invalid role.");

        return Account.Rebuild(
            Id, AccountNumber, CreatedAt, Active, role, Owner.GetCustomer(), PasswordHash, RefreshToken, RefreshTokenExpiration);
    }
}