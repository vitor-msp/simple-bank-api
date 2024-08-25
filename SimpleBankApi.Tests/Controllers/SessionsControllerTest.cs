using System;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SimpleBankApi.Api.Controllers;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Infra;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Database.Schema;
using SimpleBankApi.Repository.Implementations;
using Xunit;

namespace SimpleBankApi.Tests;

public class SessionsControllerTest
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();
    private readonly IConfiguration _configuration;

    public SessionsControllerTest()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<BankContext>().UseSqlite(_connection).Options;
        _configuration = LoadTokenConfiguration();
    }

    private static IConfiguration LoadTokenConfiguration()
        => new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .Build();

    private BankContext CreateContext()
    {
        var context = new BankContext(_contextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    private (SessionsController, BankContext) MakeSut()
    {
        var context = CreateContext();
        var controller = new SessionsController(new LoginUseCase(new AccountsRepository(context)));
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private AccountDB AccountExample()
        => new()
        {
            AccountNumber = 1,
            CreatedAt = DateTime.Now,
            Active = true,
            PasswordHash = _passwordHasher.Hash("pass123"),
            Owner = new CustomerDB()
            {
                Cpf = "0123",
                Name = "fulano",
            }
        };

    private ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration.GetSection("Token")["Key"] ?? "");
        return tokenHandler.ValidateToken(token, new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);
    }

    [Fact]
    public async void Post_ReturnAccessAndRefreshToken()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample());
        await context.SaveChangesAsync();
        var input = new LoginInput()
        {
            AccountNumber = 1,
            Password = "pass123"
        };

        var actionResult = await sut.Login(input);

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var output = Assert.IsType<LoginOutput>(okObjectResult.Value);

        Assert.IsType<Guid>(output.RefreshToken);
        var savedAccount = context.Accounts.ToList()[0];
        Assert.Equal(savedAccount.RefreshToken, output.RefreshToken);

        Assert.IsType<string>(output.AccessToken);
        var claims = ValidateToken(output.AccessToken);
        var accountNumber = claims.FindFirstValue(ClaimTypes.Name);
        Assert.Equal("1", accountNumber);
    }

    [Fact]
    public void Post_ReturnUnauthorized_EmailNotFound()
    {
    }

    [Fact]
    public void Post_ReturnUnauthorized_IncorrectPassword()
    {
    }
}