namespace Models;

public class Debit : Transaction
{
    public Account? Account { get; set; }

    public Debit(DebitFields fields) : base(fields)
    {
        fields.Value = -1 * fields.Value;
    }

    /// to check
    public (string, double, DateTime) GetDataWithoutAccount()
    {
        return (TransactionType.Debit, _fields.Value, _fields.CreatedAt);
    }
}