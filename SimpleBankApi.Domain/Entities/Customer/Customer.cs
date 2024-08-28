namespace SimpleBankApi.Domain.Entities;

public class Customer : ICustomer
{
    public int Id { get; }
    public required string Cpf { get; set; }
    public required string Name { get; set; }

    public Customer() { }

    private Customer(int id)
    {
        Id = id;
    }

    public static Customer Rebuild(int id, string cpf, string name)
        => new(id)
        {
            Cpf = cpf,
            Name = name,
        };
}