namespace SimpleBankApi.Domain.Entities;

public interface ITransfer : ITransaction
{
    IAccount Sender { get; init; }
    IAccount Recipient { get; init; }
}