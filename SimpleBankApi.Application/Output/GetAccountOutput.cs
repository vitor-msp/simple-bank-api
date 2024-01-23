using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Output;

public class GetAccountOutput
{
    public int AccountNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public Customer? Owner { get; set; }

    public class Customer
    {
        public string Name { get; set; } = "";

        public string Cpf { get; set; } = "";
    }

    public static GetAccountOutput Build(IAccount account)
    {
        return new GetAccountOutput()
        {
            AccountNumber = account.GetFields().AccountNumber,
            CreatedAt = account.GetFields().CreatedAt,
            Owner = new Customer()
            {
                Cpf = account.Owner?.GetFields().Cpf ?? "",
                Name = account.Owner?.GetFields().Name ?? "",
            }
        };
    }
}