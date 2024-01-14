namespace Models;

public class Account
{
    private readonly AccountFields _fields;

    public Customer? Owner { get; set; }

    public Account(AccountFields fields)
    {
        _fields = fields;
    }

    public AccountFields GetFields() => _fields;

    public void Update(CustomerUpdateableFields fields)
    {
        if (Owner == null) throw new Exception();
        Owner.Update(fields);
    }

    public void Inactivate()
    {
        _fields.Active = false;
    }

    /// to check
    public (int, string) GetPublicData()
    {
        return (_fields.AccountNumber, Owner?.GetFields().Name ?? "");
    }

    /// to check
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (!obj.GetType().Equals(this.GetType())) return false;
        Account accountToCompare = (Account)obj;
        return accountToCompare._fields.AccountNumber == _fields.AccountNumber;
    }

    /// to check
    public override int GetHashCode()
    {
        return _fields.AccountNumber;
    }
}