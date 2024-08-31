using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Repository.Database.Schema;

public class TransactionDB
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Value { get; set; }
    public AccountDB OperatingAccount { get; set; }
    public AccountDB? RelatedAccount { get; set; }
    public TransactionDB? RelatedTransaction { get; set; }
    public TransactionType TransactionType { get; set; }

#pragma warning disable CS8618
    public TransactionDB() { }

    public TransactionDB(ICredit credit)
    {
        Id = credit.Id;
        CreatedAt = credit.CreatedAt;
        Value = credit.Value;
        TransactionType = TransactionType.Credit;
    }

    public TransactionDB(IDebit debit)
    {
        Id = debit.Id;
        CreatedAt = debit.CreatedAt;
        Value = debit.Value;
        TransactionType = TransactionType.Debit;
    }

    private TransactionDB(ITransfer transfer, double value)
    {
        Id = transfer.Id;
        CreatedAt = transfer.CreatedAt;
        Value = value;
        TransactionType = TransactionType.Transfer;
    }
#pragma warning restore CS8618

    public static (TransactionDB, TransactionDB) BuildTransfer(ITransfer transfer, AccountDB sender, AccountDB recipient)
    {
        var debit = new TransactionDB(transfer, value: -1 * transfer.Value)
        { OperatingAccount = sender, RelatedAccount = recipient };

        var credit = new TransactionDB(transfer, value: transfer.Value)
        { OperatingAccount = recipient, RelatedAccount = sender };

        return (debit, credit);
    }

    public ICredit GetCredit()
    {
        if (TransactionType != TransactionType.Credit)
            throw new Exception("Transaction is not a credit.");

        return Credit.Rebuild(Id, CreatedAt, Value, OperatingAccount.GetAccount());
    }

    public IDebit GetDebit()
    {
        if (TransactionType != TransactionType.Debit)
            throw new Exception("Transaction is not a debit.");

        return Debit.Rebuild(Id, CreatedAt, Value, OperatingAccount.GetAccount());
    }

    public ITransfer GetTransfer()
    {
        if (TransactionType != TransactionType.Transfer)
            throw new Exception("Transaction is not a transfer.");

        if (RelatedAccount == null)
            throw new Exception("Missing related account.");

        var operatingAccount = OperatingAccount.GetAccount();
        var relatedAccount = RelatedAccount.GetAccount();

        if (Value >= 0)
            return Transfer.Rebuild(Id, CreatedAt, Value, sender: relatedAccount, recipient: operatingAccount);

        return Transfer.Rebuild(Id, CreatedAt, -1 * Value, sender: operatingAccount, recipient: relatedAccount);
    }
}