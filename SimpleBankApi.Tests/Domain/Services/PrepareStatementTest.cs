using SimpleBankApi.Domain.Entities;
using Xunit;
using SimpleBankApi.Domain.Services;
using System.Collections.Generic;
using System;
using SimpleBankApi.Domain;
using System.Globalization;

namespace SimpleBankApi.Tests;

public class PrepareStatementTest
{
    private readonly IAccount _account = new Account(AccountFields.Rebuild(1, 1, DateTime.Now, true, "hash", "", DateTime.Now))
    {
        Owner = new Customer(CustomerFields.Rebuild(1, "0123", "fulano de tal"))
    };

    private readonly double _creditValue = 50;
    private readonly DateTime _creditCreatedAt = DateTime.Now;
    private readonly double _debitValue = 15;
    private readonly DateTime _debitCreatedAt = DateTime.Now;
    private readonly double _transferAsSenderValue = 12.5;
    private readonly DateTime _transferAsSenderCreatedAt = DateTime.Now;
    private readonly double _transferAsRecipientValue = 12.25;
    private readonly DateTime _transferAsRecipientCreatedAt = DateTime.Now;

    public PrepareStatementTest() { }

    private List<ICredit> GetCreditsExample()
    {
        var credits = new List<ICredit>();
        var credit = new Credit(CreditFields.Rebuild(1, _creditCreatedAt, _creditValue))
        {
            Account = _account
        };
        credits.Add(credit);
        return credits;
    }

    private List<IDebit> GetDebitsExample()
    {
        var debits = new List<IDebit>();
        var debit = new Debit(DebitFields.Rebuild(1, _debitCreatedAt, _debitValue))
        {
            Account = _account
        };
        debits.Add(debit);
        return debits;
    }

    private List<ITransfer> GetTransfersExample()
    {
        var transfers = new List<ITransfer>();
        var transferAsSender = new Transfer(TransferFields.Rebuild(1, _transferAsSenderCreatedAt, _transferAsSenderValue))
        {
            Sender = _account,
            Recipient = GetAccountExample()
        };
        var transferAsRecipient = new Transfer(TransferFields.Rebuild(1, _transferAsRecipientCreatedAt, _transferAsRecipientValue))
        {
            Sender = GetAccountExample(),
            Recipient = _account
        };
        transfers.Add(transferAsSender);
        transfers.Add(transferAsRecipient);
        return transfers;
    }

    private Account GetAccountExample() => new Account(AccountFields.Rebuild(2, 2, DateTime.Now, true, "hash", "", DateTime.Now))
    {
        Owner = new Customer(CustomerFields.Rebuild(2, "9876", "ciclano ferreira"))
    };

    [Fact]
    public void GetOrderedTransactions()
    {
        var statement = new PrepareStatement(GetCreditsExample(), GetDebitsExample(), GetTransfersExample(), _account)
            .SortTransactionsByDateTime();

        var receivedCredit = statement.Transactions[0];
        Assert.Equal(TransactionType.Credit, receivedCredit.Type);
        Assert.Equal(_creditCreatedAt, receivedCredit.CreditDto?.CreatedAt);
        Assert.Equal(_creditValue.ToString("c", CultureInfo.GetCultureInfo("pt-BR")), receivedCredit.CreditDto?.Value);

        var receivedDebit = statement.Transactions[1];
        Assert.Equal(TransactionType.Debit, receivedDebit.Type);
        Assert.Equal(_debitCreatedAt, receivedDebit.DebitDto?.CreatedAt);
        Assert.Equal((-1 * _debitValue).ToString("c", CultureInfo.GetCultureInfo("pt-BR")), receivedDebit.DebitDto?.Value);

        var receivedTransferAsSender = statement.Transactions[2];
        Assert.Equal(TransactionType.Transfer, receivedTransferAsSender.Type);
        Assert.Equal((-1 * _transferAsSenderValue).ToString("c", CultureInfo.GetCultureInfo("pt-BR")), receivedTransferAsSender.TransferDto?.Value);
        Assert.Equal(_transferAsSenderCreatedAt, receivedTransferAsSender.TransferDto?.CreatedAt);
        Assert.Equal(_account.GetFields().AccountNumber, receivedTransferAsSender.TransferDto?.Sender?.AccountNumber);
        Assert.Equal(_account.Owner!.GetFields().Name, receivedTransferAsSender.TransferDto?.Sender?.Name);
        Assert.Equal(GetAccountExample().GetFields().AccountNumber, receivedTransferAsSender.TransferDto?.Recipient?.AccountNumber);
        Assert.Equal(GetAccountExample().Owner!.GetFields().Name, receivedTransferAsSender.TransferDto?.Recipient?.Name);

        var receivedTransferAsRecipient = statement.Transactions[3];
        Assert.Equal(TransactionType.Transfer, receivedTransferAsRecipient.Type);
        Assert.Equal(_transferAsRecipientValue.ToString("c", CultureInfo.GetCultureInfo("pt-BR")), receivedTransferAsRecipient.TransferDto?.Value);
        Assert.Equal(_transferAsRecipientCreatedAt, receivedTransferAsRecipient.TransferDto?.CreatedAt);
        Assert.Equal(GetAccountExample().GetFields().AccountNumber, receivedTransferAsRecipient.TransferDto?.Sender?.AccountNumber);
        Assert.Equal(GetAccountExample().Owner!.GetFields().Name, receivedTransferAsRecipient.TransferDto?.Sender?.Name);
        Assert.Equal(_account.GetFields().AccountNumber, receivedTransferAsRecipient.TransferDto?.Recipient?.AccountNumber);
        Assert.Equal(_account.Owner!.GetFields().Name, receivedTransferAsRecipient.TransferDto?.Recipient?.Name);
    }
}