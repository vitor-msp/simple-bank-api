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
    }

    private BankContext CreateContext()
    {
        var context = new BankContext(_contextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    private (AccountsController, BankContext) MakeSut()
    {
        var context = CreateContext();
        var controller = new AccountsController(context);
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private Account AccountExample(string cpf = "0123")
    {
        return new Account()
        {
            AccountNumber = 1,
            Active = true,
            CreatedAt = DateTime.Now,
            Owner = new Customer() { Cpf = cpf, Name = "fulano" }
        };
    }

    [Fact]
    public async Task GetAll_ReturnNotEmpty()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample("123"));
        context.Accounts.Add(AccountExample("321"));
        context.SaveChanges();

        var actionResult = await sut.GetAll();

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getAllOutput = Assert.IsType<AccountsController.GetAllOutput>(okObjectResult.Value);
        var savedAccounts = context.Accounts.ToList();
        Assert.Equal(savedAccounts.Count, getAllOutput.Accounts.Count);
        Assert.Equal(2, getAllOutput.Accounts.Count);
        Assert.Equal(savedAccounts[0].AccountNumber, getAllOutput.Accounts[0].AccountNumber);
        Assert.Equal(savedAccounts[0].Owner.Name, getAllOutput.Accounts[0].Name);
        Assert.Equal(savedAccounts[1].AccountNumber, getAllOutput.Accounts[1].AccountNumber);
        Assert.Equal(savedAccounts[1].Owner.Name, getAllOutput.Accounts[1].Name);
    }

    [Fact]
    public async Task GetAll_ReturnEmpty()
    {
        var (sut, context) = MakeSut();

        var actionResult = await sut.GetAll();

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getAllOutput = Assert.IsType<AccountsController.GetAllOutput>(okObjectResult.Value);
        var savedAccounts = context.Accounts.ToList();
        Assert.Equal(savedAccounts.Count, getAllOutput.Accounts.Count);
        Assert.Empty(getAllOutput.Accounts);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("cpf")]
    public async Task GetById_And_GetByCpf_ReturnEntity(string getType)
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        context.SaveChanges();

        var actionResult = getType == "cpf" ? await sut.GetByCpf(account.Owner.Cpf) : await sut.GetById(account.AccountNumber);

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var accountResult = Assert.IsType<Account>(okObjectResult.Value);
        Assert.Equal(account, accountResult);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("cpf")]
    public async Task GetById_And_GetByCpf_ReturnNotFound(string getType)
    {
        var (sut, context) = MakeSut();

        var actionResult = getType == "cpf" ? await sut.GetByCpf("123") : await sut.GetById(1);

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Post_ReturnCreated()
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
    public async Task Post_ReturnBadRequest()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        context.SaveChanges();
        var input = new AccountCreateDto()
        {
            Name = "fulano de tal",
            Cpf = account.Owner.Cpf,
        };

        var actionResult = await sut.Post(input);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Put_ReturnNoContent()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
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

    [Fact]
    public async Task Put_ReturnNotFound()
    {
        var (sut, context) = MakeSut();
        var input = new AccountUpdateDto() { Name = "ciclano" };

        var actionResult = await sut.Put(1, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task Delete()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        context.SaveChanges();

        var actionResult = await sut.Delete(account.AccountNumber);

        Assert.IsType<NoContentResult>(actionResult);
        var savedAccount = context.Accounts.Single(account => account.Id == account.Id);
        Assert.Equal(account.AccountNumber, savedAccount.AccountNumber);
        Assert.False(savedAccount.Active);
        Assert.Equal(account.CreatedAt, savedAccount.CreatedAt);
        Assert.Equal(account.Owner, savedAccount.Owner);
    }
}