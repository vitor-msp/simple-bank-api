namespace Models;

public class Credit : Transaction
{
    public Account? Account { get; set; }

    public Credit(CreditFields fields) : base(fields) { }

    /// to check
    public (string, double, DateTime) GetDataWithoutAccount()
    {
        return (TransactionType.Credit, _fields.Value, _fields.CreatedAt);
    }
}