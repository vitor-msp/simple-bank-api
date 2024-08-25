using SimpleBankApi.Application.Input;

namespace SimpleBankApi.Application.Exceptions;

public interface ILogoutUseCase
{
    Task Execute(LogoutInput input);
}