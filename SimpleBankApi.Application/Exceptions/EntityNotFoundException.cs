namespace SimpleBankApi.Application.Exceptions;

public class EntityNotFoundException(string message) : Exception(message) { }