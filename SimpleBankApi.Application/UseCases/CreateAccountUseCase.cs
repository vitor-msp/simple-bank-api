using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.UseCases;

public class CreateAccountUseCase : ICreateAccountUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public CreateAccountUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task<int> Execute(CreateAccountInput input)
    {
        var existingAccount = await _accountsRepository.GetByCpf(input.Cpf);
        if (existingAccount != null) throw new InvalidInputException("Cpf already registered.");

        var customer = new Customer(new CustomerFields() { Cpf = input.Cpf, Name = input.Name });
        var newAccount = new Account(new AccountFields()) { Owner = customer };

        await _accountsRepository.Save(newAccount);
        return newAccount.GetFields().AccountNumber;
    }
}