using System.ComponentModel.DataAnnotations;
using Models;

namespace Repository;

public class AccountDB
{
    [Key]
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
    public CustomerDB? Owner { get; set; }

    public AccountDB() { }

    public AccountDB(Account account)
    {
        Hydrate(account);
    }

    public Account GetEntity()
    {
        return new Account(AccountFields.Rebuild(Id, AccountNumber, CreatedAt, Active));
    }

    public void Hydrate(Account account)
    {
        var fields = account.GetFields();
        Id = fields.Id;
        AccountNumber = fields.AccountNumber;
        Active = fields.Active;
        Active = fields.Active;
    }
}