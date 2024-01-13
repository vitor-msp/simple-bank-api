using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Context;
using Controllers;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using Xunit;

namespace SimpleBankApi.Tests;

public class TransactionsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly Account _account;

    public TransactionsControllerTest()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<BankContext>().UseSqlite(_connection).Options;
        _account = AccountExample();
    }

    private BankContext CreateContext()
    {
        var context = new BankContext(_contextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    private (TransactionsController, BankContext) MakeSut()
    {
        var context = CreateContext();
        var controller = new TransactionsController(context);
        context.Accounts.Add(_account);
        context.SaveChanges();
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private Account AccountExample(int accountNumber = 1, string cpf = "123")
    {
        return new Account()
        {
            AccountNumber = accountNumber,
            Active = true,
            CreatedAt = DateTime.Now,
            Owner = new Customer() { Cpf = cpf, Name = "fulano" }
        };
    }

    private Credit CreditExample()
    {
        return new Credit() { Value = 100, Account = _account, CreatedAt = DateTime.Now };
    }

    [Fact]
    public async Task PostCredit()
    {
        var (sut, context) = MakeSut();
        var input = new CreditDto() { Value = 100.56 };

        var actionResult = await sut.PostCredit(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedCredits = context.Credits.Where(credit => credit.Account.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedCredits);
        Assert.Equal(input.Value, savedCredits[0].Value);
        Assert.Equal(_account, savedCredits[0].Account);
        Assert.IsType<int>(savedCredits[0].Id);
        Assert.IsType<DateTime>(savedCredits[0].CreatedAt);
    }

    [Fact]
    public async Task PostDebit()
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        context.SaveChanges();
        var input = new DebitDto() { Value = 50.56 };

        var actionResult = await sut.PostDebit(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedDebits = context.Debits.Where(debit => debit.Account.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedDebits);
        Assert.Equal(input.Value, -1 * savedDebits[0].Value);
        Assert.Equal(_account, savedDebits[0].Account);
        Assert.IsType<int>(savedDebits[0].Id);
        Assert.IsType<DateTime>(savedDebits[0].CreatedAt);
    }

    [Fact]
    public async Task PostTransfer()
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        context.SaveChanges();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedTransfers = context.Transfers.Where(transfer => transfer.Sender.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedTransfers);
        Assert.Equal(input.Value, savedTransfers[0].Value);
        Assert.Equal(_account, savedTransfers[0].Sender);
        Assert.Equal(recipientAccount, savedTransfers[0].Recipient);
        Assert.IsType<int>(savedTransfers[0].Id);
        Assert.IsType<DateTime>(savedTransfers[0].CreatedAt);
    }
}