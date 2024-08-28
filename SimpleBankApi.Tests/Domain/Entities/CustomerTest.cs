using SimpleBankApi.Domain.Entities;
using Xunit;

namespace SimpleBankApi.Tests.Domain;

public class CustomerTest
{
    [Fact]
    public void RebuildCustomer()
    {
        int id = 15632;
        string cpf = "01234567890";
        string name = "fulano de tal";

        var customer = Customer.Rebuild(id, cpf, name);

        Assert.Equal(id, customer.Id);
        Assert.Equal(cpf, customer.Cpf);
        Assert.Equal(name, customer.Name);
    }
}