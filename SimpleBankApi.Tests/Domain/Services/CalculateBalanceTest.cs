using SimpleBankApi.Domain.Entities;
using Xunit;
using Moq;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.Contract;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SimpleBankApi.Tests;

public class CalculateBalanceTest
{
    private readonly int _accountNumber = 1;
    private readonly IAccount _account;
    private readonly IBankCache _bankCacheMock = new Mock<IBankCache>().Object;

    public CalculateBalanceTest()
    {
        _account = new Account(AccountFields.Rebuild(1, _accountNumber, DateTime.Now, true));
    }

    private List<ICredit> GetCreditsExample()
    {
        var credits = new List<ICredit>();
        var credit = new Credit(new CreditFields() { Value = 50 })
        {
            Account = _account
        };
        credits.Add(credit);
        return credits;
    }

    private List<IDebit> GetDebitsExample()
    {
        var debits = new List<IDebit>();
        var debit = new Debit(new DebitFields() { Value = 15 })
        {
            Account = _account
        };
        debits.Add(debit);
        return debits;
    }

    private List<ITransfer> GetTransfersExample()
    {
        var transfers = new List<ITransfer>();
        var transferAsSender = new Transfer(new TransferFields() { Value = 12.5 })
        {
            Sender = _account,
            Recipient = GetAccountExample()
        };
        var transferAsRecipient = new Transfer(new TransferFields() { Value = 12.25 })
        {
            Sender = GetAccountExample(),
            Recipient = _account
        };
        transfers.Add(transferAsSender);
        transfers.Add(transferAsRecipient);
        return transfers;
    }

    private Account GetAccountExample() => new Account(AccountFields.Rebuild(2, 2, DateTime.Now, true));

    [Fact]
    public async Task NoBalance()
    {
        var transactionsRepositoryMock = new Mock<ITransactionsRepository>();
        transactionsRepositoryMock.Setup(mock => mock.GetCreditsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<ICredit>()));
        transactionsRepositoryMock.Setup(mock => mock.GetDebitsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<IDebit>()));
        transactionsRepositoryMock.Setup(mock => mock.GetTransfersFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<ITransfer>()));

        var calculateBalance =
            new CalculateBalance(transactionsRepositoryMock.Object, _bankCacheMock);
        double balance = await calculateBalance.FromAccount(_account);

        Assert.Equal(0, balance);
        transactionsRepositoryMock.Verify(mock => mock.GetCreditsFromAccount(_accountNumber), Times.Once);
        transactionsRepositoryMock.Verify(mock => mock.GetDebitsFromAccount(_accountNumber), Times.Once);
        transactionsRepositoryMock.Verify(mock => mock.GetTransfersFromAccount(_accountNumber), Times.Once);
    }

    [Fact]
    public async Task WithBalance()
    {
        var transactionsRepositoryMock = new Mock<ITransactionsRepository>();
        transactionsRepositoryMock.Setup(mock => mock.GetCreditsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetCreditsExample()));
        transactionsRepositoryMock.Setup(mock => mock.GetDebitsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetDebitsExample()));
        transactionsRepositoryMock.Setup(mock => mock.GetTransfersFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetTransfersExample()));

        var calculateBalance =
            new CalculateBalance(transactionsRepositoryMock.Object, _bankCacheMock);
        double balance = await calculateBalance.FromAccount(_account);

        Assert.Equal(34.75, balance);
        transactionsRepositoryMock.Verify(mock => mock.GetCreditsFromAccount(_accountNumber), Times.Once);
        transactionsRepositoryMock.Verify(mock => mock.GetDebitsFromAccount(_accountNumber), Times.Once);
        transactionsRepositoryMock.Verify(mock => mock.GetTransfersFromAccount(_accountNumber), Times.Once);
    }
}