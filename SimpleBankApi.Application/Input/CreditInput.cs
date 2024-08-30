using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class CreditInput : Input
{
    [Required]
    [Range(0.0, double.MaxValue)]
    public double Value { get; set; }

    internal ICredit GetCredit(IAccount account)
        => new Credit() { Value = Value, Account = account };
}