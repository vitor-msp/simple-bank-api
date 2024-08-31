using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Application.Output;

public class TransactionDto
{
    public string Type { get; set; } = "";
    public CreditDto? CreditDto { get; set; }
    public DebitDto? DebitDto { get; set; }
    public TransferDto? TransferDto { get; set; }

    public static TransactionDto BuildFromCredit(ICredit credit)
        => new()
        {
            Type = TransactionType.Credit.ToString(),
            CreditDto = CreditDto.Build(credit)
        };

    public static TransactionDto BuildFromDebit(IDebit debit)
        => new()
        {
            Type = TransactionType.Debit.ToString(),
            DebitDto = DebitDto.Build(debit)
        };

    public static TransactionDto BuildFromTransfer(ITransfer transfer, IAccount account)
        => new()
        {
            Type = TransactionType.Transfer.ToString(),
            TransferDto = TransferDto.Build(transfer, account)
        };
}