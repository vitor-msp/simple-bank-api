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

    public IDebit GetEntity(IAccount account)
        => Debit.Rebuild(Id, CreatedAt, Value, account);

    public void Hydrate(IDebit debit)
    {
        Id = debit.Id;
        Value = debit.Value;
        CreatedAt = debit.CreatedAt;
    }
}