using Input;

namespace Application;

public interface ICreateAccountUseCase
{
    Task<int> Execute(CreateAccountInput input);
}