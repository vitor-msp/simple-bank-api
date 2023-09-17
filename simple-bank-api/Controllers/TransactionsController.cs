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
    public async Task<IActionResult> Credit(int customerId, [FromBody] CreditDto creditDto)
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
    public async Task<IActionResult> Debit(int customerId, [FromBody] DebitDto debitDto)
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
    public async Task<IActionResult> Balance(int customerId)
    {
        try
        {
            Customer? customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Active && c.Id == customerId);
            if (customer == null) return NotFound();
            double creditsSum = (await GetCreditTransactionsFromCustomer(customer)).Sum(c => c.Value);
            double debitsSum = -1 * (await GetDebitTransactionsFromCustomer(customer)).Sum(d => d.Value);
            // sum tranfers
            double balance = creditsSum + debitsSum;
            return Ok(new { balance });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
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
}