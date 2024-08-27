using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface ICreateAccountUseCase
{
    Task<CreateAccountOutput> Execute(CreateAccountInput input);
}