using System;
using SimpleBankApi.Domain.Entities;
using Xunit;
using Moq;

namespace SimpleBankApi.Tests;

public class AccountTest
{
    private readonly int _accountId = 1;
    private readonly int _accountAccountNumber = 116565;
    private readonly DateTime _accountCreatedAt = DateTime.Now;
    private readonly bool _accountActive = true;

    public AccountTest() { }

    private Account GetAccountExample()
    {
        return new Account(
            AccountFields.Rebuild(_accountId, _accountAccountNumber, _accountCreatedAt, _accountActive));
    }

    [Fact]
    public void RebuildAccount()
    {
        int id = 2536;
        int accountNumber = 296161351;
        DateTime createdAt = DateTime.Now;
        bool active = true;

        var account = new Account(AccountFields.Rebuild(id, accountNumber, createdAt, active));

        Assert.Equal(id, account.GetFields().Id);
        Assert.Equal(accountNumber, account.GetFields().AccountNumber);
        Assert.Equal(createdAt, account.GetFields().CreatedAt);
        Assert.Equal(active, account.GetFields().Active);
    }

    [Fact]
    public void UpdateAccount()
    {
        var account = GetAccountExample();
        var ownerMock = new Mock<ICustomer>();
        account.Owner = ownerMock.Object;
        string newName = "joao da silva";
        var input = new CustomerUpdateableFields() { Name = newName };

        account.Update(input);

        ownerMock.Verify(mock => mock.Update(input), Times.Once);
    }

    [Fact]
    public void InactivateAccount()
    {
        var account = GetAccountExample();

        account.Inactivate();

        Assert.Equal(_accountId, account.GetFields().Id);
        Assert.Equal(_accountAccountNumber, account.GetFields().AccountNumber);
        Assert.Equal(_accountCreatedAt, account.GetFields().CreatedAt);
        Assert.False(account.GetFields().Active);
    }

    [Fact]
    public void EqualsAccount()
    {
        var account = GetAccountExample();
        var anotherAccount = new Account(
           AccountFields.Rebuild(3642, 2668651, _accountCreatedAt, _accountActive));

        var nullResult = account.Equals(null);
        var objResult = account.Equals(new object() { });
        var anotherAccountResult = account.Equals(anotherAccount);
        var sameAccountResult = account.Equals(GetAccountExample());

        Assert.False(nullResult);
        Assert.False(objResult);
        Assert.False(anotherAccountResult);
        Assert.True(sameAccountResult);
    }
}