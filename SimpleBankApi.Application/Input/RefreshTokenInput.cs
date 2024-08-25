namespace SimpleBankApi.Application.Input;

public class RefreshTokenInput
{
    public int AccountNumber { get; set; }
    public string RefreshToken { get; set; } = "";
}