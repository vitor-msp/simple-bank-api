namespace SimpleBankApi.Domain.Entities;

public interface ICredit : ITransaction
{
    public IAccount? Account { get; set; }
}