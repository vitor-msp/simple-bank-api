namespace SimpleBankApi.Api.Presenters;

public class ErrorPresenter(string message)
{
    public string ApiErrorMessage { get; set; } = message;
}