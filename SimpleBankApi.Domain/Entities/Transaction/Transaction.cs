using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Domain.Entities;

public abstract class Transaction : ITransaction
{
    protected readonly TransactionFields _fields;

    protected Transaction(TransactionFields fields)
    {
        ValidateFields(fields);
        _fields = fields;
    }

    public TransactionFields GetFields() => _fields;

    private static void ValidateFields(TransactionFields fields)
    {
        if (fields.Value <= 0)
            throw new DomainException("The transaction value must be greater than zero.");
    }
}