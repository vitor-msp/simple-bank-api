using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Context;
using Controllers;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using Repository;
using Xunit;

namespace SimpleBankApi.Tests;

public class TransactionsControllerTest : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<BankContext> _contextOptions;
    private readonly AccountDB _account;

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

    private async Task<(TransactionsController, BankContext)> MakeSut()
    {
        var context = CreateContext();
        var controller = new TransactionsController(new TransactionsRepository(context), new AccountsRepository(context));
        context.Accounts.Add(_account);
        await context.SaveChangesAsync();
        return (controller, context);
    }

    public void Dispose() => _connection.Dispose();

    private AccountDB AccountExample(int accountNumber = 1, string cpf = "123")
    {
        return new AccountDB()
        {
            AccountNumber = accountNumber,
            CreatedAt = DateTime.Now,
            Active = true,
            Owner = new CustomerDB() { Cpf = cpf, Name = "fulano" }
        };
    }

    private CreditDB CreditExample()
    {
        return new CreditDB() { CreatedAt = DateTime.Now, Value = 100, Account = _account };
    }

    private DebitDB DebitExample()
    {
        return new DebitDB() { CreatedAt = DateTime.Now, Value = 10, Account = _account };
    }

    private TransferDB TransferExampleAsSender(AccountDB recipient)
    {
        return new TransferDB() { CreatedAt = DateTime.Now, Value = 25, Sender = _account, Recipient = recipient };
    }

    private TransferDB TransferExampleAsRecipient(AccountDB sender)
    {
        return new TransferDB() { CreatedAt = DateTime.Now, Value = 25, Sender = sender, Recipient = _account };
    }

    [Fact]
    public async Task PostCredit_ReturnOk()
    {
        var (sut, context) = await MakeSut();
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
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        await context.SaveChangesAsync();
        var input = new DebitDto() { Value = 50.56 };

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
    [InlineData("get-balance")]
    [InlineData("get-transactions")]
    public async Task PostCredit_And_PostDebit_And_GetBalance_ReturnNotFound(string type)
    {
        var (sut, context) = await MakeSut();
        var creditInput = new CreditDto() { Value = 100.56 };
        var debitInput = new DebitDto() { Value = 100.56 };

        var actionResult = (type) switch
        {
            "credit" => await sut.PostCredit(_accountNumberNotUsed, creditInput),
            "debit" => await sut.PostDebit(_accountNumberNotUsed, debitInput),
            "get-balance" => (await sut.GetBalance(_accountNumberNotUsed)).Result,
            "get-transactions" => (await sut.GetTransactions(_accountNumberNotUsed)).Result,
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
        var creditInput = new CreditDto() { Value = -50 };
        var debitInput = new DebitDto() { Value = -50 };
        var transferInput = new TransferDto() { Value = -50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = (type) switch
        {
            "credit" => await sut.PostCredit(_account.AccountNumber, creditInput),
            "debit" => await sut.PostDebit(_account.AccountNumber, debitInput),
            "transfer" => await sut.PostTransfer(_account.AccountNumber, transferInput),
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
        var debitInput = new DebitDto() { Value = 50 };
        var transferInput = new TransferDto() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = type == "debit"
            ? await sut.PostDebit(_account.AccountNumber, debitInput)
            : await sut.PostTransfer(_account.AccountNumber, transferInput);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_SenderNotFound()
    {
        var (sut, context) = await MakeSut();
        var recipientAccount = AccountExample(2, "321");
        context.Accounts.Add(recipientAccount);
        await context.SaveChangesAsync();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = recipientAccount.AccountNumber };

        var actionResult = await sut.PostTransfer(_accountNumberNotUsed, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_RecipientNotFound()
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        await context.SaveChangesAsync();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = _accountNumberNotUsed };

        var actionResult = await sut.PostTransfer(_account.AccountNumber, input);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public async Task PostTransfer_SameAccount()
    {
        var (sut, context) = await MakeSut();
        context.Credits.Add(CreditExample());
        await context.SaveChangesAsync();
        var input = new TransferDto() { Value = 50.56, RecipientAccountNumber = _account.AccountNumber };

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
        var getBalanceOutput = Assert.IsType<TransactionsController.GetBalanceOutput>(okResult.Value);
        string expectedBalance = (credit.Value - debit.Value).ToString("c", CultureInfo.GetCultureInfo("pt-BR")); ;
        Assert.Equal(expectedBalance, getBalanceOutput.Balance);
    }

    private TransactionCreditDebitDto GenerateTransactionCreditDto(CreditDB credit)
    {
        return new TransactionCreditDebitDto()
        {
            Type = TransactionType.Credit,
            Value = credit.Value.ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
            CreatedAt = DateTime.SpecifyKind(credit.CreatedAt, DateTimeKind.Unspecified)
        };
    }

    private TransactionCreditDebitDto GenerateTransactionDebitDto(DebitDB debit)
    {
        return new TransactionCreditDebitDto()
        {
            Type = TransactionType.Debit,
            Value = debit.Value.ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
            CreatedAt = DateTime.SpecifyKind(debit.CreatedAt, DateTimeKind.Unspecified)
        };
    }

    private TransactionTransferDto GenerateTransactionTransferDto(TransferDB transfer, bool AsSender = true)
    {
        double value = AsSender ? -1 * transfer.Value : transfer.Value;
        return new TransactionTransferDto()
        {
            Type = TransactionType.Transfer,
            Value = value.ToString("c", CultureInfo.GetCultureInfo("pt-BR")),
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
        var getTransactionsOutput = Assert.IsType<TransactionsController.GetTransactionsOutput>(okResult.Value);
        Assert.Equal(4, getTransactionsOutput.Transactions.Count);

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

        var expectedTransferAsSender = GenerateTransactionTransferDto(transferAsSender, true);
        var receivedTransferAsSender = getTransactionsOutput.Transactions[2] as TransactionTransferDto;
        Assert.Equal(expectedTransferAsSender.Type, receivedTransferAsSender?.Type);
        Assert.Equal(expectedTransferAsSender.Value, receivedTransferAsSender?.Value);
        Assert.Equal(expectedTransferAsSender.CreatedAt, receivedTransferAsSender?.CreatedAt);
        Assert.Equal(expectedTransferAsSender.Sender.AccountNumber, receivedTransferAsSender?.Sender.AccountNumber);
        Assert.Equal(expectedTransferAsSender.Sender.Name, receivedTransferAsSender?.Sender.Name);
        Assert.Equal(expectedTransferAsSender.Recipient.AccountNumber, receivedTransferAsSender?.Recipient.AccountNumber);
        Assert.Equal(expectedTransferAsSender.Recipient.Name, receivedTransferAsSender?.Recipient.Name);

        var expectedTransferAsRecipient = GenerateTransactionTransferDto(transferAsRecipient, false);
        var receivedTransferAsRecipient = getTransactionsOutput.Transactions[3] as TransactionTransferDto;
        Assert.Equal(expectedTransferAsRecipient.Type, receivedTransferAsRecipient?.Type);
        Assert.Equal(expectedTransferAsRecipient.Value, receivedTransferAsRecipient?.Value);
        Assert.Equal(expectedTransferAsRecipient.CreatedAt, receivedTransferAsRecipient?.CreatedAt);
        Assert.Equal(expectedTransferAsRecipient.Sender.AccountNumber, receivedTransferAsRecipient?.Sender.AccountNumber);
        Assert.Equal(expectedTransferAsRecipient.Sender.Name, receivedTransferAsRecipient?.Sender.Name);
        Assert.Equal(expectedTransferAsRecipient.Recipient.AccountNumber, receivedTransferAsRecipient?.Recipient.AccountNumber);
        Assert.Equal(expectedTransferAsRecipient.Recipient.Name, receivedTransferAsRecipient?.Recipient.Name);
    }
}