namespace Controllers;

public class ErrorDto
{
    public string ApiErrorMessage { get; set; } = "";

    public ErrorDto(string message)
    {
        ApiErrorMessage = message;
    }
}