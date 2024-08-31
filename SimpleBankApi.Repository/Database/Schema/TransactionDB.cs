using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Repository.Database.Schema;

public class TransactionDB
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public AccountDB? OperatingAccount { get; set; }
    public AccountDB? RelatedAccount { get; set; }
    public TransactionType TransactionType { get; set; }

    public TransactionDB() { }

    public TransactionDB(ICredit credit)
    {
        Id = credit.Id;
        Value = credit.Value;
        CreatedAt = credit.CreatedAt;
        TransactionType = TransactionType.Credit;
    }

    public TransactionDB(IDebit debit)
    {
        Id = debit.Id;
        Value = debit.Value;
        CreatedAt = debit.CreatedAt;
        TransactionType = TransactionType.Debit;
    }

    private TransactionDB(ITransfer transfer, double value)
    {
        Id = transfer.Id;
        Value = value;
        CreatedAt = transfer.CreatedAt;
        TransactionType = TransactionType.Transfer;
    }

    public static (TransactionDB, TransactionDB) BuildTransfer(ITransfer transfer, AccountDB sender, AccountDB recipient)
    {
        var debit = new TransactionDB(transfer, value: -1 * transfer.Value)
        { OperatingAccount = sender, RelatedAccount = recipient };

        var credit = new TransactionDB(transfer, value: transfer.Value)
        { OperatingAccount = recipient, RelatedAccount = sender };

        return (debit, credit);
    }

    public ICredit GetCredit(IAccount account)
    {
        if (TransactionType != TransactionType.Credit)
            throw new Exception("Transaction is not a credit.");
        return Credit.Rebuild(Id, CreatedAt, Value, account);
    }

    public IDebit GetDebit(IAccount account)
    {
        if (TransactionType != TransactionType.Debit)
            throw new Exception("Transaction is not a debit.");
        return Debit.Rebuild(Id, CreatedAt, Value, account);
    }

    public ITransfer GetTransfer()
    {
        if (TransactionType != TransactionType.Transfer)
            throw new Exception("Transaction is not a transfer.");

        if (OperatingAccount == null || OperatingAccount.Owner == null)
            throw new Exception();

        var operatingOwner = OperatingAccount.Owner.GetEntity();
        var operatingAccount = OperatingAccount.GetEntity(operatingOwner);

        if (RelatedAccount == null || RelatedAccount.Owner == null)
            throw new Exception();

        var relatedOwner = RelatedAccount.Owner.GetEntity();
        var relatedAccount = RelatedAccount.GetEntity(relatedOwner);

        if (Value >= 0)
            return Transfer.Rebuild(Id, CreatedAt, Value, sender: relatedAccount, recipient: operatingAccount);

        return Transfer.Rebuild(Id, CreatedAt, -1 * Value, sender: operatingAccount, recipient: relatedAccount);
    }
}