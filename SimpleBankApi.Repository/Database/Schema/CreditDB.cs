using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Repository.Database.Schema;

public class CreditDB
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public AccountDB? Account { get; set; }

    public CreditDB() { }

    public CreditDB(ICredit credit)
    {
        Hydrate(credit);
    }

    public ICredit GetEntity(IAccount account)
        => Credit.Rebuild(Id, CreatedAt, Value, account);

    public void Hydrate(ICredit credit)
    {
        Id = credit.Id;
        Value = credit.Value;
        CreatedAt = credit.CreatedAt;
    }
}