using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Repository.Database.Schema;

public class DebitDB
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public AccountDB? Account { get; set; }

    public DebitDB() { }

    public DebitDB(IDebit debit)
    {
        Hydrate(debit);
    }

    public IDebit GetEntity()
    {
        return new Debit(DebitFields.Rebuild(Id, CreatedAt, Value));
    }

    public void Hydrate(IDebit debit)
    {
        var fields = debit.GetFields();
        Id = fields.Id;
        Value = fields.Value;
        CreatedAt = fields.CreatedAt;
    }
}