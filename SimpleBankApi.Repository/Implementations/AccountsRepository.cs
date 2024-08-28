using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Database.Schema;

namespace SimpleBankApi.Repository.Implementations;

public class AccountsRepository : IAccountsRepository
{
    private readonly BankContext _context;

    public AccountsRepository(BankContext context)
    {
        _context = context;
    }

    public async Task<List<IAccount>> GetAll()
    {
        var accountsDB = await _context.Accounts.AsNoTracking().Include("Owner")
            .Where(accountDB => accountDB.Active).ToListAsync();

        return accountsDB.Select(accountDB =>
        {
            if (accountDB.Owner == null) throw new Exception();
            var owner = accountDB.Owner.GetEntity();
            var account = accountDB.GetEntity(owner);
            return account;
        }).ToList();
    }

    public async Task<IAccount?> GetByAccountNumber(int accountNumber)
    {
        var accountDB = await _context.Accounts.AsNoTracking().Include("Owner")
            .FirstOrDefaultAsync(accountDB => accountDB.Active && accountDB.AccountNumber == accountNumber);

        if (accountDB == null) return null;
        if (accountDB.Owner == null) throw new Exception();

        var owner = accountDB.Owner.GetEntity();
        var account = accountDB.GetEntity(owner);
        return account;
    }

    public async Task<IAccount?> GetByCpf(string cpf)
    {
        var accountDB = await _context.Accounts.AsNoTracking().Include("Owner")
            .FirstOrDefaultAsync(accountDB =>
                accountDB.Active && accountDB.Owner != null && accountDB.Owner.Cpf.Equals(cpf));

        if (accountDB == null) return null;
        if (accountDB.Owner == null) throw new Exception();

        var owner = accountDB.Owner.GetEntity();
        var account = accountDB.GetEntity(owner);
        return account;
    }

    public async Task Save(IAccount account)
    {
        var accountDB = await _context.Accounts.Include("Owner")
            .FirstOrDefaultAsync(accountDB =>
                accountDB.Active && accountDB.AccountNumber == account.AccountNumber);

        if (accountDB == null) await Add(account);
        else await Update(accountDB, account);
    }

    private async Task Add(IAccount account)
    {
        if (account.Owner == null) throw new Exception();

        var accountDB = new AccountDB(account);
        var customerDB = new CustomerDB(account.Owner);
        accountDB.Owner = customerDB;

        _context.Accounts.Add(accountDB);
        await _context.SaveChangesAsync();
    }

    private async Task Update(AccountDB accountDB, IAccount account)
    {
        if (account.Owner == null || accountDB.Owner == null) throw new Exception();

        accountDB.Hydrate(account);
        accountDB.Owner.Hydrate(account.Owner);

        await _context.SaveChangesAsync();
    }
}