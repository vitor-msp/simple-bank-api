using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Dto;

namespace Models;

public class Customer
{

    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public string Cpf { get; set; } = "";
    public string Name { get; set; } = "";

    public Customer() { }

    public Customer(AccountCreateDto dto)
    {
        Cpf = dto.Cpf;
        Name = dto.Name;
    }
}