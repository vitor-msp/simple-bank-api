using SimpleBankApi.Domain;

namespace SimpleBankApi.Application.Output;

public class GetAllAccountsOutput
{
    public List<TransactionAccountDto> Accounts { get; set; } = new();
}