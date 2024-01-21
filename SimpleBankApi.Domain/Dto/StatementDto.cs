namespace SimpleBankApi.Domain.Dto;

public class StatementDto
{
    public List<TransactionDto> Transactions { get; set; } = new();
}