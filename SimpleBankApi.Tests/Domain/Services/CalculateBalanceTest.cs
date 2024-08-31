using SimpleBankApi.Domain.Entities;
using Xunit;
using Moq;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.Contract;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using SimpleBankApi.Domain.ValueObjects;
using System.Linq;

namespace SimpleBankApi.Tests.Domain;

public class CalculateBalanceTest
{
    private readonly int _accountNumber = 1;
    private readonly IAccount _account;

    public CalculateBalanceTest()
    {
        var owner = new Customer() { Cpf = "1", Name = "fulano" };
        _account = Account.Rebuild(1, _accountNumber, DateTime.Now, true, Role.Customer, owner, "hash", "", DateTime.Now);
    }

    private List<TransactionWrapper> GetTransactionsExample()
    {
        var credits = GetCreditsExample().Select(credit => new TransactionWrapper()
        {
            Credit = credit,
            TransactionType = TransactionType.Credit
        });
        var debits = GetDebitsExample().Select(debit => new TransactionWrapper()
        {
            Debit = debit,
            TransactionType = TransactionType.Debit
        });
        var transfers = GetTransfersExample().Select(transfer => new TransactionWrapper()
        {
            Transfer = transfer,
            TransactionType = TransactionType.Transfer
        });
        var transactions = new List<TransactionWrapper>();
        transactions.AddRange(credits);
        transactions.AddRange(debits);
        transactions.AddRange(transfers);
        return transactions;
    }

    private List<ICredit> GetCreditsExample()
        => [
            new Credit()
            {
                Value = 50,
                Account = _account,
            }
        ];

    private List<IDebit> GetDebitsExample()
        => [
            new Debit()
            {
                Value = -15,
                Account = _account,
            }
        ];

    private List<ITransfer> GetTransfersExample()
       => [
            new Transfer()
            {
                Value = 12.5,
                Sender = _account,
                Recipient = GetAccountExample(),
            },
            new Transfer()
            {
                Value = 12.25,
                Sender = GetAccountExample(),
                Recipient = _account,
            },
       ];

    private static Account GetAccountExample()
    {
        var owner = new Customer() { Cpf = "1", Name = "fulano" };
        return Account.Rebuild(2, 2, DateTime.Now, true, Role.Customer, owner, "hash", "", DateTime.Now);
    }

    [Fact]
    public async Task NoBalance()
    {
        var transactionsRepositoryMock = new Mock<ITransactionsRepository>();
        transactionsRepositoryMock.Setup(mock => mock.GetTransactionsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<TransactionWrapper>()));

        var calculateBalance = new CalculateBalance(transactionsRepositoryMock.Object);
        double balance = await calculateBalance.FromAccount(_account);

        Assert.Equal(0, balance);
        transactionsRepositoryMock.Verify(mock => mock.GetTransactionsFromAccount(_accountNumber), Times.Once);
    }

    [Fact]
    public async Task WithBalance()
    {
        var transactionsRepositoryMock = new Mock<ITransactionsRepository>();
        transactionsRepositoryMock.Setup(mock => mock.GetTransactionsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetTransactionsExample()));

        var calculateBalance = new CalculateBalance(transactionsRepositoryMock.Object);
        double balance = await calculateBalance.FromAccount(_account);

        Assert.Equal(34.75, balance);
        transactionsRepositoryMock.Verify(mock => mock.GetTransactionsFromAccount(_accountNumber), Times.Once);
    }
}