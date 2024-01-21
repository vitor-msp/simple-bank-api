using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Repository.Database.Schema;

[Index(nameof(Cpf), IsUnique = true)]
public class CustomerDB
{
    [Key]
    public int Id { get; set; }
    public string Cpf { get; set; } = "";
    public string Name { get; set; } = "";

    public CustomerDB() { }

    public CustomerDB(ICustomer customer)
    {
        Hydrate(customer);
    }

    public ICustomer GetEntity()
    {
        return new Customer(CustomerFields.Rebuild(Id, Cpf, Name));
    }

    public void Hydrate(ICustomer customer)
    {
        var fields = customer.GetFields();
        Id = fields.Id;
        Cpf = fields.Cpf ?? "";
        Name = fields.Name ?? "";
    }
}