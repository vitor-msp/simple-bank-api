using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class UpdateAccountInput
{
    [Required]
    public string Name { get; set; } = "";

    public CustomerUpdateableFields GetFields()
    {
        return new CustomerUpdateableFields() { Name = Name };
    }
}