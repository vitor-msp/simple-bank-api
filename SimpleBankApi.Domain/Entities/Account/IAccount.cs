namespace SimpleBankApi.Domain.Entities;

public interface IAccount
{
    public ICustomer? Owner { get; set; }

    public AccountFields GetFields();

    public void Update(CustomerUpdateableFields fields);

    public void Inactivate();

    public bool Equals(object? obj);

    public int GetHashCode();

    public void UpdateRefreshToken(string token);
}