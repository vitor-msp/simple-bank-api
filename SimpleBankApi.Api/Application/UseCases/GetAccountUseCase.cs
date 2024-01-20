using Application.Exceptions;
using Models;

namespace Application;

public class GetAccountUseCase : IGetAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public GetAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task<Account> ByAccountNumber(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        return account;
    }

    public async Task<Account> ByCpf(string cpf)
    {
        var account = await _accountsRepository.GetByCpf(cpf);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        return account;
    }
}