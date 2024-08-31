namespace SimpleBankApi.Application.Output;

public class StatementDto
{
    public List<TransactionDto> Transactions { get; set; } = [];
}