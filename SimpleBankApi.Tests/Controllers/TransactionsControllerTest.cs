using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleBankApi.Api.Controllers;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Application.UseCases;
using SimpleBankApi.Domain;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Dto;
using SimpleBankApi.Domain.Extensions;
using SimpleBankApi.Domain.Services;
using SimpleBankApi.Domain.ValueObjects;
using SimpleBankApi.Repository.Database.Context;
using SimpleBankApi.Repository.Database.Schema;
using SimpleBankApi.Repository.Implementations;
using Xunit;

namespace SimpleBankApi.Tests;

public class TransactionsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly AccountDB _account;
    private readonly IBankCache _bankCacheMock = new Mock<IBankCache>().Object;

    private const int _accountNumberNotUsed = 1000;

    public TransactionsControllerTest()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<BankContext>().UseSqlite(_connection).Options;
        _account = AccountExample();
    }

    private BankContext CreateContext()
    {
        var context = new BankContext(_contextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    private async Task<(TransactionsController, BankContext)> MakeSut(int? accountNumberToAuthenticate = null)
    {
        var context = CreateContext();
        var transactionsRepository = new TransactionsRepository(context);
        var accountsRepository = new AccountsRepository(context);
        var calculateBalance = new CalculateBalance(transactionsRepository, _bankCacheMock);

        var controller = new TransactionsController(
            new PostCreditUseCase(transactionsRepository, accountsRepository, _bankCacheMock),
            new PostDebitUseCase(transactionsRepository, accountsRepository, calculateBalance, _bankCacheMock),
            new PostTransferUseCase(transactionsRepository, accountsRepository, calculateBalance, _bankCacheMock),
            new GetBalanceUseCase(accountsRepository, calculateBalance),
            new GetTransactionsUseCase(transactionsRepository, accountsRepository));

        AuthenticationMock.AuthenticateUser(accountNumberToAuthenticate ?? _account.AccountNumber, controller);

        context.Accounts.Add(_account);
        await context.SaveChangesAsync();
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private static AccountDB AccountExample(int accountNumber = 1, string cpf = "123")
        => new()
        {
            AccountNumber = accountNumber,
            CreatedAt = DateTime.Now,
            Active = true,
            Role = Role.Customer.ToString(),
            Owner = new CustomerDB() { Cpf = cpf, Name = "fulano" }
        };

    private CreditDB CreditExample()
        => new() { CreatedAt = DateTime.Now, Value = 100, Account = _account };

    private DebitDB DebitExample()
        => new() { CreatedAt = DateTime.Now, Value = 10, Account = _account };

    private TransferDB TransferExampleAsSender(AccountDB recipient)
        => new() { CreatedAt = DateTime.Now, Value = 25, Sender = _account, Recipient = recipient };

    private TransferDB TransferExampleAsRecipient(AccountDB sender)
        => new() { CreatedAt = DateTime.Now, Value = 25, Sender = sender, Recipient = _account };

    [Fact]
    public async Task PostCredit_ReturnOk()
    {
        var (sut, context) = await MakeSut();
        var input = new CreditInput() { Value = 100.56 };

        var actionResult = await sut.PostCredit(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedCredits = context.Credits.Where(credit
            => credit.Account != null && credit.Account.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedCredits);
        Assert.Equal(input.Value, savedCredits[0].Value);
        Assert.Equal(_account, savedCredits[0].Account);
        Assert.IsType<int>(savedCredits[0].Id);
        Assert.IsType<DateTime>(savedCredits[0].CreatedAt);
    }

    [Fact]
    public async Task PostDebit_ReturnOk()
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        await context.SaveChangesAsync();
        var input = new DebitInput() { Value = 50.56 };

        var actionResult = await sut.PostDebit(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedDebits = await context.Debits
            .Where(debit => debit.Account != null && debit.Account.AccountNumber == _account.AccountNumber).ToListAsync();
        Assert.Single(savedDebits);
        Assert.Equal(input.Value, savedDebits[0].Value);
        Assert.Equal(_account, savedDebits[0].Account);
        Assert.IsType<int>(savedDebits[0].Id);
        Assert.IsType<DateTime>(savedDebits[0].CreatedAt);
    }

    [Fact]
    public async Task PostTransfer_ReturnOk()
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        await context.SaveChangesAsync();
        var input = new TransferInput() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedTransfers = context.Transfers.Where(transfer => transfer.Sender!.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedTransfers);
        Assert.Equal(input.Value, savedTransfers[0].Value);
        Assert.Equal(_account, savedTransfers[0].Sender);
        Assert.Equal(recipientAccount, savedTransfers[0].Recipient);
        Assert.IsType<int>(savedTransfers[0].Id);
        Assert.IsType<DateTime>(savedTransfers[0].CreatedAt);
    }

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    [InlineData("get-balance")]
    [InlineData("get-transactions")]
    public async Task PostCredit_And_PostDebit_And_GetBalance_ReturnNotFound(string type)
    {
        var (sut, _) = await MakeSut(accountNumberToAuthenticate: _accountNumberNotUsed);
        var creditInput = new CreditInput() { Value = 100.56 };
        var debitInput = new DebitInput() { Value = 100.56 };

        var actionResult = (type) switch
        {
            "credit" => await sut.PostCredit(_accountNumberNotUsed, creditInput),
            "debit" => await sut.PostDebit(_accountNumberNotUsed, debitInput),
            "get-balance" => (await sut.GetBalance(_accountNumberNotUsed)).Result,
            "get-transactions" => (await sut.GetTransactions(_accountNumberNotUsed)).Result,
            _ => throw new Exception()
        };

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    [InlineData("transfer")]
    public async Task PostCredit_And_PostDebit_And_PostTransfer_NegativeInput(string type)
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        await context.SaveChangesAsync();
        var creditInput = new CreditInput() { Value = -50 };
        var debitInput = new DebitInput() { Value = -50 };
        var transferInput = new TransferInput() { Value = -50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = (type) switch
        {
            "credit" => await sut.PostCredit(_account.AccountNumber, creditInput),
            "debit" => await sut.PostDebit(_account.AccountNumber, debitInput),
            "transfer" => await sut.PostTransfer(_account.AccountNumber, transferInput),
            _ => throw new Exception()
        };

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Theory]
    [InlineData("debit")]
    [InlineData("transfer")]
    public async Task PostDebit_And_PostTransfer_InsufficientBalance(string type)
    {
        var (sut, context) = await MakeSut();
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        await context.SaveChangesAsync();
        var debitInput = new DebitInput() { Value = 50 };
        var transferInput = new TransferInput() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = type == "debit"
            ? await sut.PostDebit(_account.AccountNumber, debitInput)
            : await sut.PostTransfer(_account.AccountNumber, transferInput);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_SenderNotFound()
    {
        var (sut, context) = await MakeSut(accountNumberToAuthenticate: _accountNumberNotUsed);
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        await context.SaveChangesAsync();
        var input = new TransferInput() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = await sut.PostTransfer(_accountNumberNotUsed, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_RecipientNotFound()
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        await context.SaveChangesAsync();
        var input = new TransferInput() { Value = 50.56, RecipientAccountNumber = _accountNumberNotUsed };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_SameAccount()
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        await context.SaveChangesAsync();
        var input = new TransferInput() { Value = 50.56, RecipientAccountNumber = _account.AccountNumber };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task GetBalance()
    {
        var (sut, context) = await MakeSut();
        var credit = CreditExample();
        var debit = DebitExample();
        context.Credits.Add(credit);
        context.Debits.Add(debit);
        await context.SaveChangesAsync();

        var actionResult = await sut.GetBalance(_account.AccountNumber);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getBalanceOutput = Assert.IsType<GetBalanceOutput>(okResult.Value);
        string expectedBalance = (credit.Value - debit.Value).ToString("c", CultureInfo.GetCultureInfo("pt-BR")); ;
        Assert.Equal(expectedBalance, getBalanceOutput.Balance);
    }

    private static TransactionDto GenerateTransactionCreditDto(CreditDB credit)
    {
        return new TransactionDto()
        {
            Type = TransactionType.Credit,
            CreditDto = new CreditDto()
            {
                Value = credit.Value.ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
                CreatedAt = DateTime.SpecifyKind(credit.CreatedAt, DateTimeKind.Unspecified)
            }
        };
    }

    private static TransactionDto GenerateTransactionDebitDto(DebitDB debit)
    {
        return new TransactionDto()
        {
            Type = TransactionType.Debit,
            DebitDto = new DebitDto()
            {
                Value = (-1 * debit.Value).ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
                CreatedAt = DateTime.SpecifyKind(debit.CreatedAt, DateTimeKind.Unspecified)
            }
        };
    }

    private static TransactionDto GenerateTransactionTransferDto(TransferDB transfer, bool AsSender = true)
    {
        if (transfer.Sender?.Owner == null || transfer.Recipient?.Owner == null)
            throw new Exception();

        double value = AsSender ? -1 * transfer.Value : transfer.Value;
        return new TransactionDto()
        {
            Type = TransactionType.Transfer,
            TransferDto = new TransferDto()
            {
                Value = value.GetBrazilianCurrency(),
                CreatedAt = transfer.CreatedAt,
                Sender = new AccountDto()
                {
                    AccountNumber = transfer.Sender.AccountNumber,
                    Name = transfer.Sender.Owner.Name
                },
                Recipient = new AccountDto()
                {
                    AccountNumber = transfer.Recipient.AccountNumber,
                    Name = transfer.Recipient.Owner.Name
                },
            }

        };
    }

    [Fact]
    public async Task GetTransactions()
    {
        var (sut, context) = await MakeSut();
        var credit = CreditExample();
        var debit = DebitExample();
        var recipient = AccountExample(2, "321");
        var transferAsSender = TransferExampleAsSender(recipient);
        var transferAsRecipient = TransferExampleAsRecipient(recipient);
        context.Credits.Add(credit);
        context.Debits.Add(debit);
        context.Transfers.Add(transferAsSender);
        context.Transfers.Add(transferAsRecipient);
        await context.SaveChangesAsync();

        var actionResult = await sut.GetTransactions(_account.AccountNumber);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getTransactionsOutput = Assert.IsType<GetTransactionsOutput>(okResult.Value);
        Assert.Equal(4, getTransactionsOutput.Statement.Transactions.Count);

        var expectedCredit = GenerateTransactionCreditDto(credit);
        var receivedCredit = getTransactionsOutput.Statement.Transactions[0];
        Assert.Equal(expectedCredit.Type, receivedCredit.Type);
        Assert.Equal(expectedCredit.CreditDto?.CreatedAt, receivedCredit.CreditDto?.CreatedAt);
        Assert.Equal(expectedCredit.CreditDto?.Value, receivedCredit.CreditDto?.Value);

        var expectedDebit = GenerateTransactionDebitDto(debit);
        var receivedDebit = getTransactionsOutput.Statement.Transactions[1];
        Assert.Equal(expectedDebit.Type, receivedDebit.Type);
        Assert.Equal(expectedDebit.DebitDto?.CreatedAt, receivedDebit.DebitDto?.CreatedAt);
        Assert.Equal(expectedDebit.DebitDto?.Value, receivedDebit.DebitDto?.Value);

        var expectedTransferAsSender = GenerateTransactionTransferDto(transferAsSender, true);
        var receivedTransferAsSender = getTransactionsOutput.Statement.Transactions[2];
        Assert.Equal(expectedTransferAsSender.Type, receivedTransferAsSender.Type);
        Assert.Equal(expectedTransferAsSender.TransferDto?.Value, receivedTransferAsSender.TransferDto?.Value);
        Assert.Equal(expectedTransferAsSender.TransferDto?.CreatedAt, receivedTransferAsSender.TransferDto?.CreatedAt);
        Assert.Equal(expectedTransferAsSender.TransferDto?.Sender?.AccountNumber, receivedTransferAsSender.TransferDto?.Sender?.AccountNumber);
        Assert.Equal(expectedTransferAsSender.TransferDto?.Sender?.Name, receivedTransferAsSender.TransferDto?.Sender?.Name);
        Assert.Equal(expectedTransferAsSender.TransferDto?.Recipient?.AccountNumber, receivedTransferAsSender.TransferDto?.Recipient?.AccountNumber);
        Assert.Equal(expectedTransferAsSender.TransferDto?.Recipient?.Name, receivedTransferAsSender.TransferDto?.Recipient?.Name);

        var expectedTransferAsRecipient = GenerateTransactionTransferDto(transferAsRecipient, false);
        var receivedTransferAsRecipient = getTransactionsOutput.Statement.Transactions[3];
        Assert.Equal(expectedTransferAsRecipient.Type, receivedTransferAsRecipient.Type);
        Assert.Equal(expectedTransferAsRecipient.TransferDto?.Value, receivedTransferAsRecipient.TransferDto?.Value);
        Assert.Equal(expectedTransferAsRecipient.TransferDto?.CreatedAt, receivedTransferAsRecipient.TransferDto?.CreatedAt);
        Assert.Equal(expectedTransferAsRecipient.TransferDto?.Sender?.AccountNumber, receivedTransferAsRecipient.TransferDto?.Sender?.AccountNumber);
        Assert.Equal(expectedTransferAsRecipient.TransferDto?.Sender?.Name, receivedTransferAsRecipient.TransferDto?.Sender?.Name);
        Assert.Equal(expectedTransferAsRecipient.TransferDto?.Recipient?.AccountNumber, receivedTransferAsRecipient.TransferDto?.Recipient?.AccountNumber);
        Assert.Equal(expectedTransferAsRecipient.TransferDto?.Recipient?.Name, receivedTransferAsRecipient.TransferDto?.Recipient?.Name);
    }

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    [InlineData("transfer")]
    public async Task AnotherAccount_ReturnUnauthorized(string type)
    {
        var (sut, _) = await MakeSut(accountNumberToAuthenticate: 2);
        Input input = type switch
        {
            "credit" => new CreditInput() { Value = 100.56 },
            "debit" => new DebitInput() { Value = 50.56 },
            "transfer" => new TransferInput() { Value = 50.56, RecipientAccountNumber = 2 },
            _ => throw new Exception()
        };

        var output = type switch
        {
            "credit" => await sut.PostCredit(_account.AccountNumber, (CreditInput)input),
            "debit" => await sut.PostDebit(_account.AccountNumber, (DebitInput)input),
            "transfer" => await sut.PostTransfer(_account.AccountNumber, (TransferInput)input),
            _ => throw new Exception()
        };

        Assert.IsType<UnauthorizedObjectResult>(output);
    }

    [Fact]
    public async Task AnotherAccount_ReturnUnauthorized_GetBalance()
    {
        var (sut, _) = await MakeSut(accountNumberToAuthenticate: 2);

        var output = await sut.GetBalance(_account.AccountNumber);

        Assert.IsType<UnauthorizedObjectResult>(output.Result);
    }

    [Fact]
    public async Task AnotherAccount_ReturnUnauthorized_GetTransactions()
    {
        var (sut, _) = await MakeSut(accountNumberToAuthenticate: 2);

        var output = await sut.GetTransactions(_account.AccountNumber);

        Assert.IsType<UnauthorizedObjectResult>(output.Result);
    }
}