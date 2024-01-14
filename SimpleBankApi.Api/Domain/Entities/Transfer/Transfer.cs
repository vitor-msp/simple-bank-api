using System.Transactions;

namespace Models;

public class Transfer : Transaction
{
    public Account? Sender { get; set; }
    public Account? Recipient { get; set; }

    public Transfer(TransferFields fields) : base(fields) { }

    /// to check
    public TransactionTransferDto GetData(Account account)
    {
        double value;
        if (account.Equals(Sender))
        {
            value = -1 * _fields.Value;
        }
        else if (account.Equals(Recipient))
        {
            value = _fields.Value;
        }
        else
        {
            throw new TransactionException("Transfer does not belong to the account.");
        }
        var dto = new TransactionTransferDto(
            TransactionType.Transfer, value, _fields.CreatedAt, Sender.GetPublicData(), Recipient.GetPublicData());
        return dto;
    }
}