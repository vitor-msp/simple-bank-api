using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleBankApi.Api.Controllers;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.ValueObjects;
using SimpleBankApi.Infra.Implementations;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Database.Schema;
using SimpleBankApi.Repository.Implementations;
using Xunit;

namespace SimpleBankApi.Tests;

public class AccountsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();

    public AccountsControllerTest()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<BankContext>().UseSqlite(_connection).Options;
    }

    private BankContext CreateContext()
    {
        var context = new BankContext(_contextOptions, useNpgsql: false);
        context.Database.EnsureCreated();
        return context;
    }

    private (AccountsController, BankContext) MakeSut(int? accountNumberToAuthenticate = null)
    {
        var context = CreateContext();
        var accountsRepository = new AccountsRepository(context);
        var passwordHasher = new PasswordHasher();
        var createAccount = new CreateAccount(accountsRepository, passwordHasher);
        var controller = new AccountsController(
            new CreateAccountUseCase(createAccount),
            new UpdateAccountUseCase(accountsRepository),
            new DeleteAccountUseCase(accountsRepository),
            new GetAllAccountsUseCase(accountsRepository),
            new GetAccountUseCase(accountsRepository),
            new CreateAdminAccountUseCase(createAccount));

        AuthenticationMock.AuthenticateUser(accountNumberToAuthenticate ?? AccountExample().AccountNumber, controller);

        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private AccountDB AccountExample(string cpf = "1234")
        => new()
        {
            AccountNumber = int.Parse(cpf),
            CreatedAt = DateTime.Now,
            Active = true,
            Role = Role.Customer.ToString(),
            PasswordHash = _passwordHasher.Hash("pass123"),
            Owner = new CustomerDB()
            {
                Cpf = cpf,
                Name = "fulano",
            }
        };

    [Fact]
    public async Task GetAll_ReturnNotEmpty()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample("123"));
        context.Accounts.Add(AccountExample("321"));
        await context.SaveChangesAsync();

        var actionResult = await sut.GetAll();

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getAllOutput = Assert.IsType<GetAllAccountsOutput>(okObjectResult.Value);
        var savedAccounts = context.Accounts.ToList();
        Assert.Equal(savedAccounts.Count, getAllOutput.Accounts.Count);
        Assert.Equal(2, getAllOutput.Accounts.Count);
        Assert.Equal(savedAccounts[0].AccountNumber, getAllOutput.Accounts[0].AccountNumber);
        Assert.Equal(savedAccounts[0].Owner?.Name, getAllOutput.Accounts[0].Name);
        Assert.Equal(savedAccounts[1].AccountNumber, getAllOutput.Accounts[1].AccountNumber);
        Assert.Equal(savedAccounts[1].Owner?.Name, getAllOutput.Accounts[1].Name);
    }

    [Fact]
    public async Task GetAll_ReturnEmpty()
    {
        var (sut, context) = MakeSut();

        var actionResult = await sut.GetAll();

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getAllOutput = Assert.IsType<GetAllAccountsOutput>(okObjectResult.Value);
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
        await context.SaveChangesAsync();

        var actionResult = getType == "cpf"
            ? await sut.GetByCpf(account.Owner?.Cpf ?? "")
            : await sut.GetById(account.AccountNumber);

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var accountResult = Assert.IsType<GetAccountOutput>(okObjectResult.Value);
        Assert.Equal(account.AccountNumber, accountResult.AccountNumber);
        Assert.Equal(account.CreatedAt, accountResult.CreatedAt);
        Assert.Equal(account.Owner?.Cpf, accountResult.Owner?.Cpf);
        Assert.Equal(account.Owner?.Name, accountResult.Owner?.Name);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("cpf")]
    public async Task GetById_And_GetByCpf_ReturnNotFound(string getType)
    {
        var (sut, _) = MakeSut();

        var actionResult = getType == "cpf" ? await sut.GetByCpf("123") : await sut.GetById(1);

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

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
        Assert.Equal(Role.Customer.ToString(), savedAccount.Role);
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

    [Fact]
    public async Task Put_ReturnNoContent()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        var input = new UpdateAccountInput() { Name = "ciclano" };

        var actionResult = await sut.Put(account.AccountNumber, input);

        Assert.IsType<NoContentResult>(actionResult);
        var savedAccount = await context.Accounts.SingleAsync(savedAccount => savedAccount.Id == account.Id);
        Assert.Equal(account.AccountNumber, savedAccount.AccountNumber);
        Assert.Equal(account.Active, savedAccount.Active);
        Assert.Equal(account.CreatedAt, savedAccount.CreatedAt);
        Assert.Equal(account.Owner?.Id, savedAccount.Owner?.Id);
        Assert.Equal(account.Owner?.Cpf, savedAccount.Owner?.Cpf);
        Assert.Equal(input.Name, savedAccount.Owner?.Name);
        Assert.Equal(Role.Customer.ToString(), savedAccount.Role);
    }

    [Theory]
    [InlineData("put")]
    [InlineData("delete")]
    public async Task Put_And_Delete_ReturnNotFound(string type)
    {
        var (sut, _) = MakeSut();
        var accountNumberNotUsed = AccountExample().AccountNumber;
        var input = new UpdateAccountInput() { Name = "ciclano" };

        var actionResult = type == "put"
            ? await sut.Put(accountNumberNotUsed, input)
            : await sut.Delete(accountNumberNotUsed);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task Delete()
    {
        var (sut, context) = MakeSut();
        var account = AccountExample();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var actionResult = await sut.Delete(account.AccountNumber);

        Assert.IsType<NoContentResult>(actionResult);
        var savedAccount = context.Accounts.Single(account => account.Id == account.Id);
        Assert.Equal(account.AccountNumber, savedAccount.AccountNumber);
        Assert.False(savedAccount.Active);
        Assert.Equal(account.CreatedAt, savedAccount.CreatedAt);
        Assert.Equal(account.Owner, savedAccount.Owner);
    }

    [Theory]
    [InlineData("put")]
    [InlineData("delete")]
    public async Task AnotherAccount_ReturnUnauthorized(string type)
    {
        var (sut, context) = MakeSut(accountNumberToAuthenticate: 2);
        var account = AccountExample();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        var input = new UpdateAccountInput() { Name = "ciclano" };

        var output = type switch
        {
            "put" => await sut.Put(account.AccountNumber, input),
            "delete" => await sut.Delete(account.AccountNumber),
            _ => throw new Exception()
        };

        Assert.IsType<UnauthorizedObjectResult>(output);
    }

    private AccountDB AdminAccountExample(string cpf = "0123")
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
    public async Task Post_ReturnCreated_Admin()
    {
        var (sut, context) = MakeSut();
        var input = new CreateAccountInput()
        {
            Name = "fulano de tal",
            Cpf = "01234567890",
            Password = "pass123",
            PasswordConfirmation = "pass123",
        };

        var actionResult = await sut.PostAdmin(input);

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
    public async Task Post_ReturnBadRequest_Admin()
    {
        var (sut, context) = MakeSut();
        var account = AdminAccountExample();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        var input = new CreateAccountInput()
        {
            Name = "fulano de tal",
            Cpf = account.Owner?.Cpf ?? "",
            Password = "pass123",
            PasswordConfirmation = "pass123",
        };

        var actionResult = await sut.PostAdmin(input);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Post_ReturnBadRequest_PasswordAndConfirmationNotEqual_Admin()
    {
        var (sut, _) = MakeSut();
        var input = new CreateAccountInput()
        {
            Name = "fulano de tal",
            Cpf = "01234567890",
            Password = "pass123",
            PasswordConfirmation = "pass1234",
        };

        var actionResult = await sut.PostAdmin(input);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}