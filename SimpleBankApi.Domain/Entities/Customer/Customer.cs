namespace SimpleBankApi.Domain.Entities;

public class Customer : ICustomer
{
    private readonly CustomerFields _fields;

    public Customer(CustomerFields fields)
    {
        _fields = fields;
    }

    public CustomerFields GetFields() => _fields;

    public void Update(CustomerUpdateableFields fields)
    {
        if (fields.Name != null) _fields.Name = fields.Name;
    }
}