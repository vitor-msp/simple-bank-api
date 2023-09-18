using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Dto;

namespace Models;

public class Customer
{

    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Cpf { get; set; } = "";
    [JsonIgnore]
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

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (!obj.GetType().Equals(this.GetType())) return false;
        Customer customerToCompare = (Customer)obj;
        return customerToCompare.Id == Id;
    }


    public TransactionCustomerDto GetPublicData()
    {
        return new TransactionCustomerDto() { Id = Id, Name = Name };
    }
}