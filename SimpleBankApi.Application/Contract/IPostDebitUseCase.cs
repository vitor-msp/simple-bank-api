using SimpleBankApi.Application.Input;

namespace SimpleBankApi.Application.Exceptions;

public interface IPostDebitUseCase
{
    Task Execute(int accountNumber, DebitInput debitDto);
}