using Dto;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;

[ApiController]
[Route("accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;

    public AccountsController(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
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
            var accounts = await _accountsRepository.GetAll();
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
            var account = await _accountsRepository.GetByAccountNumber(accountNumber);
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
            var account = await _accountsRepository.GetByCpf(cpf);
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
            var existingAccount = await _accountsRepository.GetByCpf(newAccountDto.Cpf);
            if (existingAccount != null) return BadRequest(new ErrorDto("Cpf already registered."));
            var customer = new Customer(new CustomerFields() { Cpf = newAccountDto.Cpf, Name = newAccountDto.Name });
            var newAccount = new Account(new AccountFields()) { Owner = customer };
            await _accountsRepository.Save(newAccount);
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
    public async Task<ActionResult> Put(int accountNumber, [FromBody] AccountUpdateDto updatedAccountDto)
    {
        try
        {
            var account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            account.Update(new CustomerUpdateableFields() { Name = updatedAccountDto.Name });
            await _accountsRepository.Save(account);
            return NoContent();
        }
        catch (Exception error)
        {
            System.Console.WriteLine(error.Message);
            System.Console.WriteLine(error.StackTrace);
            return StatusCode(500, new ErrorDto("Error to update account."));
        }
    }

    [HttpDelete("{accountNumber}")]
    public async Task<ActionResult> Delete(int accountNumber)
    {
        try
        {
            var account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            account.Inactivate();
            await _accountsRepository.Save(account);
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to inactivate account."));
        }
    }
}