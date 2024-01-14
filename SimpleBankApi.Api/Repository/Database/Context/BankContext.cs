using Microsoft.EntityFrameworkCore;
using Repository;

namespace Context;

public class BankContext : DbContext
{
    public BankContext(DbContextOptions<BankContext> options) : base(options) { }

    public DbSet<CustomerDB> Customers { get; set; }
    public DbSet<AccountDB> Accounts { get; set; }
    public DbSet<CreditDB> Credits { get; set; }
    public DbSet<DebitDB> Debits { get; set; }
    public DbSet<TransferDB> Transfers { get; set; }
}