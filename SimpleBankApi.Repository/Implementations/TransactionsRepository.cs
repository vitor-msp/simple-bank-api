using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
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
        if (credit.Account == null) throw new Exception();

        var accountDB = await _context.Accounts.FindAsync(credit.Account.Id);
        if (accountDB == null) throw new Exception();

        var creditDB = new CreditDB(credit) { Account = accountDB };

        _context.Credits.Add(creditDB);
        await _context.SaveChangesAsync();
    }

    public async Task SaveDebit(IDebit debit)
    {
        if (debit.Account == null) throw new Exception();

        var accountDB = await _context.Accounts.FindAsync(debit.Account.Id);
        if (accountDB == null) throw new Exception();

        var debitDB = new DebitDB(debit) { Account = accountDB };

        _context.Debits.Add(debitDB);
        await _context.SaveChangesAsync();
    }

    public async Task SaveTransfer(ITransfer transfer)
    {
        if (transfer.Sender == null || transfer.Recipient == null) throw new Exception();

        var senderDB = await _context.Accounts.FindAsync(transfer.Sender.Id);
        var recipientDB = await _context.Accounts.FindAsync(transfer.Recipient.Id);
        if (senderDB == null || recipientDB == null) throw new Exception();

        var transferDB = new TransferDB(transfer) { Sender = senderDB, Recipient = recipientDB };

        _context.Transfers.Add(transferDB);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ICredit>> GetCreditsFromAccount(int accountNumber)
    {
        var creditsDB = await _context.Credits.AsNoTracking().Include("Account.Owner")
            .Where(creditDB => creditDB.Account != null && creditDB.Account.AccountNumber == accountNumber)
            .ToListAsync();

        return creditsDB.Select(creditDB =>
        {
            if (creditDB.Account == null || creditDB.Account.Owner == null) throw new Exception();

            var credit = creditDB.GetEntity();
            var owner = creditDB.Account.Owner.GetEntity();
            var account = creditDB.Account.GetEntity(owner);

            credit.Account = account;
            return credit;
        }).ToList();
    }

    public async Task<List<IDebit>> GetDebitsFromAccount(int accountNumber)
    {
        var debitsDB = await _context.Debits.AsNoTracking().Include("Account.Owner")
            .Where(debitDB => debitDB.Account != null && debitDB.Account.AccountNumber == accountNumber)
            .ToListAsync();

        return debitsDB.Select(debitDB =>
        {
            if (debitDB.Account == null || debitDB.Account.Owner == null) throw new Exception();

            var debit = debitDB.GetEntity();
            var owner = debitDB.Account.Owner.GetEntity();
            var account = debitDB.Account.GetEntity(owner);

            debit.Account = account;
            return debit;
        }).ToList();
    }

    public async Task<List<ITransfer>> GetTransfersFromAccount(int accountNumber)
    {
        var transfersDB = await _context.Transfers.AsNoTracking().Include("Sender.Owner").Include("Recipient.Owner")
            .Where(transferDB => (transferDB.Sender != null && transferDB.Sender.AccountNumber == accountNumber)
                || (transferDB.Recipient != null && transferDB.Recipient.AccountNumber == accountNumber))
            .ToListAsync();

        return transfersDB.Select(transferDB =>
        {
            if (transferDB.Sender == null || transferDB.Sender.Owner == null) throw new Exception();
            if (transferDB.Recipient == null || transferDB.Recipient.Owner == null) throw new Exception();

            var transfer = transferDB.GetEntity();
            var senderOwner = transferDB.Sender.Owner.GetEntity();
            var sender = transferDB.Sender.GetEntity(senderOwner);
            var recipientOwner = transferDB.Recipient.Owner.GetEntity();
            var recipient = transferDB.Recipient.GetEntity(recipientOwner);

            transfer.Sender = sender;
            transfer.Recipient = recipient;
            return transfer;
        }).ToList();
    }
}