using Application;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;

[ApiController]
[Route("accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly ICreateAccountUseCase _createAccountUseCase;

    private readonly IUpdateAccountUseCase _updateAccountUseCase;

    public AccountsController(IAccountsRepository accountsRepository, ICreateAccountUseCase createAccountUseCase, IUpdateAccountUseCase updateAccountUseCase)
    {
        _accountsRepository = accountsRepository;
        _createAccountUseCase = createAccountUseCase;
        _updateAccountUseCase = updateAccountUseCase;
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
            var accountNumber = await _createAccountUseCase.Execute(newAccountDto);
            return new CreatedAtRouteResult("GetAccount",
                new PostOutput { AccountNumber = accountNumber },
                new PostOutput { AccountNumber = accountNumber });
        }
        catch (ApplicationException error)
        {
            return BadRequest(new ErrorDto(error.Message));
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
            await _updateAccountUseCase.Execute(accountNumber, updatedAccountDto);
            return NoContent();
        }
        catch (ApplicationException error)
        {
            return NotFound(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
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