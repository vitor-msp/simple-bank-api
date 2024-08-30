using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class TransferInput : Input
{
    [Required]
    [Range(0.0, double.MaxValue)]
    public double Value { get; set; }

    [Required]
    [Range(0.0, int.MaxValue)]
    public int RecipientAccountNumber { get; set; }

    internal ITransfer GetTransfer(IAccount sender, IAccount recipient)
            => new Transfer() { Value = Value, Sender = sender, Recipient = recipient };
}