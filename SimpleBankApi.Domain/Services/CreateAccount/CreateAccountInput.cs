using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Exceptions;
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

    internal string? PasswordHash { get; set; }

    private Customer GetCustomer()
        => new()
        {
            Name = Name,
            Cpf = Cpf,
        };

    internal Account GetAccount()
        => new()
        {
            Role = Role,
            Owner = GetCustomer(),
            PasswordHash = PasswordHash ?? throw new DomainException("Missing password hash."),
        };
}