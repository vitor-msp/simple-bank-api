using System.Globalization;

namespace SimpleBankApi.Domain.Helpers;

public static class CurrencyHelper
{
    public static string GetBrazilianCurrency(double value)
    {
        return value.ToString("c", CultureInfo.GetCultureInfo("pt-BR"));
    }
}