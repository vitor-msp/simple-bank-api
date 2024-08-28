using System;
using SimpleBankApi.Domain.Entities;
using Xunit;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Tests.Domain;

public class AccountTest
{
    private readonly int _accountId = 1;
    private readonly int _accountAccountNumber = 116565;
    private readonly string _accountPasswordHash = "hash";
    private readonly string _accountRefreshToken = "";
    private readonly DateTime _accountRefreshTokenExpiration = DateTime.Now;
    private readonly DateTime _accountCreatedAt = DateTime.Now;
    private readonly bool _accountActive = true;
    private readonly Role _role = Role.Customer;

    private Account GetAccountExample()
    {
        var owner = new Customer() { Cpf = "1", Name = "fulano" };
        return Account.Rebuild(_accountId, _accountAccountNumber, _accountCreatedAt, _accountActive,
            _role, owner, _accountPasswordHash, _accountRefreshToken, _accountRefreshTokenExpiration);
    }

    [Fact]
    public void RebuildAccount()
    {
        var id = 2536;
        var accountNumber = 296161351;
        var passwordHash = "hash";
        var refreshToken = "";
        var refreshTokenExpiration = DateTime.Now;
        var createdAt = DateTime.Now;
        var active = true;
        var role = Role.Admin;
        var owner = new Customer() { Cpf = "1", Name = "fulano" };

        var account = Account
            .Rebuild(id, accountNumber, createdAt, active, role, owner,
                passwordHash, refreshToken, refreshTokenExpiration);

        Assert.Equal(id, account.Id);
        Assert.Equal(accountNumber, account.AccountNumber);
        Assert.Equal(passwordHash, account.PasswordHash);
        Assert.Equal(createdAt, account.CreatedAt);
        Assert.Equal(active, account.Active);
        Assert.Equal(role, account.Role);
    }

    [Fact]
    public void InactivateAccount()
    {
        var account = GetAccountExample();

        account.Inactivate();

        Assert.Equal(_accountId, account.Id);
        Assert.Equal(_accountAccountNumber, account.AccountNumber);
        Assert.Equal(_accountCreatedAt, account.CreatedAt);
        Assert.False(account.Active);
        Assert.Equal(_role, account.Role);
    }

    [Fact]
    public void EqualsAccount()
    {
        var account = GetAccountExample();
        var owner = new Customer() { Cpf = "1", Name = "fulano" };
        var anotherAccount = Account.Rebuild(3642, 2668651, _accountCreatedAt, _accountActive, _role,
            owner, _accountPasswordHash, _accountRefreshToken, _accountRefreshTokenExpiration);

        bool nullResult = account.Equals(null);
        bool objResult = account?.Equals(new object() { }) ?? false;
        bool anotherAccountResult = account?.Equals(anotherAccount) ?? false;
        bool sameAccountResult = account?.Equals(GetAccountExample()) ?? false;

        Assert.False(nullResult);
        Assert.False(objResult);
        Assert.False(anotherAccountResult);
        Assert.True(sameAccountResult);
    }
}