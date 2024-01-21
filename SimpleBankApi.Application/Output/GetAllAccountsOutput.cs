using SimpleBankApi.Domain.Dto;

namespace SimpleBankApi.Application.Output;

public class GetAllAccountsOutput
{
    public List<AccountDto> Accounts { get; set; } = new();
}