using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Application.UseCases;

public class GetTransactionsUseCase(
    ITransactionsRepository transactionsRepository,
    IAccountsRepository accountsRepository) : IGetTransactionsUseCase
{
    private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;
    private readonly IAccountsRepository _accountsRepository = accountsRepository;

    public async Task<GetTransactionsOutput> Execute(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber)
            ?? throw new EntityNotFoundException("Account not found.");

        var transactions = await _transactionsRepository
            .GetTransactionsFromAccount(account.AccountNumber);

        var preparedTransactions = transactions.Select(transaction =>
        {
            if (transaction.TransactionType == TransactionType.Credit)
            {
                var credit = transaction.Credit
                    ?? throw new Exception("Missing credit in the transaction wrapper.");
                return TransactionDto.BuildFromCredit(credit);
            }

            if (transaction.TransactionType == TransactionType.Debit)
            {
                var debit = transaction.Debit
                    ?? throw new Exception("Missing debit in the transaction wrapper.");
                return TransactionDto.BuildFromDebit(debit);
            }

            var transfer = transaction.Transfer
                ?? throw new Exception("Missing transfer in the transaction wrapper.");
            return TransactionDto.BuildFromTransfer(transfer, account);
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