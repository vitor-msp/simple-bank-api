using Dto;

namespace Application;

public interface ICreateAccountUseCase
{
    Task<int> Execute(AccountCreateDto input);
}