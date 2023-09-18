using Helpers;

namespace Dto;

public static class TransactionType
{
    public static readonly string Credit = "credit";
    public static readonly string Debit = "debit";
    public static readonly string Transfer = "transfer";
}

public class TransactionCreditDebitDto
{
    public string Type { get; set; } = "";
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public TransactionCreditDebitDto(string type, double value, DateTime createdAt)
    {
        Type = type;
        Value = CurrencyHelper.GetBrazilianCurrency(value);
        CreatedAt = createdAt;
    }
}

public class TransactionTransferDto
{
    public string Type { get; set; } = "";
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public TransactionAccountDto Sender { get; set; }
    public TransactionAccountDto Recipient { get; set; }

    public TransactionTransferDto(string type, double value,
        DateTime createdAt, TransactionAccountDto sender, TransactionAccountDto recipient)
    {
        Type = type;
        Value = CurrencyHelper.GetBrazilianCurrency(value);
        CreatedAt = createdAt;
        Sender = sender;
        Recipient = recipient;
    }
}

public class TransactionAccountDto
{
    public int AccountNumber { get; set; }
    public string Name { get; set; } = "";
}