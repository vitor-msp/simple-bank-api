using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Contract;

public interface IAccountsRepository
{
    Task<List<Account>> GetAll();
    Task<Account?> GetByAccountNumber(int accountNumber);
    Task<Account?> GetByCpf(string cpf);
    Task Save(Account account);
}