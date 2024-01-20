using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface IGetBalanceUseCase
{
    Task<GetBalanceOutput> Execute(int accountNumber);
}