using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface IGetAccountUseCase
{
    Task<GetAccountOutput> ByAccountNumber(int accountNumber);
    Task<GetAccountOutput> ByCpf(string cpf);
}