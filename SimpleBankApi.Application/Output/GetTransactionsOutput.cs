namespace SimpleBankApi.Application.Output;

public class GetTransactionsOutput
{
    public StatementDto Statement { get; set; } = new();
}