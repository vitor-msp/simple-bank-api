namespace SimpleBankApi.Domain.Entities;

public class Credit : Transaction, ICredit
{
    public IAccount? Account { get; set; }

    public Credit(CreditFields fields) : base(fields) { }
}