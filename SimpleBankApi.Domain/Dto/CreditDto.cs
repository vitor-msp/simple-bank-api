using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Extensions;

namespace SimpleBankApi.Domain.Dto;

public class CreditDto
{
    public string Value { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public static CreditDto Build(ICredit credit)
    {
        return new CreditDto()
        {
            Value = credit.Value.GetBrazilianCurrency(),
            CreatedAt = credit.CreatedAt,
        };
    }
}