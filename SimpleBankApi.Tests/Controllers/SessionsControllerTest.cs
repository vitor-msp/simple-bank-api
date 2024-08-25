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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleBankApi.Api.Controllers;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain.Configuration;
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
    private readonly string _refreshToken = Guid.NewGuid().ToString();

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

        var configuration = new TokenConfiguration();
        _configuration.GetSection("Token").Bind(configuration);
        var options = Options.Create(configuration);

        var accountsRepository = new AccountsRepository(context);
        var passwordHasher = new PasswordHasher();
        var tokenProvider = new TokenProvider(options);

        var controller = new SessionsController(
            new LoginUseCase(accountsRepository, passwordHasher, tokenProvider, options),
            new RefreshTokenUseCase(accountsRepository, tokenProvider),
            new LogoutUseCase(accountsRepository, passwordHasher));
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private AccountDB AccountExample(string? refreshToken = null, DateTime? refreshTokenExpiration = null)
        => new()
        {
            AccountNumber = 1,
            CreatedAt = DateTime.Now,
            Active = true,
            PasswordHash = _passwordHasher.Hash("pass123"),
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
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
    public async void Login_ReturnAccessAndRefreshToken()
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

        Assert.IsType<string>(output.RefreshToken);
        var savedAccount = context.Accounts.ToList()[0];
        Assert.Equal(savedAccount.RefreshToken, output.RefreshToken);
        Assert.NotNull(savedAccount.RefreshTokenExpiration);
        Assert.True(savedAccount.RefreshTokenExpiration > DateTime.UtcNow);

        Assert.IsType<string>(output.AccessToken);
        var claims = ValidateToken(output.AccessToken);
        var accountNumber = claims.FindFirstValue(ClaimTypes.Name);
        Assert.Equal("1", accountNumber);
    }

    [Fact]
    public async void Login_ReturnUnauthorized_AccountNotFound()
    {
        var (sut, _) = MakeSut();
        var input = new LoginInput()
        {
            AccountNumber = 1,
            Password = "pass123"
        };

        var actionResult = await sut.Login(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async void Login_ReturnUnauthorized_IncorrectPassword()
    {
        {
            var (sut, context) = MakeSut();
            context.Accounts.Add(AccountExample());
            await context.SaveChangesAsync();
            var input = new LoginInput()
            {
                AccountNumber = 1,
                Password = "pass1234"
            };

            var actionResult = await sut.Login(input);

            Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        }
    }

    [Fact]
    public async void RefreshToken_ReturnAccessToken()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample(_refreshToken, DateTime.Now.AddMinutes(15)));
        await context.SaveChangesAsync();
        var input = new RefreshTokenInput()
        {
            AccountNumber = 1,
            RefreshToken = _refreshToken
        };

        var actionResult = await sut.RefreshToken(input);

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var output = Assert.IsType<RefreshTokenOutput>(okObjectResult.Value);

        Assert.IsType<string>(output.AccessToken);
        var claims = ValidateToken(output.AccessToken);
        var accountNumber = claims.FindFirstValue(ClaimTypes.Name);
        Assert.Equal("1", accountNumber);
    }

    [Fact]
    public async void RefreshToken_ReturnUnauthorized_AccountNotFound()
    {
        var (sut, _) = MakeSut();
        var input = new RefreshTokenInput()
        {
            AccountNumber = 1,
            RefreshToken = _refreshToken
        };

        var actionResult = await sut.RefreshToken(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async void RefreshToken_ReturnUnauthorized_TokenNotMatch()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample(_refreshToken, DateTime.Now.AddMinutes(15)));
        await context.SaveChangesAsync();
        var input = new RefreshTokenInput()
        {
            AccountNumber = 1,
            RefreshToken = Guid.NewGuid().ToString()
        };

        var actionResult = await sut.RefreshToken(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async void RefreshToken_ReturnUnauthorized_TokenExpired()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample(_refreshToken, DateTime.Now.AddMinutes(-1)));
        await context.SaveChangesAsync();
        var input = new RefreshTokenInput()
        {
            AccountNumber = 1,
            RefreshToken = _refreshToken
        };

        var actionResult = await sut.RefreshToken(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async void Logout_ReturnNoContent()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample(_refreshToken, DateTime.Now.AddMinutes(15)));
        await context.SaveChangesAsync();
        var input = new LogoutInput()
        {
            AccountNumber = 1,
            RefreshToken = _refreshToken
        };

        var actionResult = await sut.Logout(input);

        Assert.IsType<NoContentResult>(actionResult.Result);
        var savedAccount = context.Accounts.ToList()[0];
        Assert.NotEqual(_refreshToken, savedAccount.RefreshToken);
    }

    [Fact]
    public async void Logout_ReturnUnauthorized_AccountNotFound()
    {
        var (sut, _) = MakeSut();
        var input = new LogoutInput()
        {
            AccountNumber = 1,
            RefreshToken = _refreshToken
        };

        var actionResult = await sut.Logout(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async void Logout_ReturnUnauthorized_TokenNotMatch()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample(_refreshToken, DateTime.Now.AddMinutes(15)));
        await context.SaveChangesAsync();
        var input = new LogoutInput()
        {
            AccountNumber = 1,
            RefreshToken = Guid.NewGuid().ToString()
        };

        var actionResult = await sut.Logout(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async void Logout_ReturnUnauthorized_TokenExpired()
    {
        var (sut, context) = MakeSut();
        context.Accounts.Add(AccountExample(_refreshToken, DateTime.Now.AddMinutes(-1)));
        await context.SaveChangesAsync();
        var input = new LogoutInput()
        {
            AccountNumber = 1,
            RefreshToken = _refreshToken
        };

        var actionResult = await sut.Logout(input);

        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }
}