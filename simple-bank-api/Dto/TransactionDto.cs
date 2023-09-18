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
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransactionTransferDto
{
    public string TransactionType { get; set; } = "";
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public TransactionCustomerDto Sender { get; set; }
    public TransactionCustomerDto Recipient { get; set; }

    public TransactionTransferDto(string transactionType, double value,
        DateTime createdAt, TransactionCustomerDto sender, TransactionCustomerDto recipient)
    {
        TransactionType = transactionType;
        Value = value;
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