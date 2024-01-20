using System.Collections;

namespace SimpleBankApi.Application.Output;

public class GetTransactionsOutput
{
    public ArrayList Transactions { get; set; } = new();
}