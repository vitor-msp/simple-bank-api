namespace SimpleBankApi.Domain.Entities;

public interface IDebit : ITransaction
{
    IAccount Account { get; init; }
}