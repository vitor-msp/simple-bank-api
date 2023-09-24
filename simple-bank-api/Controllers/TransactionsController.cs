using System.Collections;
using Context;
using Dto;
using Exceptions;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly BankContext _context;

    public TransactionsController(BankContext context)
    {
        _context = context;
    }

    [HttpPost("credit/{accountNumber}")]
    public async Task<IActionResult> PostCredit(int accountNumber, [FromBody] CreditDto creditDto)
    {
        try
        {
            Account? account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (account == null) return NotFound("Account not found.");
            var credit = new Credit(creditDto, account);
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (TransactionException error)
        {
            return BadRequest(error.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Error to create credit.");
        }
    }

    [HttpPost("debit/{accountNumber}")]
    public async Task<IActionResult> PostDebit(int accountNumber, [FromBody] DebitDto debitDto)
    {
        try
        {
            Account? account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (account == null) return NotFound("Account not found.");
            double balance = await CalculateBalanceFromAccount(account);
            if (balance < debitDto.Value) return BadRequest("Insufficient balance.");
            var debit = new Debit(debitDto, account);
            _context.Debits.Add(debit);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (TransactionException error)
        {
            return BadRequest(error.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Error to create debit.");
        }
    }

    [HttpPost("transfer/{accountNumber}")]
    public async Task<IActionResult> PostTransfer(int accountNumber, [FromBody] TransferDto transferDto)
    {
        try
        {
            Account? sender = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (sender == null) return NotFound("Sender account not found.");
            Account? recipient = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == transferDto.RecipientAccountNumber);
            if (recipient == null) return NotFound("Recipient account not found.");
            if (sender.Equals(recipient)) return BadRequest("Transfer to the same account is not allowed.");
            double balance = await CalculateBalanceFromAccount(sender);
            if (balance < transferDto.Value) return BadRequest("Insufficient balance.");
            var transfer = new Transfer(transferDto, sender, recipient);
            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (TransactionException error)
        {
            return BadRequest(error.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Error to create transfer.");
        }
    }

    [HttpGet("balance/{accountNumber}")]
    public async Task<IActionResult> GetBalance(int accountNumber)
    {
        try
        {
            Account? account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
            if (account == null) return NotFound("Account not found.");
            double balance = await CalculateBalanceFromAccount(account);
            return Ok(new { balance = CurrencyHelper.GetBrazilianCurrency(balance) });
        }
        catch (Exception)
        {
            return StatusCode(500, "Error to get balance.");
        }
    }

    [HttpGet("{accountNumber}")]
    public async Task<IActionResult> GetTransactions(int accountNumber)
    {
        try
        {
            Account? account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
            if (account == null) return NotFound("Account not found.");
            var credits = await GetCreditsFromAccount(account);
            var debits = await GetDebitsFromAccount(account);
            var transfers = await GetTransfersFromAccount(account);
            var sortedTransactions = SortTransactionsByDateTime(credits, debits, transfers, account);
            return Ok(new { transactions = sortedTransactions });
        }
        catch (Exception)
        {
            return StatusCode(500, "Error to get transactions.");
        }
    }

    private async Task<double> CalculateBalanceFromAccount(Account account)
    {
        double creditSum = (await GetCreditsFromAccount(account)).Sum(c => c.Value);
        double debitSum = (await GetDebitsFromAccount(account)).Sum(d => d.Value);
        var transfers = await GetTransfersFromAccount(account);
        double transferSum = transfers.Sum(t => t.Sender.Equals(account) ? (-1 * t.Value) : t.Value);
        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<List<Credit>> GetCreditsFromAccount(Account account)
    {
        var credits = await _context.Credits.AsNoTracking().Include("Account")
            .Where(c => c.Account.Equals(account)).ToListAsync();
        return credits;
    }

    private async Task<List<Debit>> GetDebitsFromAccount(Account account)
    {
        var debits = await _context.Debits.AsNoTracking().Include("Account")
            .Where(d => d.Account.Equals(account)).ToListAsync();
        return debits;
    }

    private async Task<List<Transfer>> GetTransfersFromAccount(Account account)
    {
        var transfers = await _context.Transfers.AsNoTracking()
            .Include("Sender.Owner").Include("Recipient.Owner")
            .Where(t => t.Sender.Equals(account) || t.Recipient.Equals(account)).ToListAsync();
        return transfers;
    }

    private ArrayList SortTransactionsByDateTime(
        List<Credit> credits, List<Debit> debits, List<Transfer> transfers, Account account)
    {
        var sortedTransactions = new ArrayList();
        int creditIndex = 0, debitIndex = 0, transferIndex = 0;
        int max = credits.Count() + debits.Count() + transfers.Count();
        List<long> creditTimestamps = credits
            .Select(c => new DateTimeOffset(c.CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> debitTimestamps = debits
            .Select(d => new DateTimeOffset(d.CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> transferTimestamps = transfers
            .Select(t => new DateTimeOffset(t.CreatedAt).ToUnixTimeMilliseconds()).ToList();
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
                sortedTransactions.Add(credit.GetDataWithoutAccount());
                creditIndex++;
            }
            else if (debitTimestamp <= transferTimestamp)
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(debit.GetDataWithoutAccount());
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