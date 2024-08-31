using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Output;

public class AccountDto
{
    public int AccountNumber { get; set; }
    public string Name { get; set; } = "";

    public static AccountDto Build(IAccount account)
        => new()
        {
            AccountNumber = account.AccountNumber,
            Name = account.Owner.Name,
        };
}