using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

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
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        var credits = await GetCreditsFromAccount(account);
        var debits = await GetDebitsFromAccount(account);
        var transfers = await GetTransfersFromAccount(account);

        var prepareStatement = new PrepareStatement();
        var statement = prepareStatement.SortTransactionsByDateTime(credits, debits, transfers, account);
        return new GetTransactionsOutput() { Statement = statement };
    }

    private async Task<List<ICredit>> GetCreditsFromAccount(IAccount account)
    {
        return await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<IDebit>> GetDebitsFromAccount(IAccount account)
    {
        return await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
    }

    private async Task<List<ITransfer>> GetTransfersFromAccount(IAccount account)
    {
        return await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
    }
}