using Microsoft.EntityFrameworkCore;
using Models;

namespace Context;

public class BankContext : DbContext
{
    public BankContext(DbContextOptions<BankContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
}