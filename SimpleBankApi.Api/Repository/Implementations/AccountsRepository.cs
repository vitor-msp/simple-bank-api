using Context;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class AccountsRepository : IAccountsRepository
{
    private readonly BankContext _context;

    public AccountsRepository(BankContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAll()
    {
        var accountsDB = await _context.Accounts.AsNoTracking().Include("Owner").Where(accountDB => accountDB.Active).ToListAsync();
        return accountsDB.Select(accountDB => accountDB.GetEntity()).ToList();
    }

    public async Task<Account?> GetByAccountNumber(int accountNumber)
    {
        var accountDB = await _context.Accounts.AsNoTracking().Include("Owner")
            .FirstOrDefaultAsync(accountDB => accountDB.Active && accountDB.AccountNumber == accountNumber);
        if (accountDB == null) return null;
        return accountDB.GetEntity();
    }

    public async Task<Account?> GetByCpf(string cpf)
    {
        var accountDB = await _context.Accounts.AsNoTracking().Include("Owner")
            .FirstOrDefaultAsync(accountDB => accountDB.Active && accountDB.Owner != null && accountDB.Owner.Cpf.Equals(cpf));
        if (accountDB == null) return null;
        return accountDB.GetEntity();
    }

    public async Task Save(Account account)
    {
        var accountDB = _context.Accounts.Find(account.GetFields().Id);
        if (accountDB == null) await Add(account);
        else await Update(accountDB, account);
    }


    private async Task Add(Account account)
    {
        var accountDB = new AccountDB(account);
        if (account.Owner == null) throw new Exception();
        var customerDB = new CustomerDB(account.Owner);
        accountDB.Owner = customerDB;
        _context.Accounts.Add(accountDB);
        await _context.SaveChangesAsync();
    }

    private async Task Update(AccountDB accountDB, Account account)
    {
        accountDB.Hydrate(account);
        if (account.Owner == null) throw new Exception();
        var customerDB = new CustomerDB(account.Owner);
        accountDB.Owner = customerDB;
        await _context.SaveChangesAsync();
    }
}