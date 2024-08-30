namespace SimpleBankApi.Domain.Entities;

public interface ITransaction
{
    int Id { get; }
    DateTime CreatedAt { get; }
    double Value { get; init; }
}