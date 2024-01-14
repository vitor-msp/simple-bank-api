using Dto;

namespace Application;

public interface IUpdateAccountUseCase
{
    Task Execute(int accountNumber, AccountUpdateDto updatedAccountDto);
}