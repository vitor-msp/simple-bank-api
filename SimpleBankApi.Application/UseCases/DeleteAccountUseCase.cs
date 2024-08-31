using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class DeleteAccountUseCase(IAccountsRepository accountsRepository) : IDeleteAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository = accountsRepository;

    public async Task Execute(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber)
            ?? throw new EntityNotFoundException("Account not found.");

        account.Inactivate();
        await _accountsRepository.Save(account);
    }
}