using System;
using System.Data.Common;
using System.Globalization;
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

public class TransactionsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly Account _account;

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

    private (TransactionsController, BankContext) MakeSut()
    {
        var context = CreateContext();
        var controller = new TransactionsController(context);
        context.Accounts.Add(_account);
        context.SaveChanges();
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private Account AccountExample(int accountNumber = 1, string cpf = "123")
    {
        return new Account()
        {
            AccountNumber = accountNumber,
            Active = true,
            CreatedAt = DateTime.Now,
            Owner = new Customer() { Cpf = cpf, Name = "fulano" }
        };
    }

    private Credit CreditExample()
    {
        return new Credit() { Value = 100, Account = _account, CreatedAt = DateTime.Now };
    }

    private Debit DebitExample()
    {
        return new Debit() { Value = -10, Account = _account, CreatedAt = DateTime.Now };
    }

    private Transfer TransferExample(Account recipient)
    {
        return new Transfer() { Value = 25, Sender = _account, Recipient = recipient, CreatedAt = DateTime.Now };
    }

    [Fact]
    public async Task PostCredit_ReturnOk()
    {
        var (sut, context) = MakeSut();
        var input = new CreditDto() { Value = 100.56 };

        var actionResult = await sut.PostCredit(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedCredits = context.Credits.Where(credit => credit.Account.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedCredits);
        Assert.Equal(input.Value, savedCredits[0].Value);
        Assert.Equal(_account, savedCredits[0].Account);
        Assert.IsType<int>(savedCredits[0].Id);
        Assert.IsType<DateTime>(savedCredits[0].CreatedAt);
    }

    [Fact]
    public async Task PostDebit_ReturnOk()
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        context.SaveChanges();
        var input = new DebitDto() { Value = 50.56 };

        var actionResult = await sut.PostDebit(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedDebits = context.Debits.Where(debit => debit.Account.AccountNumber == _account.AccountNumber).ToList();
        Assert.Single(savedDebits);
        Assert.Equal(input.Value, -1 * savedDebits[0].Value);
        Assert.Equal(_account, savedDebits[0].Account);
        Assert.IsType<int>(savedDebits[0].Id);
        Assert.IsType<DateTime>(savedDebits[0].CreatedAt);
    }

    [Fact]
    public async Task PostTransfer_ReturnOk()
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        context.SaveChanges();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<OkResult>(actionResult);
        var savedTransfers = context.Transfers.Where(transfer => transfer.Sender.AccountNumber == _account.AccountNumber).ToList();
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
    public async Task PostCredit_And_PostDebit_ReturnNotFound(string type)
    {
        var (sut, context) = MakeSut();
        var creditInput = new CreditDto() { Value = 100.56 };
        var debitInput = new DebitDto() { Value = 100.56 };

        var actionResult = type == "credit"
            ? await sut.PostCredit(_accountNumberNotUsed, creditInput)
            : await sut.PostDebit(_accountNumberNotUsed, debitInput);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Theory]
    [InlineData("credit")]
    [InlineData("debit")]
    public async Task PostCredit_And_PostDebit_NegativeInput(string type)
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        context.SaveChanges();
        var creditInput = new CreditDto() { Value = -50 };
        var debitInput = new DebitDto() { Value = -50 };

        var actionResult = type == "credit"
            ? await sut.PostCredit(_account.AccountNumber, creditInput)
            : await sut.PostDebit(_account.AccountNumber, debitInput);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostDebit_InsufficientBalance()
    {
        var (sut, context) = MakeSut();
        var input = new DebitDto() { Value = 50 };

        var actionResult = await sut.PostDebit(_account.AccountNumber, input);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_SenderNotFound()
    {
        var (sut, context) = MakeSut();
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        context.SaveChanges();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = await sut.PostTransfer(_accountNumberNotUsed, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_RecipientNotFound()
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        context.SaveChanges();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = _accountNumberNotUsed };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_SameAccount()
    {
        var (sut, context) = MakeSut();
        context.Credits.Add(CreditExample());
        context.SaveChanges();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = _account.AccountNumber };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task GetBalance()
    {
        var (sut, context) = MakeSut();
        var credit = CreditExample();
        var debit = DebitExample();
        context.Credits.Add(credit);
        context.Debits.Add(debit);
        context.SaveChanges();

        var actionResult = await sut.GetBalance(_account.AccountNumber);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getBalanceOutput = Assert.IsType<TransactionsController.GetBalanceOutput>(okResult.Value);
        string expectedBalance = (credit.Value + debit.Value).ToString("c", CultureInfo.GetCultureInfo("pt-BR")); ;
        Assert.Equal(expectedBalance, getBalanceOutput.Balance);
    }

    private TransactionCreditDebitDto GenerateTransactionCreditDto(Credit credit)
    {
        return new TransactionCreditDebitDto()
        {
            Type = TransactionType.Credit,
            Value = credit.Value.ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
            CreatedAt = DateTime.SpecifyKind(credit.CreatedAt, DateTimeKind.Unspecified)
        };
    }

    private TransactionCreditDebitDto GenerateTransactionDebitDto(Debit debit)
    {
        return new TransactionCreditDebitDto()
        {
            Type = TransactionType.Debit,
            Value = debit.Value.ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
            CreatedAt = DateTime.SpecifyKind(debit.CreatedAt, DateTimeKind.Unspecified)
        };
    }

    private TransactionTransferDto GenerateTransactionTransferDto(Transfer transfer)
    {
        return new TransactionTransferDto()
        {
            Type = TransactionType.Transfer,
            Value = (-1 * transfer.Value).ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
            CreatedAt = DateTime.SpecifyKind(transfer.CreatedAt, DateTimeKind.Unspecified),
            Sender = new TransactionAccountDto()
            {
                AccountNumber = transfer.Sender.AccountNumber,
                Name = transfer.Sender.Owner.Name
            },
            Recipient = new TransactionAccountDto()
            {
                AccountNumber = transfer.Recipient.AccountNumber,
                Name = transfer.Recipient.Owner.Name
            }
        };
    }

    [Fact]
    public async Task GetTransactions()
    {
        var (sut, context) = MakeSut();
        var credit = CreditExample();
        var debit = DebitExample();
        var recipient = AccountExample(2, "321");
        var transfer = TransferExample(recipient);
        context.Credits.Add(credit);
        context.Debits.Add(debit);
        context.Transfers.Add(transfer);
        context.SaveChanges();

        var actionResult = await sut.GetTransactions(_account.AccountNumber);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var getTransactionsOutput = Assert.IsType<TransactionsController.GetTransactionsOutput>(okResult.Value);
        Assert.Equal(3, getTransactionsOutput.Transactions.Count);

        var expectedCredit = GenerateTransactionCreditDto(credit);
        var receivedCredit = getTransactionsOutput.Transactions[0] as TransactionCreditDebitDto;
        Assert.Equal(expectedCredit.Type, receivedCredit?.Type);
        Assert.Equal(expectedCredit.CreatedAt, receivedCredit?.CreatedAt);
        Assert.Equal(expectedCredit.Value, receivedCredit?.Value);

        var expectedDebit = GenerateTransactionDebitDto(debit);
        var receivedDebit = getTransactionsOutput.Transactions[1] as TransactionCreditDebitDto;
        Assert.Equal(expectedDebit.Type, receivedDebit?.Type);
        Assert.Equal(expectedDebit.CreatedAt, receivedDebit?.CreatedAt);
        Assert.Equal(expectedDebit.Value, receivedDebit?.Value);

        var expectedTransfer = GenerateTransactionTransferDto(transfer);
        var receivedTransfer = getTransactionsOutput.Transactions[2] as TransactionTransferDto;
        Assert.Equal(expectedTransfer.Type, receivedTransfer?.Type);
        Assert.Equal(expectedTransfer.Value, receivedTransfer?.Value);
        Assert.Equal(expectedTransfer.CreatedAt, receivedTransfer?.CreatedAt);
        Assert.Equal(expectedTransfer.Sender.AccountNumber, receivedTransfer?.Sender.AccountNumber);
        Assert.Equal(expectedTransfer.Sender.Name, receivedTransfer?.Sender.Name);
        Assert.Equal(expectedTransfer.Recipient.AccountNumber, receivedTransfer?.Recipient.AccountNumber);
        Assert.Equal(expectedTransfer.Recipient.Name, receivedTransfer?.Recipient.Name);
    }
}