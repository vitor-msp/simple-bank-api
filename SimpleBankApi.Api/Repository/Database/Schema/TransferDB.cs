using System.ComponentModel.DataAnnotations;
using Models;

namespace Repository;

public class TransferDB
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public AccountDB? Sender { get; set; }
    public AccountDB? Recipient { get; set; }

    public TransferDB() { }

    public TransferDB(Transfer transfer)
    {
        Hydrate(transfer);
    }

    public Transfer GetEntity()
    {
        return new Transfer(TransferFields.Rebuild(Id, CreatedAt, Value));
    }

    public void Hydrate(Transfer transfer)
    {
        var fields = transfer.GetFields();
        Id = fields.Id;
        Value = fields.Value;
        CreatedAt = fields.CreatedAt;
    }
}