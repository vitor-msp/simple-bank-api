using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class CreditInput
{
    [Required]
    [Range(0.0, double.MaxValue)]
    public double Value { get; set; }

    public CreditFields GetFields()
    {
        return new CreditFields() { Value = Value };
    }
}