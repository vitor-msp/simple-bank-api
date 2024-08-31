using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Repository.Database.Schema;

[Index(nameof(Cpf), IsUnique = true)]
public class CustomerDB
{
    [Key]
    public int Id { get; set; }
    public string Cpf { get; set; }
    public string Name { get; set; }

#pragma warning disable CS8618
    public CustomerDB() { }

    public CustomerDB(ICustomer customer)
    {
        Hydrate(customer);
    }
#pragma warning restore CS8618

    public void Hydrate(ICustomer customer)
    {
        Id = customer.Id;
        Cpf = customer.Cpf;
        Name = customer.Name;
    }

    public ICustomer GetCustomer() => Customer.Rebuild(Id, Cpf, Name);
}