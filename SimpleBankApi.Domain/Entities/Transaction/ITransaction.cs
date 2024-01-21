namespace SimpleBankApi.Domain.Entities;

public interface ITransaction
{
    public TransactionFields GetFields();
}