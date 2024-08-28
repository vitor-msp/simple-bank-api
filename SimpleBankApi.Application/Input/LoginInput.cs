using System.ComponentModel.DataAnnotations;

namespace SimpleBankApi.Application.Input;

public class LoginInput
{
    [Required]
    public int AccountNumber { get; set; }

    [Required]
    public string Password { get; set; } = "";
}