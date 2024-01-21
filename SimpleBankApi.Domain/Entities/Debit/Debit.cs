namespace SimpleBankApi.Domain.Entities;

public class Debit : Transaction, IDebit
{
    public IAccount? Account { get; set; }

    public Debit(DebitFields fields) : base(fields) { }
}