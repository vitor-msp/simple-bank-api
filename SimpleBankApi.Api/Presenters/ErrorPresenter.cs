namespace SimpleBankApi.Api.Presenters;

public class ErrorPresenter
{
    public string ApiErrorMessage { get; set; } = "";

    public ErrorPresenter(string message)
    {
        ApiErrorMessage = message;
    }
}