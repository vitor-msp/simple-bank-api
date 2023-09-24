using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Dto;
using Exceptions;

namespace Models;

public class Transfer : Transaction
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public double Value { get; set; }
    public Account Sender { get; set; }
    public Account Recipient { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Transfer() { }

    public Transfer(TransferDto transferDto, Account sender, Account recipient)
    {
        ValueIsValid(transferDto.Value);
        Value = transferDto.Value;
        Sender = sender;
        Recipient = recipient;
    }

    private void ValueIsValid(double value)
    {
        if(value <= 0) throw new TransactionException("The transfer value must be greater than zero.");
    }

    public TransactionTransferDto GetData(Account account)
    {
        double value;
        if (account.Equals(Sender))
        {
            value = -1 * Value;
        }
        else if (account.Equals(Recipient))
        {
            value = Value;
        }
        else
        {
            throw new TransactionException("Transfer does not belong to the account.");
        }
        var dto = new TransactionTransferDto(
            TransactionType.Transfer, value, CreatedAt, Sender.GetPublicData(), Recipient.GetPublicData());
        return dto;
    }
}