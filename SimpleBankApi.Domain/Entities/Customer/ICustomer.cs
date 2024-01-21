namespace SimpleBankApi.Domain.Entities;

public interface ICustomer
{
    public CustomerFields GetFields();

    public void Update(CustomerUpdateableFields fields);
}