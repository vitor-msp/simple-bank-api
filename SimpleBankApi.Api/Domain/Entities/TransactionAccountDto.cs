namespace Models;

public class TransactionAccountDto
{
    public int AccountNumber { get; set; }
    public string Name { get; set; } = "";

    public TransactionAccountDto() { }

    public TransactionAccountDto((int, string) input)
    {
        (AccountNumber, Name) = input;
    }
}