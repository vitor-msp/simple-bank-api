using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Application.Output;

public class CreditDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public static CreditDto Build(ICredit credit)
        => new()
        {
            Value = credit.Value.GetBrazilianCurrency(),
            CreatedAt = credit.CreatedAt,
        };
}