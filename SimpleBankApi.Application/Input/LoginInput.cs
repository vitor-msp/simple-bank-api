namespace SimpleBankApi.Application.Input;

public class LoginInput
{
    public int AccountNumber { get; set; }
    public string Password { get; set; } = "";
}