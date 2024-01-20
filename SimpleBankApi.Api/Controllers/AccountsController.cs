using Application;
using Dto;
using Input;
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
    private readonly IDeleteAccountUseCase _deleteAccountUseCase;
    private readonly IGetAllAccountsUseCase _getAllAccountUseCase;

    public AccountsController(IAccountsRepository accountsRepository,
        ICreateAccountUseCase createAccountUseCase, IUpdateAccountUseCase updateAccountUseCase,
        IDeleteAccountUseCase deleteAccountUseCase, IGetAllAccountsUseCase getAllAccountUseCase)
    {
        _accountsRepository = accountsRepository;
        _createAccountUseCase = createAccountUseCase;
        _updateAccountUseCase = updateAccountUseCase;
        _deleteAccountUseCase = deleteAccountUseCase;
        _getAllAccountUseCase = getAllAccountUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<GetAllAccountsOutput>> GetAll()
    {
        try
        {
            var output = await _getAllAccountUseCase.Execute();
            return Ok(output);
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

    [HttpPost]
    public async Task<ActionResult<PostAccountOutput>> Post([FromBody] CreateAccountInput newAccountDto)
    {
        try
        {
            var accountNumber = await _createAccountUseCase.Execute(newAccountDto);
            var postAccountOutput = new PostAccountOutput() { AccountNumber = accountNumber };
            return new CreatedAtRouteResult("GetAccount", postAccountOutput, postAccountOutput);
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
            await _deleteAccountUseCase.Execute(accountNumber);
            return NoContent();
        }
        catch (ApplicationException error)
        {
            return NotFound(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to inactivate account."));
        }
    }
}