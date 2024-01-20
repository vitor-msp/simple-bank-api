using Application;
using Context;
using Microsoft.EntityFrameworkCore;
using Models;
using Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db config
builder.Services.AddDbContext<BankContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
builder.Services.AddScoped<IGetAllAccountsUseCase, GetAllAccountsUseCase>();
builder.Services.AddScoped<IGetAccountUseCase, GetAccountUseCase>();

builder.Services.AddScoped<IPostCreditUseCase, PostCreditUseCase>();
builder.Services.AddScoped<IPostDebitUseCase, PostDebitUseCase>();
builder.Services.AddScoped<IPostTransferUseCase, PostTransferUseCase>();
builder.Services.AddScoped<IGetBalanceUseCase, GetBalanceUseCase>();
builder.Services.AddScoped<IGetTransactionsUseCase, GetTransactionsUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { };