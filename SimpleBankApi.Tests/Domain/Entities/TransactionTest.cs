using System;
using SimpleBankApi.Domain.Entities;
using Xunit;
using SimpleBankApi.Domain.Exceptions;
using Moq;

namespace SimpleBankApi.Tests.Domain;

public class TransactionTest
{
    private readonly int _id = 12;
    private readonly DateTime _createdAt = DateTime.Now;
    private readonly double _value = 164.63;
    private readonly IAccount _account = new Mock<IAccount>().Object;
    private readonly IAccount _otherAccount = new Mock<IAccount>().Object;

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    [InlineData("transfer")]
    public void NotBuildTransaction(string type)
    {
        var value = -10;

        var error = type switch
        {
            "credit" => Assert.Throws<DomainException>(() => new Credit() { Value = value, Account = _account }),
            "debit" => Assert.Throws<DomainException>(() => new Debit() { Value = value, Account = _account }),
            "transfer" => Assert.Throws<DomainException>(() => new Transfer() { Value = value, Sender = _account, Recipient = _otherAccount }),
            _ => new Exception(""),
        };

        Assert.Equal("The transaction value must be greater than zero.", error.Message);
    }

    [Fact]
    public void RebuildCredit()
    {
        var credit = Credit.Rebuild(_id, _createdAt, _value, _account);

        Assert.NotNull(credit);
        Assert.Equal(_id, credit.Id);
        Assert.Equal(_createdAt, credit.CreatedAt);
        Assert.Equal(_value, credit.Value);
        Assert.Equal(_account, credit.Account);
    }

    [Fact]
    public void RebuildDebit()
    {
        var debit = Debit.Rebuild(_id, _createdAt, _value, _account);

        Assert.NotNull(debit);
        Assert.Equal(_id, debit.Id);
        Assert.Equal(_createdAt, debit.CreatedAt);
        Assert.Equal(_value, debit.Value);
        Assert.Equal(_account, debit.Account);
    }

    [Fact]
    public void RebuildTransfer()
    {
        var transfer = Transfer.Rebuild(_id, _createdAt, _value, _account, _otherAccount);

        Assert.NotNull(transfer);
        Assert.Equal(_id, transfer.Id);
        Assert.Equal(_createdAt, transfer.CreatedAt);
        Assert.Equal(_value, transfer.Value);
        Assert.Equal(_account, transfer.Sender);
        Assert.Equal(_otherAccount, transfer.Recipient);
    }
}