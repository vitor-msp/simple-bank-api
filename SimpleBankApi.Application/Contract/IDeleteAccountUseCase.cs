namespace SimpleBankApi.Application.Exceptions;

public interface IDeleteAccountUseCase
{
    Task Execute(int accountNumber);
}