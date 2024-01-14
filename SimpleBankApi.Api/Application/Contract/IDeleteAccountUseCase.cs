namespace Application;

public interface IDeleteAccountUseCase
{
    Task Execute(int accountNumber);
}