using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class GetAccountUseCase(IAccountsRepository accountsRepository) : IGetAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository = accountsRepository;

    public async Task<GetAccountOutput> ByAccountNumber(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber)
            ?? throw new EntityNotFoundException("Account not found.");
        return GetAccountOutput.Build(account);
    }

    public async Task<GetAccountOutput> ByCpf(string cpf)
    {
        var account = await _accountsRepository.GetByCpf(cpf)
            ?? throw new EntityNotFoundException("Account not found.");
        return GetAccountOutput.Build(account);
    }
}