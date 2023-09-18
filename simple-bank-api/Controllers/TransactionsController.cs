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
            // validate balance
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
            var credits = await GetCreditTransactionsFromCustomer(customer);
            var debits = await GetDebitTransactionsFromCustomer(customer);
            var sortedTransactions = SortTransactionsByDateTime(credits, debits);
            return Ok(new { transactions = sortedTransactions });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    private async Task<double> CalculateBalanceFromCustomer(Customer customer)
    {
        double creditsSum = (await GetCreditTransactionsFromCustomer(customer)).Sum(c => c.Value);
        double debitsSum = -1 * (await GetDebitTransactionsFromCustomer(customer)).Sum(d => d.Value);
        // sum tranfers
        double balance = creditsSum + debitsSum;
        return balance;
    }

    private async Task<List<Credit>> GetCreditTransactionsFromCustomer(Customer customer)
    {
        var credits = await _context.Credits.AsNoTracking()
            .Where(c => c.Customer.Equals(customer)).ToListAsync();
        return credits;
    }

    private async Task<List<Debit>> GetDebitTransactionsFromCustomer(Customer customer)
    {
        var debits = await _context.Debits.AsNoTracking()
            .Where(d => d.Customer.Equals(customer)).ToListAsync();
        return debits;
    }

    private List<CreditDebitDto> SortTransactionsByDateTime(List<Credit> credits, List<Debit> debits)
    {
        var sortedTransactions = new List<CreditDebitDto>();
        int creditIndex = 0, debitIndex = 0;
        int max = credits.Count() + debits.Count();
        List<long> creditTimestamps = credits
            .Select(c => new DateTimeOffset(c.CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> debitTimestamps = debits
            .Select(d => new DateTimeOffset(d.CreatedAt).ToUnixTimeMilliseconds()).ToList();
        creditTimestamps.Add(long.MaxValue);
        debitTimestamps.Add(long.MaxValue);
        for (int index = 0; index < max; index++)
        {
            var creditTimestamp = creditTimestamps.ElementAt(creditIndex);
            var debitTimestamp = debitTimestamps.ElementAt(debitIndex);
            if (creditTimestamp <= debitTimestamp)
            {
                var credit = credits.ElementAt(creditIndex);
                sortedTransactions.Add(credit.GetDataWithoutCustomer());
                creditIndex++;
            }
            else
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(debit.GetDataWithoutCustomer());
                debitIndex++;
            }
        }
        return sortedTransactions;
    }
}