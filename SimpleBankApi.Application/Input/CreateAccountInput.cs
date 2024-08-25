using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

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

    public CustomerFields GetFields()
    {
        return new CustomerFields()
        {
            Name = Name,
            Cpf = Cpf,
        };
    }
}