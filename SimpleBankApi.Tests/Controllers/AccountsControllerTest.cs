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
        var account = new Account(AccountFields.Rebuild(1, 1, DateTime.Now, true));
        var customer = new Customer(new CustomerFields()
        {
            Cpf = cpf,
            Name = "fulano",
        });
        account.Owner = customer;
        return account;
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
        Assert.Equal(savedAccounts[0].GetFields().AccountNumber, getAllOutput.Accounts[0].AccountNumber);
        Assert.Equal(savedAccounts[0].Owner?.GetFields().Name, getAllOutput.Accounts[0].Name);
        Assert.Equal(savedAccounts[1].GetFields().AccountNumber, getAllOutput.Accounts[1].AccountNumber);
        Assert.Equal(savedAccounts[1].Owner?.GetFields().Name, getAllOutput.Accounts[1].Name);
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

        var actionResult = getType == "cpf"
            ? await sut.GetByCpf(account.Owner?.GetFields().Cpf ?? "")
            : await sut.GetById(account.GetFields().AccountNumber);

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
        var savedAccount = context.Accounts.Single(account => account.GetFields().AccountNumber == accountNumber);
        Assert.Equal(accountNumber, savedAccount.GetFields().AccountNumber);
        Assert.True(savedAccount.GetFields().Active);
        Assert.IsType<DateTime>(savedAccount.GetFields().CreatedAt);
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
            Cpf = account.Owner?.GetFields().Cpf ?? "",
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

        var actionResult = await sut.Put(account.GetFields().AccountNumber, input);

        Assert.IsType<NoContentResult>(actionResult);
        var savedAccount = context.Accounts.Single(account => account.GetFields().Id == account.GetFields().Id);
        Assert.Equal(account.GetFields().AccountNumber, savedAccount.GetFields().AccountNumber);
        Assert.Equal(account.GetFields().Active, savedAccount.GetFields().Active);
        Assert.Equal(account.GetFields().CreatedAt, savedAccount.GetFields().CreatedAt);
        Assert.Equal(account.Owner?.GetFields().Id, savedAccount.Owner?.GetFields().Id);
        Assert.Equal(account.Owner?.GetFields().Cpf, savedAccount.Owner?.GetFields().Cpf);
        Assert.Equal(input.Name, savedAccount.Owner?.GetFields().Name);
    }

    [Theory]
    [InlineData("put")]
    [InlineData("delete")]
    public async Task Put_And_Delete_ReturnNotFound(string type)
    {
        var (sut, context) = MakeSut();
        var input = new AccountUpdateDto() { Name = "ciclano" };

        var actionResult = type == "put" ? await sut.Put(1, input) : await sut.Delete(1);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task Delete()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        context.SaveChanges();

        var actionResult = await sut.Delete(account.GetFields().AccountNumber);

        Assert.IsType<NoContentResult>(actionResult);
        var savedAccount = context.Accounts.Single(account => account.GetFields().Id == account.GetFields().Id);
        Assert.Equal(account.GetFields().AccountNumber, savedAccount.GetFields().AccountNumber);
        Assert.False(savedAccount.GetFields().Active);
        Assert.Equal(account.GetFields().CreatedAt, savedAccount.GetFields().CreatedAt);
        Assert.Equal(account.Owner, savedAccount.Owner);
    }
}