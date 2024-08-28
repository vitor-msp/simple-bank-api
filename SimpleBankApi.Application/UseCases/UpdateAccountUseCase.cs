using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class UpdateAccountUseCase : IUpdateAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public UpdateAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber, UpdateAccountInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        input.Update(account);
        await _accountsRepository.Save(account);
    }
}