using Dto;
using Models;

namespace Application;

public class CreateAccountUseCase : ICreateAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public CreateAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task<int> Execute(AccountCreateDto input)
    {
        var existingAccount = await _accountsRepository.GetByCpf(input.Cpf);
        if (existingAccount != null) throw new ApplicationException("Cpf already registered.");

        var customer = new Customer(new CustomerFields() { Cpf = input.Cpf, Name = input.Name });
        var newAccount = new Account(new AccountFields()) { Owner = customer };

        await _accountsRepository.Save(newAccount);
        return newAccount.GetFields().AccountNumber;
    }
}