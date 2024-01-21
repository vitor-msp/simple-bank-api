using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Exceptions;

public interface IGetAccountUseCase
{
    Task<IAccount> ByAccountNumber(int accountNumber);
    Task<IAccount> ByCpf(string cpf);
}