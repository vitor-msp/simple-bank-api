using System.Collections;
using Dto;
using Exceptions;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;

    public TransactionsController(ITransactionsRepository transactionsRepository, IAccountsRepository accountsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
    }


    [HttpPost("credit/{accountNumber}")]
    public async Task<ActionResult> PostCredit(int accountNumber, [FromBody] CreditDto creditDto)
    {
        try
        {
            Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            var credit = new Credit(new CreditFields() { Value = creditDto.Value }) { Account = account };
            await _transactionsRepository.SaveCredit(credit);
            return Ok();
        }
        catch (TransactionException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create credit."));
        }
    }

    [HttpPost("debit/{accountNumber}")]
    public async Task<ActionResult> PostDebit(int accountNumber, [FromBody] DebitDto debitDto)
    {
        try
        {
            Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            double balance = await CalculateBalanceFromAccount(account);
            if (balance < debitDto.Value) return BadRequest(new ErrorDto("Insufficient balance."));
            var debit = new Debit(new DebitFields() { Value = debitDto.Value }) { Account = account };
            await _transactionsRepository.SaveDebit(debit);
            return Ok();
        }
        catch (TransactionException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create debit."));
        }
    }

    [HttpPost("transfer/{accountNumber}")]
    public async Task<ActionResult> PostTransfer(int accountNumber, [FromBody] TransferDto transferDto)
    {
        try
        {
            Account? sender = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (sender == null) return NotFound(new ErrorDto("Sender account not found."));
            Account? recipient = await _accountsRepository.GetByAccountNumber(transferDto.RecipientAccountNumber);
            if (recipient == null) return NotFound(new ErrorDto("Recipient account not found."));
            if (sender.Equals(recipient))
                return BadRequest(new ErrorDto("Transfer to the same account is not allowed."));
            double balance = await CalculateBalanceFromAccount(sender);
            if (balance < transferDto.Value) return BadRequest(new ErrorDto("Insufficient balance."));
            var transfer = new Transfer(new TransferFields() { Value = transferDto.Value }) { Sender = sender, Recipient = recipient };
            await _transactionsRepository.SaveTransfer(transfer);
            return Ok();
        }
        catch (TransactionException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create transfer."));
        }
    }

    public class GetBalanceOutput
    {
        public string Balance { get; set; } = "";
    }

    [HttpGet("balance/{accountNumber}")]
    public async Task<ActionResult<GetBalanceOutput>> GetBalance(int accountNumber)
    {
        try
        {
            Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            double balance = await CalculateBalanceFromAccount(account);
            return Ok(new GetBalanceOutput { Balance = CurrencyHelper.GetBrazilianCurrency(balance) });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get balance."));
        }
    }

    public class GetTransactionsOutput
    {
        public ArrayList Transactions { get; set; } = new();
    }

    [HttpGet("{accountNumber}")]
    public async Task<ActionResult<GetTransactionsOutput>> GetTransactions(int accountNumber)
    {
        try
        {
            Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            var credits = await GetCreditsFromAccount(account);
            var debits = await GetDebitsFromAccount(account);
            var transfers = await GetTransfersFromAccount(account);
            var sortedTransactions = SortTransactionsByDateTime(credits, debits, transfers, account);
            return Ok(new GetTransactionsOutput { Transactions = sortedTransactions });
        }
        catch (Exception error)
        {
            System.Console.WriteLine(error.Message);
            System.Console.WriteLine(error.StackTrace);
            return StatusCode(500, new ErrorDto("Error to get transactions."));
        }
    }

    private async Task<double> CalculateBalanceFromAccount(Account account)
    {
        double creditSum = (await GetCreditsFromAccount(account)).Sum(c => c.GetFields().Value);
        double debitSum = -1 * (await GetDebitsFromAccount(account)).Sum(d => d.GetFields().Value);
        var transfers = await GetTransfersFromAccount(account);
        double transferSum = transfers.Sum(t => t.Sender.Equals(account) ? (-1 * t.GetFields().Value) : t.GetFields().Value);
        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<List<Credit>> GetCreditsFromAccount(Account account)
    {
        var credits = await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
        return credits;
    }

    private async Task<List<Debit>> GetDebitsFromAccount(Account account)
    {
        var debits = await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
        return debits;
    }

    private async Task<List<Transfer>> GetTransfersFromAccount(Account account)
    {
        var transfers = await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
        return transfers;
    }

    private ArrayList SortTransactionsByDateTime(
        List<Credit> credits, List<Debit> debits, List<Transfer> transfers, Account account)
    {
        var sortedTransactions = new ArrayList();
        int creditIndex = 0, debitIndex = 0, transferIndex = 0;
        int max = credits.Count() + debits.Count() + transfers.Count();
        List<long> creditTimestamps = credits
            .Select(c => new DateTimeOffset(c.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> debitTimestamps = debits
            .Select(d => new DateTimeOffset(d.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> transferTimestamps = transfers
            .Select(t => new DateTimeOffset(t.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        creditTimestamps.Add(long.MaxValue);
        debitTimestamps.Add(long.MaxValue);
        transferTimestamps.Add(long.MaxValue);
        for (int index = 0; index < max; index++)
        {
            var creditTimestamp = creditTimestamps.ElementAt(creditIndex);
            var debitTimestamp = debitTimestamps.ElementAt(debitIndex);
            var transferTimestamp = transferTimestamps.ElementAt(transferIndex);
            if (creditTimestamp <= debitTimestamp && creditTimestamp <= transferTimestamp)
            {
                var credit = credits.ElementAt(creditIndex);
                sortedTransactions.Add(new TransactionCreditDebitDto(credit.GetDataWithoutAccount()));
                creditIndex++;
            }
            else if (debitTimestamp <= transferTimestamp)
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(new TransactionCreditDebitDto(debit.GetDataWithoutAccount()));
                debitIndex++;
            }
            else
            {
                var transfer = transfers.ElementAt(transferIndex);
                sortedTransactions.Add(transfer.GetData(account));
                transferIndex++;
            }
        }
        return sortedTransactions;
    }
}