using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Contract;

public interface IAccountsRepository
{
    Task<List<IAccount>> GetAll();
    Task<IAccount?> GetByAccountNumber(int accountNumber);
    Task<IAccount?> GetByCpf(string cpf);
    Task Save(IAccount account);
}