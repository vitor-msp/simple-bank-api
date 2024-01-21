using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Entities;

public class Account : IAccount
{
    private readonly AccountFields _fields;

    public ICustomer? Owner { get; set; }

    public Account(AccountFields fields)
    {
        _fields = fields;
    }

    public AccountFields GetFields() => _fields;

    public void Update(CustomerUpdateableFields fields)
    {
        if (Owner == null) throw new DomainException("owner not setted");
        Owner.Update(fields);
    }

    public void Inactivate()
    {
        _fields.Active = false;
    }
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (!obj.GetType().Equals(this.GetType())) return false;
        Account accountToCompare = (Account)obj;
        return accountToCompare._fields.AccountNumber == _fields.AccountNumber;
    }

    public override int GetHashCode()
    {
        return _fields.AccountNumber;
    }
}