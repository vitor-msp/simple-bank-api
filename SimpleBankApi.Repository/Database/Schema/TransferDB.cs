using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Repository.Database.Schema;

public class TransferDB
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public AccountDB? Sender { get; set; }
    public AccountDB? Recipient { get; set; }

    public TransferDB() { }

    public TransferDB(ITransfer transfer)
    {
        Hydrate(transfer);
    }

    public ITransfer GetEntity(IAccount sender, IAccount recipient)
        => Transfer.Rebuild(Id, CreatedAt, Value, sender, recipient);

    public void Hydrate(ITransfer transfer)
    {
        Id = transfer.Id;
        Value = transfer.Value;
        CreatedAt = transfer.CreatedAt;
    }
}