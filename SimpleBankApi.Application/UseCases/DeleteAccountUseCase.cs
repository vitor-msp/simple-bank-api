using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class DeleteAccountUseCase : IDeleteAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public DeleteAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        account.Inactivate();
        await _accountsRepository.Save(account);
    }
}