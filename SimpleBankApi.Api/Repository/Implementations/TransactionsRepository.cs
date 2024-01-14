using Context;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly BankContext _context;

    public TransactionsRepository(BankContext context)
    {
        _context = context;
    }

    public async Task SaveCredit(Credit credit)
    {
        var creditDB = new CreditDB(credit);
        _context.Credits.Add(creditDB);
        await _context.SaveChangesAsync();
    }

    public async Task SaveDebit(Debit debit)
    {
        var debitDB = new DebitDB(debit);
        _context.Debits.Add(debitDB);
        await _context.SaveChangesAsync();
    }

    public async Task SaveTransfer(Transfer transfer)
    {
        var transferDB = new TransferDB(transfer);
        _context.Transfers.Add(transferDB);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Credit>> GetAccountCredits(int accountNumber)
    {
        var creditsDB = await _context.Credits.AsNoTracking().Include("Account")
            .Where(creditDB => creditDB.Account != null && creditDB.Account.AccountNumber == accountNumber).ToListAsync();
        return creditsDB.Select(creditDB => creditDB.GetEntity()).ToList();
    }

    public async Task<List<Debit>> GetAccountDebits(int accountNumber)
    {
        var debitsDB = await _context.Debits.AsNoTracking().Include("Account")
            .Where(debitDB => debitDB.Account != null && debitDB.Account.AccountNumber == accountNumber).ToListAsync();
        return debitsDB.Select(debitDB => debitDB.GetEntity()).ToList();
    }

    public async Task<List<Transfer>> GetAccountTransfers(int accountNumber)
    {
        var transfersDB = await _context.Transfers.AsNoTracking().Include("Sender.Owner").Include("Recipient.Owner")
            .Where(transferDB => (transferDB.Sender != null && transferDB.Sender.AccountNumber == accountNumber)
                || (transferDB.Recipient != null && transferDB.Recipient.AccountNumber == accountNumber)).ToListAsync();
        return transfersDB.Select(transferDB => transferDB.GetEntity()).ToList();
    }
}