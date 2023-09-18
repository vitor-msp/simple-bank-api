using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Dto;

namespace Models;

public class Account
{

    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public Customer Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [JsonIgnore]
    public bool Active { get; set; } = true;

    public Account() { }

    public Account(Customer owner)
    {
        AccountNumber = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());
        Owner = owner;
    }

    public void Update(AccountUpdateDto dto)
    {
        Owner.Name = dto.Name;
    }

    public void Inactivate()
    {
        Active = false;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (!obj.GetType().Equals(this.GetType())) return false;
        Account accountToCompare = (Account)obj;
        return accountToCompare.AccountNumber == AccountNumber;
    }

    public TransactionAccountDto GetPublicData()
    {
        return new TransactionAccountDto() { AccountNumber = AccountNumber, Name = Owner.Name };
    }
}