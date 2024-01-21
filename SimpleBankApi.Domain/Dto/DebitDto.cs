using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Domain.Dto;

public class DebitDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public static DebitDto Build(IDebit debit)
    {
        return new DebitDto()
        {
            Value = (-1 * debit.GetFields().Value).GetBrazilianCurrency(),
            CreatedAt = debit.GetFields().CreatedAt,
        };
    }
}