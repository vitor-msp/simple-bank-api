using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Utils;

namespace SimpleBankApi.Application.UseCases;

public class PostCreditUseCase : IPostCreditUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IBankCache _bankCache;

    public PostCreditUseCase(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        IBankCache bankCache)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _bankCache = bankCache;
    }

    public async Task Execute(int accountNumber, CreditInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var credit = input.GetCredit(account);
        await _transactionsRepository.SaveCredit(credit);
        
        await _bankCache.Delete(CacheKeys.Balance(account));
    }
}