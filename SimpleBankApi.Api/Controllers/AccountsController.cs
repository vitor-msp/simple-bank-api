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

    public class GetAllOutput
    {
        public List<TransactionAccountDto> Accounts { get; set; } = new();
    }

    [HttpGet]
    public async Task<ActionResult<GetAllOutput>> GetAll()
    {
        try
        {
            var accounts = await _context.Accounts.AsNoTracking().Include("Owner")
                .Where(a => a.Active).ToListAsync();
            return Ok(new GetAllOutput { Accounts = accounts.Select(a => a.GetPublicData()).ToList() });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get accounts."));
        }
    }

    [HttpGet("{accountNumber}", Name = "GetAccount")]
    public async Task<IActionResult> GetById(int accountNumber)
    {
        try
        {
            var account = await _context.Accounts.AsNoTracking().Include("Owner")
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            return Ok(account);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get account."));
        }
    }

    [HttpGet("bycpf/{cpf}")]
    public async Task<IActionResult> GetByCpf(string cpf)
    {
        try
        {
            var account = await _context.Accounts.AsNoTracking().Include("Owner")
                .FirstOrDefaultAsync(a => a.Active && a.Owner.Cpf.Equals(cpf));
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            return Ok(account);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get account."));
        }
    }

    public class PostOutput
    {
        public int AccountNumber { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult<PostOutput>> Post([FromBody] AccountCreateDto newAccountDto)
    {
        try
        {
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Owner.Cpf.Equals(newAccountDto.Cpf));
            if (existingAccount != null) return BadRequest(new ErrorDto("Cpf already registered."));
            var customer = new Customer(newAccountDto);
            var newAccount = new Account(customer);
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();
            return new CreatedAtRouteResult("GetAccount",
                new PostOutput { AccountNumber = newAccount.AccountNumber },
                new PostOutput { AccountNumber = newAccount.AccountNumber });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create account."));
        }
    }

    [HttpPut("{accountNumber}")]
    public async Task<IActionResult> Put(int accountNumber, [FromBody] AccountUpdateDto updatedAccountDto)
    {
        try
        {
            var existingAccount = await _context.Accounts.Include("Owner")
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (existingAccount == null) return NotFound(new ErrorDto("Account not found."));
            existingAccount.Update(updatedAccountDto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to update account."));
        }
    }

    [HttpDelete("{accountNumber}")]
    public async Task<IActionResult> Delete(int accountNumber)
    {
        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Active && a.AccountNumber == accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            account.Inactivate();
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to inactivate account."));
        }
    }
}