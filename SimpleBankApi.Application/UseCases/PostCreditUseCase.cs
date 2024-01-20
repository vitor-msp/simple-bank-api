using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.UseCases;

public class PostCreditUseCase : IPostCreditUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;

    public PostCreditUseCase(ITransactionsRepository transactionsRepository, IAccountsRepository accountsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber, CreditInput creditDto)
    {
        Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        var credit = new Credit(new CreditFields() { Value = creditDto.Value }) { Account = account };
        await _transactionsRepository.SaveCredit(credit);
    }
}