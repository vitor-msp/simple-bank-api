using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Dto;

public class AccountDto
{
    public int AccountNumber { get; set; }
    public string Name { get; set; } = "";

    public static AccountDto Build(IAccount account)
    {
        return new AccountDto()
        {
            AccountNumber = account.GetFields().AccountNumber,
            Name = account.Owner?.GetFields().Name ?? ""
        };
    }
}