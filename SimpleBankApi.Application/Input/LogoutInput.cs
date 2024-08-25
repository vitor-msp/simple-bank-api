namespace SimpleBankApi.Application.Input;

public class LogoutInput
{
    public int AccountNumber { get; set; }
    public string RefreshToken { get; set; } = "";
}