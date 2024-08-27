using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Application.UseCases;

public class CreateAccountUseCase : ICreateAccountUseCase
{
    private readonly ICreateAccount _createAccount;

    public CreateAccountUseCase(ICreateAccount createAccount)
    {
        _createAccount = createAccount;
    }

    public async Task<CreateAccountOutput> Execute(CreateAccountInput input)
    {
        input.Role = Role.Customer;
        return await _createAccount.Execute(input);
    }
}