using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class TransferInput
{
    [Required]
    [Range(0.0, double.MaxValue)]
    public double Value { get; set; }

    [Required]
    [Range(0.0, int.MaxValue)]
    public int RecipientAccountNumber { get; set; }

    public TransferFields GetFields()
    {
        return new TransferFields() { Value = Value };
    }
}