using System;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;
using Context;
using Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

    public void Dispose() => _connection.Dispose();


    [Fact]
    public async Task Test1()
    {
        var controller = new AccountsController(CreateContext());

        var result = (await controller.GetAll()) as OkObjectResult;

        Console.WriteLine(JsonSerializer.Serialize(result?.Value));
    }

}