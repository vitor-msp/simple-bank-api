using System.Globalization;

namespace SimpleBankApi.Domain.Extensions;

public static class DoubleExtension
{
    public static string GetBrazilianCurrency(this double value)
        => value.ToString("c", CultureInfo.GetCultureInfo("pt-BR"));
}