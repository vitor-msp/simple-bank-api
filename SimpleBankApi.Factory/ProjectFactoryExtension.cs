using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain.Configuration;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Infra;
using SimpleBankApi.Repository.Cache;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Implementations;

namespace SimpleBankApi.Factory;

public static class ProjectFactoryExtension
{
    public static void BuildProject(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BankContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("SqliteConnection")));

        services.Configure<RedisConfiguration>(configuration.GetSection("Redis"));
        services.Configure<TokenConfiguration>(configuration.GetSection("Token"));
        ConfigureToken(services, configuration);

        services.AddScoped<ICalculateBalance, CalculateBalance>();

        services.AddScoped<IAccountsRepository, AccountsRepository>();
        services.AddScoped<ITransactionsRepository, TransactionsRepository>();

        services.AddScoped<IBankCache, BankCacheRedis>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        services.AddScoped<IUpdateAccountUseCase, UpdateAccountUseCase>();
        services.AddScoped<IDeleteAccountUseCase, DeleteAccountUseCase>();
        services.AddScoped<IGetAllAccountsUseCase, GetAllAccountsUseCase>();
        services.AddScoped<IGetAccountUseCase, GetAccountUseCase>();

        services.AddScoped<IPostCreditUseCase, PostCreditUseCase>();
        services.AddScoped<IPostDebitUseCase, PostDebitUseCase>();
        services.AddScoped<IPostTransferUseCase, PostTransferUseCase>();
        services.AddScoped<IGetBalanceUseCase, GetBalanceUseCase>();
        services.AddScoped<IGetTransactionsUseCase, GetTransactionsUseCase>();

        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();

        services.AddAllElasticApm();
    }

    private static void ConfigureToken(IServiceCollection services, IConfiguration configuration)
    {
        var key = configuration.GetSection("Token")["Key"];
        if (key == null) throw new Exception("Missing configure token key.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
        });
        services.AddAuthorization();
    }
}