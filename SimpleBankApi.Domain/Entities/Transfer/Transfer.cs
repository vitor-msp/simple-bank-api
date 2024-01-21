namespace SimpleBankApi.Domain.Entities;

public class Transfer : Transaction, ITransfer
{
    public IAccount? Sender { get; set; }
    public IAccount? Recipient { get; set; }

    public Transfer(TransferFields fields) : base(fields) { }
}