using System;
using SimpleBankApi.Domain.Entities;
using Xunit;
using SimpleBankApi.Domain.Exceptions;

namespace SimpleBankApi.Tests;

public class CreditTest
{
    public CreditTest() { }

    [Fact]
    public void NotBuildCredit()
    {
        var value = -10;

        var error = Assert.Throws<DomainException>(() => new Credit(new CreditFields() { Value = value }));

        Assert.Equal("The transaction value must be greater than zero.", error.Message);
    }

    [Fact]
    public void RebuildCredit()
    {
        int id = 12;
        DateTime createdAt = DateTime.Now;
        double value = 164.63;

        var credit = new Credit(CreditFields.Rebuild(id, createdAt, value));

        Assert.Equal(id, credit.GetFields().Id);
        Assert.Equal(createdAt, credit.GetFields().CreatedAt);
        Assert.Equal(value, credit.GetFields().Value);
    }
}