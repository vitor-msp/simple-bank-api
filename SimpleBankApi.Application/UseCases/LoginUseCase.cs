using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class LoginUseCase : ILoginUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public LoginUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task<LoginOutput> Execute(LoginInput input)
    {
        return default;
    }
}