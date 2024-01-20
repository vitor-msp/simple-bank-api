using Input;

namespace Application;

public interface IPostCreditUseCase
{
    Task Execute(int accountNumber, CreditInput creditDto);
}