using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.UseCases;

public class CreateAccountUseCase : ICreateAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAccountUseCase(IAccountsRepository accountsRepository, IPasswordHasher passwordHasher)
    {
        _accountsRepository = accountsRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<int> Execute(CreateAccountInput input)
    {
        var existingAccount = await _accountsRepository.GetByCpf(input.Cpf);
        if (existingAccount != null) throw new InvalidInputException("Cpf already registered.");

        if (!input.Password.Equals(input.PasswordConfirmation))
            throw new InvalidInputException("Password and confirmation must be equal.");

        var customer = new Customer(input.GetFields());
        var accountFields = new AccountFields()
        {
            PasswordHash = _passwordHasher.Hash(input.Password)
        };
        var newAccount = new Account(accountFields) { Owner = customer };

        await _accountsRepository.Save(newAccount);
        return newAccount.GetFields().AccountNumber;
    }
}