using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface ILoginUseCase
{
    Task<LoginOutput> Execute(LoginInput input);
}