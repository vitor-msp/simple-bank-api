using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

[Index(nameof(Cpf), IsUnique = true)]
public class CustomerDB
{
    [Key]
    public int Id { get; set; }
    public string Cpf { get; set; } = "";
    public string Name { get; set; } = "";

    public CustomerDB() { }

    public CustomerDB(Customer customer)
    {
        Hydrate(customer);
    }

    public Customer GetEntity()
    {
        return new Customer(CustomerFields.Rebuild(Id, Cpf, Name));
    }

    public void Hydrate(Customer customer)
    {
        var fields = customer.GetFields();
        Id = fields.Id;
        Cpf = fields.Cpf ?? "";
        Name = fields.Name ?? "";
    }
}