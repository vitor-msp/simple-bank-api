using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Exceptions;

public interface IGetAccountUseCase
{
    Task<Account> ByAccountNumber(int accountNumber);
    Task<Account> ByCpf(string cpf);
}