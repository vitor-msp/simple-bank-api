using System.ComponentModel.DataAnnotations;
using Exceptions;
using Dto;

namespace Models;

public class Debit : Transaction
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Customer Customer { get; set; }

    public Debit() { }

    public Debit(DebitDto debitDto, Customer customer)
    {
        if (!ValueIsValid(debitDto.Value))
            throw new TransactionException("the value must be greater than zero");
        Value = debitDto.Value;
        Customer = customer;
    }

    private bool ValueIsValid(double value)
    {
        return value > 0;
    }
}