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
    public string TransactionType { get; set; } = "";
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public TransactionCreditDebitDto(string transactionType, double value, DateTime createdAt)
    {
        TransactionType = transactionType;
        Value = CurrencyHelper.GetBrazilianCurrency(value);
        CreatedAt = createdAt;
    }
}

public class TransactionTransferDto
{
    public string TransactionType { get; set; } = "";
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public TransactionCustomerDto Sender { get; set; }
    public TransactionCustomerDto Recipient { get; set; }

    public TransactionTransferDto(string transactionType, double value,
        DateTime createdAt, TransactionCustomerDto sender, TransactionCustomerDto recipient)
    {
        TransactionType = transactionType;
        Value = CurrencyHelper.GetBrazilianCurrency(value);
        CreatedAt = createdAt;
        Sender = sender;
        Recipient = recipient;
    }
}

public class TransactionCustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}