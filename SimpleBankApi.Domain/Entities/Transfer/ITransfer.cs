namespace SimpleBankApi.Domain.Entities;

public interface ITransfer : ITransaction
{
    public IAccount? Sender { get; set; }

    public IAccount? Recipient { get; set; }

    public TransactionTransferDto GetData(IAccount account);
}