using Models;

namespace Application;

public class GetAllAccountsOutput
{
    public List<TransactionAccountDto> Accounts { get; set; } = new();
}