using System.ComponentModel.DataAnnotations;
using Dto;

namespace Models;

public class Customer
{

    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Cpf { get; set; } = "";
    public bool Active { get; set; } = true;

    public Customer Hydrate(CustomerDto dto)
    {
        Name = dto.Name;
        Cpf = dto.Cpf;
        return this;
    }

    public void Inactivate()
    {
        Active = false;
    }

    public void Update(Customer updatedCustomer)
    {
        Name = updatedCustomer.Name;
        Cpf = updatedCustomer.Cpf;
    }
}