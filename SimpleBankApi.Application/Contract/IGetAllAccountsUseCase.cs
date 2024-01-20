using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface IGetAllAccountsUseCase
{
    Task<GetAllAccountsOutput> Execute();
}