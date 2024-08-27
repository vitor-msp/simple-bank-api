using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Services;

public class CreateAccountInput
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(11)]
    public string Cpf { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required]
    [Compare("Password")]
    public string PasswordConfirmation { get; set; } = "";

    public Role Role { get; set; } = Role.Customer;

    public CustomerFields GetCustomerFields()
        => new()
        {
            Name = Name,
            Cpf = Cpf,
        };

    public AccountFields GetAccountFields()
        => new()
        {
            Role = Role,
        };
}