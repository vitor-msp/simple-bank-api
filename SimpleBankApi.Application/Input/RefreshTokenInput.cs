using System.ComponentModel.DataAnnotations;

namespace SimpleBankApi.Application.Input;

public class RefreshTokenInput
{
    [Required]
    public int AccountNumber { get; set; }

    [Required]
    public string RefreshToken { get; set; } = "";
}