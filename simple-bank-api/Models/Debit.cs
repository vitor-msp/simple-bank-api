using System.ComponentModel.DataAnnotations;

namespace Models;

public class Debit : Transaction
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Customer Customer { get; set; }

    public Debit(double value, Customer customer)
    {
        if (!ValueIsValid(value)) throw new Exception("the value must be greater than zero ");
        Value = -1 * value;
        Customer = customer;
    }

    private bool ValueIsValid(double value)
    {
        return value > 0;
    }
}