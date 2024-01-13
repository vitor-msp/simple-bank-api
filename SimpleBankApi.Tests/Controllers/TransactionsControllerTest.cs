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

    private Account AccountExample()
    {
        return new Account()
        {
            AccountNumber = 1,
            Active = true,
            CreatedAt = DateTime.Now,
            Owner = new Customer() { Cpf = "0123", Name = "fulano" }
        };
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
}