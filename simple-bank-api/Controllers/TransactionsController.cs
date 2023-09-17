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
}