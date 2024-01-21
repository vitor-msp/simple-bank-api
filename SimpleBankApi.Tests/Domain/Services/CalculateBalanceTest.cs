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
    private readonly IAccount _account = new Account(AccountFields.Rebuild(1, 1, DateTime.Now, true));

    public CalculateBalanceTest() { }

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
        var transactionsRepository = new Mock<ITransactionsRepository>();
        transactionsRepository.Setup(mock => mock.GetCreditsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<ICredit>()));
        transactionsRepository.Setup(mock => mock.GetDebitsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<IDebit>()));
        transactionsRepository.Setup(mock => mock.GetTransfersFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(new List<ITransfer>()));

        var calculateBalance = new CalculateBalance(transactionsRepository.Object);
        double balance = await calculateBalance.FromAccount(_account);

        Assert.Equal(0, balance);
    }

    [Fact]
    public async Task WithBalance()
    {
        var transactionsRepository = new Mock<ITransactionsRepository>();
        transactionsRepository.Setup(mock => mock.GetCreditsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetCreditsExample()));
        transactionsRepository.Setup(mock => mock.GetDebitsFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetDebitsExample()));
        transactionsRepository.Setup(mock => mock.GetTransfersFromAccount(It.IsAny<int>()))
            .Returns(Task.FromResult(GetTransfersExample()));

        var calculateBalance = new CalculateBalance(transactionsRepository.Object);
        double balance = await calculateBalance.FromAccount(_account);

        Assert.Equal(34.75, balance);
    }
}