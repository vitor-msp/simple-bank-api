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
        if (!ValueIsValid(creditDto.Value))
            throw new TransactionException("the value must be greater than zero");
        Value = creditDto.Value;
        Account = account;
    }

    private bool ValueIsValid(double value)
    {
        return value > 0;
    }

    public TransactionCreditDebitDto GetDataWithoutAccount()
    {
        return new TransactionCreditDebitDto(TransactionType.Credit, Value, CreatedAt);
    }
}