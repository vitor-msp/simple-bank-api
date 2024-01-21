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

    public ITransfer GetEntity()
    {
        return new Transfer(TransferFields.Rebuild(Id, CreatedAt, Value));
    }

    public void Hydrate(ITransfer transfer)
    {
        var fields = transfer.GetFields();
        Id = fields.Id;
        Value = fields.Value;
        CreatedAt = fields.CreatedAt;
    }
}