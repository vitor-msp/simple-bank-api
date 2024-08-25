namespace SimpleBankApi.Application.Output;

public class LoginOutput
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}