using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Application.UseCases;

public class CreateAdminAccountUseCase(ICreateAccount createAccount) : ICreateAdminAccountUseCase
{
    private readonly ICreateAccount _createAccount = createAccount;

    public async Task<CreateAccountOutput> Execute(CreateAccountInput input)
    {
        input.Role = Role.Admin;
        return await _createAccount.Execute(input);
    }
}