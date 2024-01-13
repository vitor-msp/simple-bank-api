using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Dto;
using Exceptions;

namespace Models;

public class Credit : Transaction
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public double Value { get; set; }
    public Account Account { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Credit() { }

    public Credit(CreditDto creditDto, Account account)
    {
        ValidateValue(creditDto.Value);
        Value = creditDto.Value;
        Account = account;
    }

    private void ValidateValue(double value)
    {
        if (value <= 0) throw new TransactionException("The credit value must be greater than zero.");
    }

    public TransactionCreditDebitDto GetDataWithoutAccount()
    {
        return new TransactionCreditDebitDto(TransactionType.Credit, Value, CreatedAt);
    }
}