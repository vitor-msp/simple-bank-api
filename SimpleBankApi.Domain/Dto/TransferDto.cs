using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Exceptions;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Domain.Dto;

public class TransferDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public AccountDto? Sender { get; set; }
    public AccountDto? Recipient { get; set; }

    public static TransferDto Build(ITransfer transfer, IAccount account)
        => new()
        {
            Value = GetTransferValue(transfer, account),
            CreatedAt = transfer.CreatedAt,
            Sender = AccountDto.Build(transfer.Sender),
            Recipient = AccountDto.Build(transfer.Recipient)
        };

    private static string GetTransferValue(ITransfer transfer, IAccount account)
    {
        double value;
        if (transfer.Sender.AccountNumber == account.AccountNumber)
        {
            value = -1 * transfer.Value;
        }
        else if (transfer.Recipient.AccountNumber == account.AccountNumber)
        {
            value = transfer.Value;
        }
        else
        {
            throw new DomainException("Transfer does not belong to the account.");
        }
        return value.GetBrazilianCurrency();
    }
}