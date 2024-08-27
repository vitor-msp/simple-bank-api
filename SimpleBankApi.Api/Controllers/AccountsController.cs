using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBankApi.Api.Presenters;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Exceptions;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
[Route("accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly ICreateAccountUseCase _createAccountUseCase;
    private readonly IUpdateAccountUseCase _updateAccountUseCase;
    private readonly IDeleteAccountUseCase _deleteAccountUseCase;
    private readonly IGetAllAccountsUseCase _getAllAccountsUseCase;
    private readonly IGetAccountUseCase _getAccountUseCase;

    public AccountsController(
        ICreateAccountUseCase createAccountUseCase, IUpdateAccountUseCase updateAccountUseCase,
        IDeleteAccountUseCase deleteAccountUseCase, IGetAllAccountsUseCase getAllAccountsUseCase,
        IGetAccountUseCase getAccountUseCase)
    {
        _createAccountUseCase = createAccountUseCase;
        _updateAccountUseCase = updateAccountUseCase;
        _deleteAccountUseCase = deleteAccountUseCase;
        _getAllAccountsUseCase = getAllAccountsUseCase;
        _getAccountUseCase = getAccountUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<GetAllAccountsOutput>> GetAll()
    {
        try
        {
            var output = await _getAllAccountsUseCase.Execute();
            return Ok(output);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to get accounts."));
        }
    }

    [HttpGet("{accountNumber}", Name = "GetAccount")]
    public async Task<ActionResult<object>> GetById(int accountNumber)
    {
        try
        {
            var output = await _getAccountUseCase.ByAccountNumber(accountNumber);
            return Ok(output);
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to get account."));
        }
    }

    [HttpGet("bycpf/{cpf}")]
    public async Task<ActionResult<object>> GetByCpf(string cpf)
    {
        try
        {
            var output = await _getAccountUseCase.ByCpf(cpf);
            return Ok(output);
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to get account."));
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateAccountOutput>> Post([FromBody] CreateAccountInput newAccountDto)
    {
        try
        {
            var output = await _createAccountUseCase.Execute(newAccountDto);
            return new CreatedAtRouteResult("GetAccount", output, output);
        }
        catch (DomainException error)
        {
            return BadRequest(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to create account."));
        }
    }

    [HttpPut("{accountNumber}")]
    public async Task<ActionResult> Put(int accountNumber, [FromBody] UpdateAccountInput updatedAccountDto)
    {
        try
        {
            await _updateAccountUseCase.Execute(accountNumber, updatedAccountDto);
            return NoContent();
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to update account."));
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
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to inactivate account."));
        }
    }
}