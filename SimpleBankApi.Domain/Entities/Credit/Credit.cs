namespace SimpleBankApi.Domain.Entities;

public class Credit : Transaction, ICredit
{
    public IAccount? Account { get; set; }

    public Credit(CreditFields fields) : base(fields) { }

    /// to check
    public (string, double, DateTime) GetDataWithoutAccount()
    {
        return (TransactionType.Credit, _fields.Value, _fields.CreatedAt);
    }
}