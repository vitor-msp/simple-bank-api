using SimpleBankApi.Application.Input;

namespace SimpleBankApi.Application.Exceptions;

public interface IPostCreditUseCase
{
    Task Execute(int accountNumber, CreditInput creditDto);
}