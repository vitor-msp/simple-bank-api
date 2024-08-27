using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Api.Controllers;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.ValueObjects;
using SimpleBankApi.Infra;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Database.Schema;
using SimpleBankApi.Repository.Implementations;
using Xunit;

namespace SimpleBankApi.Tests;

public class AdminAccountsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();

    public AdminAccountsControllerTest()
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

    private (AdminAccountsController, BankContext) MakeSut()
    {
        var context = CreateContext();
        var accountsRepository = new AccountsRepository(context);
        var passwordHasher = new PasswordHasher();
        var controller = new AdminAccountsController(
            new CreateAdminAccountUseCase(new CreateAccount(accountsRepository, passwordHasher)));
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private AccountDB AccountExample(string cpf = "0123")
        => new()
        {
            AccountNumber = 1,
            CreatedAt = DateTime.Now,
            Active = true,
            Role = Role.Admin.ToString(),
            PasswordHash = _passwordHasher.Hash("pass123"),
            Owner = new CustomerDB()
            {
                Cpf = cpf,
                Name = "fulano",
            }
        };

    [Fact]
    public async Task Post_ReturnCreated()
    {
        var (sut, context) = MakeSut();
        var input = new CreateAccountInput()
        {
            Name = "fulano de tal",
            Cpf = "01234567890",
            Password = "pass123",
            PasswordConfirmation = "pass123",
        };

        var actionResult = await sut.Post(input);

        var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
        Assert.Equal("GetAccount", createdAtRouteResult.RouteName);
        var postOutput = Assert.IsType<CreateAccountOutput>(createdAtRouteResult.Value);
        var accountNumber = Assert.IsType<int>(postOutput.AccountNumber);
        var savedAccount = context.Accounts.Single(account => account.AccountNumber == accountNumber);
        Assert.Equal(accountNumber, savedAccount.AccountNumber);
        Assert.True(savedAccount.Active);
        Assert.IsType<DateTime>(savedAccount.CreatedAt);
        Assert.IsType<string>(savedAccount.PasswordHash);
        Assert.True(_passwordHasher.Verify(savedAccount.PasswordHash!, "pass123"));
        Assert.Equal(Role.Admin.ToString(), savedAccount.Role);
    }

    [Fact]
    public async Task Post_ReturnBadRequest()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        var input = new CreateAccountInput()
        {
            Name = "fulano de tal",
            Cpf = account.Owner?.Cpf ?? "",
            Password = "pass123",
            PasswordConfirmation = "pass123",
        };

        var actionResult = await sut.Post(input);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Post_ReturnBadRequest_PasswordAndConfirmationNotEqual()
    {
        var (sut, _) = MakeSut();
        var input = new CreateAccountInput()
        {
            Name = "fulano de tal",
            Cpf = "01234567890",
            Password = "pass123",
            PasswordConfirmation = "pass1234",
        };

        var actionResult = await sut.Post(input);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}