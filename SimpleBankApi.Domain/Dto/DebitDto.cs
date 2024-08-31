using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Domain.Dto;

public class DebitDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public static DebitDto Build(IDebit debit)
        => new()
        {
            Value = (-1 * debit.Value).GetBrazilianCurrency(),
            CreatedAt = debit.CreatedAt,
        };
}