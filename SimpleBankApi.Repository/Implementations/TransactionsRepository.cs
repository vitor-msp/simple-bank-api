using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Database.Schema;

namespace SimpleBankApi.Repository.Implementations;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly BankContext _context;

    public TransactionsRepository(BankContext context)
    {
        _context = context;
    }

    public async Task SaveCredit(ICredit credit)
    {
        var accountDB = await _context.Accounts.FindAsync(credit.Account.Id)
            ?? throw new Exception("Account not found.");

        var creditDB = new TransactionDB(credit) { OperatingAccount = accountDB };

        _context.Transactions.Add(creditDB);
        await _context.SaveChangesAsync();
    }

    public async Task SaveDebit(IDebit debit)
    {
        var accountDB = await _context.Accounts.FindAsync(debit.Account.Id)
            ?? throw new Exception("Account not found.");

        var debitDB = new TransactionDB(debit) { OperatingAccount = accountDB };

        _context.Transactions.Add(debitDB);
        await _context.SaveChangesAsync();
    }

    public async Task SaveTransfer(ITransfer transfer)
    {
        var senderDB = await _context.Accounts.FindAsync(transfer.Sender.Id)
            ?? throw new Exception("Sender not found.");

        var recipientDB = await _context.Accounts.FindAsync(transfer.Recipient.Id)
            ?? throw new Exception("Recipient not found.");

        var (debitDB, creditDB) = TransactionDB.BuildTransfer(transfer, senderDB, recipientDB);

        _context.Transactions.Add(debitDB);
        _context.Transactions.Add(creditDB);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TransactionWrapper>> GetTransactionsFromAccount(int accountNumber)
    {
        var transactionsDB = await _context.Transactions.AsNoTracking()
            .Include("OperatingAccount.Owner")
            .Include("RelatedAccount.Owner")
            .Where(transactionDB => transactionDB.OperatingAccount.AccountNumber.Equals(accountNumber))
            .OrderByDescending(transactionsDB => transactionsDB.CreatedAt)
            .ToListAsync();

        return transactionsDB.Select(transactionDB =>
        {
            if (transactionDB.TransactionType == TransactionType.Credit)
                return new TransactionWrapper()
                {
                    TransactionType = TransactionType.Credit,
                    Credit = transactionDB.GetCredit()
                };

            if (transactionDB.TransactionType == TransactionType.Debit)
                return new TransactionWrapper()
                {
                    TransactionType = TransactionType.Debit,
                    Debit = transactionDB.GetDebit()
                };

            return new TransactionWrapper()
            {
                TransactionType = TransactionType.Transfer,
                Transfer = transactionDB.GetTransfer()
            };

        }).ToList();
    }
}