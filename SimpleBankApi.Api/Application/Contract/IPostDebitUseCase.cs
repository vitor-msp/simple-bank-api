using Input;

namespace Application;

public interface IPostDebitUseCase
{
    Task Execute(int accountNumber, DebitInput debitDto);
}