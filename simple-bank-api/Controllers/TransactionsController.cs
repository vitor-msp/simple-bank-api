using System.Collections;
using Context;
using Dto;
using Exceptions;
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

    [HttpPost("credit/{customerId}")]
    public async Task<IActionResult> PostCredit(int customerId, [FromBody] CreditDto creditDto)
    {
        try
        {
            Customer? customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == customerId);
            if (customer == null) return NotFound();
            var credit = new Credit(creditDto, customer);
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (TransactionException)
        {
            return BadRequest();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("debit/{customerId}")]
    public async Task<IActionResult> PostDebit(int customerId, [FromBody] DebitDto debitDto)
    {
        try
        {
            Customer? customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == customerId);
            if (customer == null) return NotFound();
            double balance = await CalculateBalanceFromCustomer(customer);
            if (balance < debitDto.Value) return BadRequest();
            var debit = new Debit(debitDto, customer);
            _context.Debits.Add(debit);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (TransactionException)
        {
            return BadRequest();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("transfer/{customerId}")]
    public async Task<IActionResult> PostTransfer(int customerId, [FromBody] TransferDto transferDto)
    {
        try
        {
            Customer? sender = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == customerId);
            if (sender == null) return NotFound();
            Customer? recipient = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == transferDto.RecipientId);
            if (recipient == null) return NotFound();
            if (sender.Equals(recipient)) return BadRequest();
            double balance = await CalculateBalanceFromCustomer(sender);
            if (balance < transferDto.Value) return BadRequest();
            var transfer = new Transfer(transferDto, sender, recipient);
            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (TransactionException)
        {
            return BadRequest();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("balance/{customerId}")]
    public async Task<IActionResult> GetBalance(int customerId)
    {
        try
        {
            Customer? customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == customerId);
            if (customer == null) return NotFound();
            double balance = await CalculateBalanceFromCustomer(customer);
            return Ok(new { balance });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetTransactions(int customerId)
    {
        try
        {
            Customer? customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == customerId);
            if (customer == null) return NotFound();
            var credits = await GetCreditsFromCustomer(customer);
            var debits = await GetDebitsFromCustomer(customer);
            var transfers = await GetTransfersFromCustomer(customer);
            var sortedTransactions = SortTransactionsByDateTime(credits, debits, transfers, customer);
            return Ok(new { transactions = sortedTransactions });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    private async Task<double> CalculateBalanceFromCustomer(Customer customer)
    {
        double creditSum = (await GetCreditsFromCustomer(customer)).Sum(c => c.Value);
        double debitSum = (await GetDebitsFromCustomer(customer)).Sum(d => d.Value);
        var transfers = await GetTransfersFromCustomer(customer);
        double transferSum = transfers.Sum(t => t.Sender.Equals(customer) ? (-1 * t.Value) : t.Value);
        double balance = creditSum + debitSum + transferSum;
        return balance;
    }

    private async Task<List<Credit>> GetCreditsFromCustomer(Customer customer)
    {
        var credits = await _context.Credits.AsNoTracking()
            .Where(c => c.Customer.Equals(customer)).ToListAsync();
        return credits;
    }

    private async Task<List<Debit>> GetDebitsFromCustomer(Customer customer)
    {
        var debits = await _context.Debits.AsNoTracking()
            .Where(d => d.Customer.Equals(customer)).ToListAsync();
        return debits;
    }

    private async Task<List<Transfer>> GetTransfersFromCustomer(Customer customer)
    {
        var transfers = await _context.Transfers.AsNoTracking().Include("Sender").Include("Recipient")
            .Where(t => t.Sender.Equals(customer) || t.Recipient.Equals(customer)).ToListAsync();
        return transfers;
    }

    private ArrayList SortTransactionsByDateTime(
        List<Credit> credits, List<Debit> debits, List<Transfer> transfers, Customer customer)
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
                sortedTransactions.Add(credit.GetDataWithoutCustomer());
                creditIndex++;
            }
            else if (debitTimestamp <= transferTimestamp)
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(debit.GetDataWithoutCustomer());
                debitIndex++;
            }
            else
            {
                var transfer = transfers.ElementAt(transferIndex);
                sortedTransactions.Add(transfer.GetData(customer));
                transferIndex++;
            }
        }
        return sortedTransactions;
    }
}