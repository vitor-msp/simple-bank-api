using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Application.UseCases;

public class CreateAdminAccountUseCase : ICreateAdminAccountUseCase
{
    private readonly ICreateAccount _createAccount;

    public CreateAdminAccountUseCase(ICreateAccount createAccount)
    {
        _createAccount = createAccount;
    }

    public async Task<CreateAccountOutput> Execute(CreateAccountInput input)
    {
        input.Role = Role.Admin;
        return await _createAccount.Execute(input);
    }
}