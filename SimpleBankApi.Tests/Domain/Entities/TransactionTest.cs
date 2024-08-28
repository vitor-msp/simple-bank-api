using System;
using SimpleBankApi.Domain.Entities;
using Xunit;
using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Tests.Domain;

public class TransactionTest
{
    public TransactionTest() { }

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    [InlineData("transfer")]
    public void NotBuildTransaction(string type)
    {
        var value = -10;

        var error = type switch
        {
            "credit" => Assert.Throws<DomainException>(() => new Credit(new CreditFields() { Value = value })),
            "debit" => Assert.Throws<DomainException>(() => new Debit(new DebitFields() { Value = value })),
            "transfer" => Assert.Throws<DomainException>(() => new Transfer(new TransferFields() { Value = value })),
            _ => new Exception(""),
        };

        Assert.Equal("The transaction value must be greater than zero.", error.Message);
    }

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    [InlineData("transfer")]
    public void RebuildTransaction(string type)
    {
        int id = 12;
        DateTime createdAt = DateTime.Now;
        double value = 164.63;

        Transaction? transaction = type switch
        {
            "credit" => new Credit(CreditFields.Rebuild(id, createdAt, value)),
            "debit" => new Debit(DebitFields.Rebuild(id, createdAt, value)),
            "transfer" => new Transfer(TransferFields.Rebuild(id, createdAt, value)),
            _ => null,
        };

        Assert.NotNull(transaction);
        Assert.Equal(id, transaction!.GetFields().Id);
        Assert.Equal(createdAt, transaction.GetFields().CreatedAt);
        Assert.Equal(value, transaction.GetFields().Value);
    }
}