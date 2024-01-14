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
                .Where(a => a.GetFields().Active).ToListAsync();
            return Ok(new GetAllOutput
            {
                Accounts = accounts.Select(a => new TransactionAccountDto(a.GetPublicData())).ToList()
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get accounts."));
        }
    }

    [HttpGet("{accountNumber}", Name = "GetAccount")]
    public async Task<ActionResult<Account>> GetById(int accountNumber)
    {
        try
        {
            var account = await _context.Accounts.AsNoTracking().Include("Owner")
                .FirstOrDefaultAsync(a => a.GetFields().Active && a.GetFields().AccountNumber == accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            return Ok(account);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get account."));
        }
    }

    [HttpGet("bycpf/{cpf}")]
    public async Task<ActionResult<Account>> GetByCpf(string cpf)
    {
        try
        {
            var account = await _context.Accounts.AsNoTracking().Include("Owner")
                .FirstOrDefaultAsync(a =>
                    a.GetFields().Active && a.Owner != null && a.Owner.GetFields().Cpf != null
                    && a.Owner != null && a.Owner.GetFields().Cpf != null && a.Owner.GetFields().Cpf!.Equals(cpf));
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
                .FirstOrDefaultAsync(a =>
                    a.Owner != null && a.Owner.GetFields().Cpf != null && a.Owner.GetFields().Cpf!.Equals(newAccountDto.Cpf));
            if (existingAccount != null) return BadRequest(new ErrorDto("Cpf already registered."));
            var customer = new Customer(new CustomerFields() { Cpf = newAccountDto.Cpf, Name = newAccountDto.Name });
            var newAccount = new Account(new AccountFields());
            newAccount.Owner = customer;
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();
            return new CreatedAtRouteResult("GetAccount",
                new PostOutput { AccountNumber = newAccount.GetFields().AccountNumber },
                new PostOutput { AccountNumber = newAccount.GetFields().AccountNumber });
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
                .FirstOrDefaultAsync(a => a.GetFields().Active && a.GetFields().AccountNumber == accountNumber);
            if (existingAccount == null) return NotFound(new ErrorDto("Account not found."));
            existingAccount.Update(new CustomerUpdateableFields() { Name = updatedAccountDto.Name });
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
                .FirstOrDefaultAsync(a => a.GetFields().Active && a.GetFields().AccountNumber == accountNumber);
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