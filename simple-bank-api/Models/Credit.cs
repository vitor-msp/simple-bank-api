using System.ComponentModel.DataAnnotations;
using Dto;
using Exceptions;

namespace Models;

public class Credit : Transaction
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public Customer Customer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Credit() { }

    public Credit(CreditDto creditDto, Customer customer)
    {
        if (!ValueIsValid(creditDto.Value))
            throw new TransactionException("the value must be greater than zero");
        Value = creditDto.Value;
        Customer = customer;
    }

    private bool ValueIsValid(double value)
    {
        return value > 0;
    }
}