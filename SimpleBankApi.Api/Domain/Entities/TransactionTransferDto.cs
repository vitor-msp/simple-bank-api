using Helpers;

namespace Models;

public class TransactionTransferDto
{
    public string Type { get; set; } = "";
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public TransactionAccountDto Sender { get; set; }
    public TransactionAccountDto Recipient { get; set; }

    public TransactionTransferDto() { }

    public TransactionTransferDto(string type, double value,
        DateTime createdAt, (int, string) sender, (int, string) recipient)
    {
        Type = type;
        Value = CurrencyHelper.GetBrazilianCurrency(value);
        CreatedAt = createdAt;
        Sender = new TransactionAccountDto()
        {
            AccountNumber = sender.Item1,
            Name = sender.Item2
        };
        Recipient = new TransactionAccountDto()
        {
            AccountNumber = recipient.Item1,
            Name = recipient.Item2
        }; ;
    }
}