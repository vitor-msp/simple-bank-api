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

    public Customer Hydrate(CustomerCreateDto dto)
    {
        Name = dto.Name;
        Cpf = dto.Cpf;
        return this;
    }

    public void Update(CustomerUpdateDto dto)
    {
        Name = dto.Name;
    }

    public void Inactivate()
    {
        Active = false;
    }
}