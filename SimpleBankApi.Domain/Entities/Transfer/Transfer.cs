using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Entities;

public class Transfer : Transaction, ITransfer
{
    public Account? Sender { get; set; }
    public Account? Recipient { get; set; }

    public Transfer(TransferFields fields) : base(fields) { }

    /// to check
    public TransactionTransferDto GetData(Account account)
    {
        double value;
        if (Sender != null && account.GetFields().AccountNumber == Sender.GetFields().AccountNumber)
        {
            value = -1 * _fields.Value;
        }
        else if (Recipient != null && account.GetFields().AccountNumber == Recipient.GetFields().AccountNumber)
        {
            value = _fields.Value;
        }
        else
        {
            throw new DomainException("Transfer does not belong to the account.");
        }
        var dto = new TransactionTransferDto(
            TransactionType.Transfer, value, _fields.CreatedAt, Sender.GetPublicData(), Recipient.GetPublicData());
        return dto;
    }
}