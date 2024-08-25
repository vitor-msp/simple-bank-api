using Microsoft.AspNetCore.Mvc;
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

    public Task<ActionResult<LoginOutput>> Login(LoginInput input)
    {
        return default;
    }
}