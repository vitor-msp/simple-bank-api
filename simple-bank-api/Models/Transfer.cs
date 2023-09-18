using System.ComponentModel.DataAnnotations;
using Dto;
using Exceptions;

namespace Models;

public class Transfer : Transaction
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public Customer Sender { get; set; }
    public Customer Recipient { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Transfer() { }

    public Transfer(TransferDto transferDto, Customer sender, Customer recipient)
    {
        if (!ValueIsValid(transferDto.Value))
            throw new TransactionException("the value must be greater than zero");
        Value = transferDto.Value;
        Sender = sender;
        Recipient = recipient;
    }

    private bool ValueIsValid(double value)
    {
        return value > 0;
    }
}