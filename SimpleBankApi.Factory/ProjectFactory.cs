using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Infra;
using SimpleBankApi.Repository.Cache;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Implementations;

namespace SimpleBankApi.Factory;

public static class ProjectFactory
{
    public static void BuildProject(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BankContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("SqliteConnection")));

        services.Configure<RedisConfiguration>(configuration.GetSection("Redis"));

        services.AddScoped<ICalculateBalance, CalculateBalance>();

        services.AddScoped<IAccountsRepository, AccountsRepository>();
        services.AddScoped<ITransactionsRepository, TransactionsRepository>();

        services.AddScoped<IBankCache, BankCacheRedis>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

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

        services.AddAllElasticApm();
    }
}