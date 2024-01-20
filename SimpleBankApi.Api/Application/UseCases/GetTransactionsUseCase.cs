using System.Collections;
using Application.Exceptions;
using Dto;
using Models;

namespace Application;

public class GetTransactionsUseCase : IGetTransactionsUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;

    public GetTransactionsUseCase(ITransactionsRepository transactionsRepository, IAccountsRepository accountsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
    }

    public async Task<GetTransactionsOutput> Execute(int accountNumber)
    {
        Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var credits = await GetCreditsFromAccount(account);
        var debits = await GetDebitsFromAccount(account);
        var transfers = await GetTransfersFromAccount(account);

        var prepareStatement = new PrepareStatement();
        var sortedTransactions = prepareStatement.SortTransactionsByDateTime(credits, debits, transfers, account);
        return new GetTransactionsOutput { Transactions = sortedTransactions };
    }

    private async Task<List<Credit>> GetCreditsFromAccount(Account account)
    {
        return await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<Debit>> GetDebitsFromAccount(Account account)
    {
        return await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<Transfer>> GetTransfersFromAccount(Account account)
    {
        return await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
    }
}