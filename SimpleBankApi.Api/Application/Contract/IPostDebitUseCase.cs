using Dto;

namespace Application;

public interface IPostDebitUseCase
{
    Task Execute(int accountNumber, DebitDto debitDto);
}