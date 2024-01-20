using SimpleBankApi.Application.Input;

namespace SimpleBankApi.Application.Exceptions;

public interface IUpdateAccountUseCase
{
    Task Execute(int accountNumber, UpdateAccountInput input);
}