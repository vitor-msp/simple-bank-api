using Microsoft.AspNetCore.Mvc;
using SimpleBankApi.Api.Presenters;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Domain.Exceptions;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
[Route("admin-accounts")]
public class AdminAccountsController : ControllerBase
{
    private readonly ICreateAdminAccountUseCase _createAdminAccountUseCase;

    public AdminAccountsController(ICreateAdminAccountUseCase createAdminAccountUseCase)
    {
        _createAdminAccountUseCase = createAdminAccountUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<CreateAccountOutput>> Post([FromBody] CreateAccountInput input)
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