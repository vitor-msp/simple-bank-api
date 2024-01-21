namespace SimpleBankApi.Domain.Entities;

public class Debit : Transaction, IDebit
{
    public IAccount? Account { get; set; }

    public Debit(DebitFields fields) : base(fields) { }

    /// to check
    public (string, double, DateTime) GetDataWithoutAccount()
    {
        return (TransactionType.Debit, _fields.Value, _fields.CreatedAt);
    }
}