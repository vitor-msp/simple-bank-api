namespace SimpleBankApi.Domain.Entities;

public interface ICustomer
{
    int Id { get; }
    string Cpf { get; set; }
    string Name { get; set; }
}