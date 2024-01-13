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

public class AccountsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;

    public AccountsControllerTest()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<BankContext>().UseSqlite(_connection).Options;
        var context = new BankContext(_contextOptions);
        if (context.Database.EnsureCreated())
        {
            // add data
        }
        context.SaveChanges();
    }

    private BankContext CreateContext() => new BankContext(_contextOptions);

    private (AccountsController, BankContext) MakeSut()
    {
        var context = CreateContext();
        var controller = new AccountsController(context);
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task Post()
    {
        var (sut, context) = MakeSut();
        var input = new AccountCreateDto()
        {
            Name = "fulano de tal",
            Cpf = "01234567890",
        };

        var actionResult = await sut.Post(input);

        var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
        Assert.Equal("GetAccount", createdAtRouteResult.RouteName);
        var postOutput = Assert.IsType<AccountsController.PostOutput>(createdAtRouteResult.Value);
        var accountNumber = Assert.IsType<int>(postOutput.AccountNumber);
        var savedAccount = context.Accounts.Single(account => account.AccountNumber == accountNumber);
        Assert.Equal(accountNumber, savedAccount.AccountNumber);
        Assert.True(savedAccount.Active);
        Assert.IsType<DateTime>(savedAccount.CreatedAt);
    }

    [Fact]
    public async Task Put()
    {
        var (sut, context) = MakeSut();
        var account = new Account()
        {
            Id = 1,
            AccountNumber = 1,
            Active = true,
            CreatedAt = DateTime.Now,
            Owner = new Customer() { Id = 1, Cpf = "0123", Name = "fulano" }
        };
        context.Accounts.Add(account);
        context.SaveChanges();
        var input = new AccountUpdateDto() { Name = "ciclano" };

        var actionResult = await sut.Put(account.AccountNumber, input);

        Assert.IsType<NoContentResult>(actionResult);
        var savedAccount = context.Accounts.Single(account => account.Id == account.Id);
        Assert.Equal(account.AccountNumber, savedAccount.AccountNumber);
        Assert.Equal(account.Active, savedAccount.Active);
        Assert.Equal(account.CreatedAt, savedAccount.CreatedAt);
        Assert.Equal(account.Owner.Id, savedAccount.Owner.Id);
        Assert.Equal(account.Owner.Cpf, savedAccount.Owner.Cpf);
        Assert.Equal(input.Name, savedAccount.Owner.Name);
    }

}