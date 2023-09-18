using Context;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Controllers;

[ApiController]
[Route("accounts")]
public class AccountsController : ControllerBase
{
    private readonly BankContext _context;

    public AccountsController(BankContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var accounts = await _context.Accounts.AsNoTracking().Include("Owner")
                .Where(a => a.Active).ToListAsync();
            return Ok(accounts);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{accountNumber}", Name = "GetAccount")]
    public async Task<IActionResult> GetById(int accountNumber)
    {
        try
        {
            var account = await _context.Accounts.AsNoTracking().Include("Owner")
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (account == null) return NotFound();
            return Ok(account);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("bycpf/{cpf}")]
    public async Task<IActionResult> GetByCpf(string cpf)
    {
        try
        {
            var account = await _context.Accounts.AsNoTracking().Include("Owner")
                .FirstOrDefaultAsync(a => a.Active && a.Owner.Cpf.Equals(cpf));
            if (account == null) return NotFound();
            return Ok(account);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AccountCreateDto newAccountDto)
    {
        try
        {
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Owner.Cpf.Equals(newAccountDto.Cpf));
            if (existingAccount != null) return BadRequest();
            var customer = new Customer(newAccountDto);
            var newAccount = new Account(customer);
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();
            return new CreatedAtRouteResult("GetAccount", new { accountNumber = newAccount.AccountNumber }, null);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("{accountNumber}")]
    public async Task<IActionResult> Put(int accountNumber, [FromBody] AccountUpdateDto updatedAccountDto)
    {
        try
        {
            var existingAccount = await _context.Accounts.Include("Owner")
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (existingAccount == null) return NotFound();
            existingAccount.Update(updatedAccountDto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{accountNumber}")]
    public async Task<IActionResult> Delete(int accountNumber)
    {
        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (account == null) return NotFound();
            account.Inactivate();
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}