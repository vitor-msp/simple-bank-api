using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Helpers;

namespace SimpleBankApi.Domain.Dto;

public class DebitDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public static DebitDto Build(IDebit debit)
    {
        return new DebitDto()
        {
            Value = CurrencyHelper.GetBrazilianCurrency(-1 * debit.GetFields().Value),
            CreatedAt = debit.GetFields().CreatedAt,
        };
    }
}