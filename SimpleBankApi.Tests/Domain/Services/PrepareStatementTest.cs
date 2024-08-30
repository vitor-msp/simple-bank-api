using SimpleBankApi.Domain.Entities;
using Xunit;
using SimpleBankApi.Domain.Services;
using System.Collections.Generic;
using System;
using SimpleBankApi.Domain;
using System.Globalization;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Tests.Domain;

public class PrepareStatementTest
{
    private readonly IAccount _account = Account.Rebuild(1, 1, DateTime.Now, true, Role.Customer,
        Customer.Rebuild(1, "0123", "fulano de tal"), "hash", "", DateTime.Now);
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
        var credit = Credit.Rebuild(1, _creditCreatedAt, _creditValue, _account);
        credits.Add(credit);
        return credits;
    }

    private List<IDebit> GetDebitsExample()
    {
        var debits = new List<IDebit>();
        var debit = Debit.Rebuild(1, _debitCreatedAt, _debitValue, _account);
        debits.Add(debit);
        return debits;
    }

    private List<ITransfer> GetTransfersExample()
    {
        var transfers = new List<ITransfer>();
        var transferAsSender = Transfer.Rebuild(1, _transferAsSenderCreatedAt, _transferAsSenderValue,
            sender: _account, recipient: GetAccountExample());

        var transferAsRecipient = Transfer.Rebuild(1, _transferAsRecipientCreatedAt, _transferAsRecipientValue,
            sender: GetAccountExample(), recipient: _account);

        transfers.Add(transferAsSender);
        transfers.Add(transferAsRecipient);
        return transfers;
    }

    private static Account GetAccountExample() => Account.Rebuild(2, 2, DateTime.Now, true, Role.Customer,
        Customer.Rebuild(2, "9876", "ciclano ferreira"), "hash", "", DateTime.Now);

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
        Assert.Equal(_account.AccountNumber, receivedTransferAsSender.TransferDto?.Sender?.AccountNumber);
        Assert.Equal(_account.Owner.Name, receivedTransferAsSender.TransferDto?.Sender?.Name);
        Assert.Equal(GetAccountExample().AccountNumber, receivedTransferAsSender.TransferDto?.Recipient?.AccountNumber);
        Assert.Equal(GetAccountExample().Owner.Name, receivedTransferAsSender.TransferDto?.Recipient?.Name);

        var receivedTransferAsRecipient = statement.Transactions[3];
        Assert.Equal(TransactionType.Transfer, receivedTransferAsRecipient.Type);
        Assert.Equal(_transferAsRecipientValue.ToString("c", CultureInfo.GetCultureInfo("pt-BR")), receivedTransferAsRecipient.TransferDto?.Value);
        Assert.Equal(_transferAsRecipientCreatedAt, receivedTransferAsRecipient.TransferDto?.CreatedAt);
        Assert.Equal(GetAccountExample().AccountNumber, receivedTransferAsRecipient.TransferDto?.Sender?.AccountNumber);
        Assert.Equal(GetAccountExample().Owner.Name, receivedTransferAsRecipient.TransferDto?.Sender?.Name);
        Assert.Equal(_account.AccountNumber, receivedTransferAsRecipient.TransferDto?.Recipient?.AccountNumber);
        Assert.Equal(_account.Owner.Name, receivedTransferAsRecipient.TransferDto?.Recipient?.Name);
    }
}