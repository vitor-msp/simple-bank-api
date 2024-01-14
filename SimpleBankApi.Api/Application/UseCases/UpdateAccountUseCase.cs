using Dto;
using Models;

namespace Application;

public class UpdateAccountUseCase : IUpdateAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public UpdateAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber, AccountUpdateDto updatedAccountDto)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber) ?? throw new ApplicationException("Account not found.");
        account.Update(new CustomerUpdateableFields() { Name = updatedAccountDto.Name });
        await _accountsRepository.Save(account);
    }
}