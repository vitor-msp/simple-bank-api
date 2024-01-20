using SimpleBankApi.Domain.Entities;
using Xunit;

namespace SimpleBankApi.Tests;

public class CustomerTest
{
    private readonly int _customerId = 1;
    private readonly string _customerCpf = "01234567890";
    private readonly string _customerName = "fulano de tal";

    public CustomerTest() { }

    private Customer GetCustomerExample()
    {
        return new Customer(CustomerFields.Rebuild(_customerId, _customerCpf, _customerName));
    }

    [Fact]
    public void RebuildCustomer()
    {
        int id = 15632;
        string cpf = "01234567890";
        string name = "fulano de tal";

        var customer = new Customer(CustomerFields.Rebuild(id, cpf, name));

        Assert.Equal(id, customer.GetFields().Id);
        Assert.Equal(cpf, customer.GetFields().Cpf);
        Assert.Equal(name, customer.GetFields().Name);
    }

    [Fact]
    public void UpdateCustomer()
    {
        var customer = GetCustomerExample();
        string newName = "joao da silva";
        var input = new CustomerUpdateableFields() { Name = newName };

        customer.Update(input);

        Assert.Equal(newName, customer.GetFields().Name);
        Assert.Equal(_customerId, customer.GetFields().Id);
        Assert.Equal(_customerCpf, customer.GetFields().Cpf);
    }
}