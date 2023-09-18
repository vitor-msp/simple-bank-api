using Microsoft.EntityFrameworkCore;
using Models;

namespace Context;

public class BankContext : DbContext
{
    public BankContext(DbContextOptions<BankContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Credit> Credits { get; set; }
    public DbSet<Debit> Debits { get; set; }
    public DbSet<Tranfer> Tranfers { get; set; }
}