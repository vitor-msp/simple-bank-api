namespace SimpleBankApi.Domain.Entities;

public interface ICredit : ITransaction
{
    IAccount Account { get; init; }
}