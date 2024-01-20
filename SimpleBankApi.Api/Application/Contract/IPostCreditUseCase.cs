using Dto;

namespace Application;

public interface IPostCreditUseCase
{
    Task Execute(int accountNumber, CreditDto creditDto);
}