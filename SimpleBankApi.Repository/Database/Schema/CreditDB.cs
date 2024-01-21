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

    public ICredit GetEntity()
    {
        return new Credit(CreditFields.Rebuild(Id, CreatedAt, Value));
    }

    public void Hydrate(ICredit credit)
    {
        var fields = credit.GetFields();
        Id = fields.Id;
        Value = fields.Value;
        CreatedAt = fields.CreatedAt;
    }
}