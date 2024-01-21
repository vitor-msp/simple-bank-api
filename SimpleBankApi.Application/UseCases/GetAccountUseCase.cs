using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.UseCases;

public class GetAccountUseCase : IGetAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public GetAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task<IAccount> ByAccountNumber(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        return account;
    }

    public async Task<IAccount> ByCpf(string cpf)
    {
        var account = await _accountsRepository.GetByCpf(cpf);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        return account;
    }
}