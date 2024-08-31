namespace SimpleBankApi.Application.Exceptions;

public class InvalidInputException(string message) : Exception(message) {}