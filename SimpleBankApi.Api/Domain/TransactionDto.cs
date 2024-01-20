using Helpers;

namespace Models;

public class TransactionCreditDebitDto
{
    public string Type { get; set; } = "";
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public TransactionCreditDebitDto() { }

    public TransactionCreditDebitDto((string, double, DateTime) input)
    {
        Type = input.Item1;
        Value = CurrencyHelper.GetBrazilianCurrency(input.Item2);
        CreatedAt = input.Item3;
    }

    public TransactionCreditDebitDto(string type, double value, DateTime createdAt)
    {
        Type = type;
        Value = CurrencyHelper.GetBrazilianCurrency(value);
        CreatedAt = createdAt;
    }
}