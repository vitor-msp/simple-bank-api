namespace SimpleBankApi.Application.Input;

public class TransferInput
{
    public double Value { get; set; }
    public int RecipientAccountNumber { get; set; }
}