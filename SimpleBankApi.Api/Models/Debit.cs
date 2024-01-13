using System.ComponentModel.DataAnnotations;
using Exceptions;
using Dto;
using System.Text.Json.Serialization;

namespace Models;

public class Debit : Transaction
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Account Account { get; set; }

    public Debit() { }

    public Debit(DebitDto debitDto, Account account)
    {
        ValueIsValid(debitDto.Value);
        Value = -1 * debitDto.Value;
        Account = account;
    }

    private void ValueIsValid(double value)
    {
        if(value <= 0)throw new TransactionException("The debit value must be greater than zero.");
    }

    public TransactionCreditDebitDto GetDataWithoutAccount()
    {
        return new TransactionCreditDebitDto(TransactionType.Debit, Value, CreatedAt);
    }
}