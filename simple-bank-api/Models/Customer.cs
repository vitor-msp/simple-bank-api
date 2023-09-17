namespace Models;

public class Customer
{

    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string Cpf { get; set; }
    public bool Active { get; set; }

    public Customer()
    {
        Name = "";
        Cpf = "";
        Active = true;
    }

    public void Inactivate()
    {
        Active = false;
    }

    public void Update(Customer updatedCustomer)
    {
        Name = updatedCustomer.Name;
        Cpf = updatedCustomer.Cpf;
    }
}