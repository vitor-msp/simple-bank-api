using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Repository.Database.Schema;

public class AccountDB
{
    [Key]
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public string? PasswordHash { get; set; } = "";
    public string? RefreshToken { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
    public CustomerDB? Owner { get; set; }

    public AccountDB() { }

    public AccountDB(IAccount account)
    {
        Hydrate(account);
    }

    public IAccount GetEntity()
    {
        return new Account(AccountFields.Rebuild(Id, AccountNumber, CreatedAt, Active, PasswordHash));
    }

    public void Hydrate(IAccount account)
    {
        var fields = account.GetFields();
        Id = fields.Id;
        AccountNumber = fields.AccountNumber;
        Active = fields.Active;
        PasswordHash = fields.PasswordHash;
        RefreshToken = fields.RefreshToken;
    }
}