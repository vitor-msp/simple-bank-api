using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Repository.Database.Schema;

namespace SimpleBankApi.Repository.Database.Context;

public class BankContext : DbContext
{
    public BankContext() { }

    public BankContext(DbContextOptions<BankContext> options) : base(options) { }

    public DbSet<CustomerDB> Customers { get; set; }
    public DbSet<AccountDB> Accounts { get; set; }
    public DbSet<TransactionDB> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite();
    }
}