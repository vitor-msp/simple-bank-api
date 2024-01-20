using SimpleBankApi.Application.Input;

namespace SimpleBankApi.Application.Exceptions;

public interface ICreateAccountUseCase
{
    Task<int> Execute(CreateAccountInput input);
}