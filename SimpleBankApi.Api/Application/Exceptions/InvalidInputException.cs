namespace Application.Exceptions;

public class InvalidInputException : ApplicationException
{
    public InvalidInputException(string message) : base(message) { }
}