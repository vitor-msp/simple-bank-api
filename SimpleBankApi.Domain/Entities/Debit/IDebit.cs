namespace SimpleBankApi.Domain.Entities;

public interface IDebit : ITransaction
{
    public IAccount? Account { get; set; }

    public (string, double, DateTime) GetDataWithoutAccount();
}