using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Dto;

public class TransactionDto
{
    public string Type { get; set; } = "";
    public CreditDto? CreditDto { get; set; }
    public DebitDto? DebitDto { get; set; }
    public TransferDto? TransferDto { get; set; }

    public static TransactionDto BuildFromCredit(ICredit credit)
    {
        return new TransactionDto()
        {
            Type = TransactionType.Credit,
            CreditDto = CreditDto.Build(credit)
        };
    }

    public static TransactionDto BuildFromDebit(IDebit debit)
    {
        return new TransactionDto()
        {
            Type = TransactionType.Debit,
            DebitDto = DebitDto.Build(debit)
        };
    }

    public static TransactionDto BuildFromTransfer(ITransfer transfer, IAccount account)
    {
        return new TransactionDto()
        {
            Type = TransactionType.Transfer,
            TransferDto = TransferDto.Build(transfer, account)
        };
    }
}