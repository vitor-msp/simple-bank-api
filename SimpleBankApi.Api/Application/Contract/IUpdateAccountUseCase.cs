using Input;

namespace Application;

public interface IUpdateAccountUseCase
{
    Task Execute(int accountNumber, AccountUpdateInput updatedAccountDto);
}