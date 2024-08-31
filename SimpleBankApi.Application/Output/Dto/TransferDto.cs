using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Application.Output;

public class TransferDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public AccountDto? Sender { get; set; }
    public AccountDto? Recipient { get; set; }

    public static TransferDto Build(ITransfer transfer, IAccount account)
    {
        var value = transfer.Value;
        if (transfer.Sender.Equals(account))
            value = -1 * value;
        return new()
        {
            Value = value.GetBrazilianCurrency(),
            CreatedAt = transfer.CreatedAt,
            Sender = AccountDto.Build(transfer.Sender),
            Recipient = AccountDto.Build(transfer.Recipient)
        };
    }
}