using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class DebitInput : Input
{
    [Required]
    [Range(0.0, double.MaxValue)]
    public double Value { get; set; }

    internal IDebit GetDebit(IAccount account)
        => new Debit() { Value = Value, Account = account };
}