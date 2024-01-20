using Application.Exceptions;
using Dto;
using Models;

namespace Application;

public class PostCreditUseCase : IPostCreditUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;

    public PostCreditUseCase(ITransactionsRepository transactionsRepository, IAccountsRepository accountsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber, CreditDto creditDto)
    {
        Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");
        var credit = new Credit(new CreditFields() { Value = creditDto.Value }) { Account = account };
        await _transactionsRepository.SaveCredit(credit);
    }
}