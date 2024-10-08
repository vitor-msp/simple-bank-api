using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Application.Output;

public class DebitDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public static DebitDto Build(IDebit debit)
        => new()
        {
            Value = debit.Value.GetBrazilianCurrency(),
            CreatedAt = debit.CreatedAt,
        };
}