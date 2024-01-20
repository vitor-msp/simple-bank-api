using Models;

namespace Application;

public interface IGetAccountUseCase
{
    Task<Account> ByAccountNumber(int accountNumber);
    Task<Account> ByCpf(string cpf);
}