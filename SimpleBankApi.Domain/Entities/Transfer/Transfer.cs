namespace SimpleBankApi.Domain.Entities;

public class Transfer : Transaction, ITransfer
{
    public required IAccount Sender { get; init; }
    public required IAccount Recipient { get; init; }

    public Transfer() : base() { }

    private Transfer(int id, DateTime createdAt) : base(id, createdAt) { }

    public static Transfer Rebuild(int id, DateTime createdAt, double value,
        IAccount sender, IAccount recipient)
        => new(id, createdAt)
        {
            Value = value,
            Sender = sender,
            Recipient = recipient,
        };
}