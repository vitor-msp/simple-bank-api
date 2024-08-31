using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBankApi.Api.Presenters;
using SimpleBankApi.Api.Validators;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Exceptions;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
[Route("accounts")]
public class AccountsController(
    ICreateAccountUseCase createAccountUseCase, IUpdateAccountUseCase updateAccountUseCase,
    IDeleteAccountUseCase deleteAccountUseCase, IGetAllAccountsUseCase getAllAccountsUseCase,
    IGetAccountUseCase getAccountUseCase, ICreateAdminAccountUseCase createAdminAccountUseCase) : ControllerBase
{
    private readonly ICreateAccountUseCase _createAccountUseCase = createAccountUseCase;
    private readonly IUpdateAccountUseCase _updateAccountUseCase = updateAccountUseCase;
    private readonly IDeleteAccountUseCase _deleteAccountUseCase = deleteAccountUseCase;
    private readonly IGetAllAccountsUseCase _getAllAccountsUseCase = getAllAccountsUseCase;
    private readonly IGetAccountUseCase _getAccountUseCase = getAccountUseCase;
    private readonly ICreateAdminAccountUseCase _createAdminAccountUseCase = createAdminAccountUseCase;

    [HttpGet]
    [Authorize(Roles = "Customer,Admin")]
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
    [Authorize(Roles = "Customer,Admin")]
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
    [Authorize(Roles = "Customer,Admin")]
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
    public async Task<ActionResult<CreateAccountOutput>> Post([FromBody] CreateAccountInput input)
    {
        try
        {
            var output = await _createAccountUseCase.Execute(input);
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
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> Put(int accountNumber, [FromBody] UpdateAccountInput updatedAccountDto)
    {
        try
        {
            AccountNumberAccessValidator.UserCanAccess(User, accountNumber);
            await _updateAccountUseCase.Execute(accountNumber, updatedAccountDto);
            return NoContent();
        }
        catch (UnauthorizedAccessException error)
        {
            return Unauthorized(new ErrorPresenter(error.Message));
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
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> Delete(int accountNumber)
    {
        try
        {
            AccountNumberAccessValidator.UserCanAccess(User, accountNumber);
            await _deleteAccountUseCase.Execute(accountNumber);
            return NoContent();
        }
        catch (UnauthorizedAccessException error)
        {
            return Unauthorized(new ErrorPresenter(error.Message));
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

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CreateAccountOutput>> PostAdmin([FromBody] CreateAccountInput input)
    {
        try
        {
            var output = await _createAdminAccountUseCase.Execute(input);
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
}