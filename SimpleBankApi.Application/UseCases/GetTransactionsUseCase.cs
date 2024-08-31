using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Dto;
using SimpleBankApi.Domain.ValueObjects;

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

        var transactions = await _transactionsRepository.GetTransactionsFromAccount(account.AccountNumber);

        var preparedTransactions = transactions.Select(transaction =>
        {
            if (transaction.TransactionType == TransactionType.Credit)
            {
                var credit = transaction.Credit ?? throw new Exception();
                return new TransactionDto()
                {
                    Type = transaction.TransactionType.ToString(),
                    CreditDto = CreditDto.Build(credit)
                };
            }
            if (transaction.TransactionType == TransactionType.Debit)
            {
                var debit = transaction.Debit ?? throw new Exception();
                return new TransactionDto()
                {
                    Type = transaction.TransactionType.ToString(),
                    DebitDto = DebitDto.Build(debit)
                };
            }
            var transfer = transaction.Transfer ?? throw new Exception();
            return new TransactionDto()
            {
                Type = transaction.TransactionType.ToString(),
                TransferDto = TransferDto.Build(transfer, account)
            };
        });

        return new GetTransactionsOutput()
        {
            Statement = new StatementDto()
            {
                Transactions = preparedTransactions.ToList()
            }
        };
    }
}