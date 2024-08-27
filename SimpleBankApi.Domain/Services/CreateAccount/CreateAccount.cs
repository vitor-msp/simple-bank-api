using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Services;

public class CreateAccount : ICreateAccount
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAccount(IAccountsRepository accountsRepository, IPasswordHasher passwordHasher)
    {
        _accountsRepository = accountsRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateAccountOutput> Execute(CreateAccountInput input)
    {
        var existingAccount = await _accountsRepository.GetByCpf(input.Cpf);
        if (existingAccount != null) throw new DomainException("Cpf already registered.");

        if (!input.Password.Equals(input.PasswordConfirmation))
            throw new DomainException("Password and confirmation must be equal.");

        var customer = new Customer(input.GetCustomerFields());
        var accountFields = input.GetAccountFields();
        accountFields.PasswordHash = _passwordHasher.Hash(input.Password);
        var newAccount = new Account(accountFields) { Owner = customer };

        await _accountsRepository.Save(newAccount);

        return new CreateAccountOutput()
        {
            AccountNumber = newAccount.GetFields().AccountNumber,
        };
    }
}