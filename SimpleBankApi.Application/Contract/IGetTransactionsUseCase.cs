using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface IGetTransactionsUseCase
{
    Task<GetTransactionsOutput> Execute(int accountNumber);
}