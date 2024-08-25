using Microsoft.AspNetCore.Mvc;
using SimpleBankApi.Api.Presenters;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController : ControllerBase
{
    private readonly ILoginUseCase _loginUseCase;

    public SessionsController(ILoginUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<LoginOutput>> Login([FromBody] LoginInput input)
    {
        try
        {
            var output = await _loginUseCase.Execute(input);
            return Ok(output);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
            return StatusCode(500, new ErrorPresenter("Error to login."));
        }
    }
}