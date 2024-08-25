using Microsoft.AspNetCore.Mvc;
using SimpleBankApi.Api.Presenters;
using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
public class SessionsController : ControllerBase
{
    private readonly ILoginUseCase _loginUseCase;
    private readonly IRefreshTokenUseCase _refreshTokenUseCase;
    private readonly ILogoutUseCase _logoutUseCase;

    public SessionsController(ILoginUseCase loginUseCase,
        IRefreshTokenUseCase refreshTokenUseCase, ILogoutUseCase logoutUseCase)
    {
        _loginUseCase = loginUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _logoutUseCase = logoutUseCase;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginOutput>> Login([FromBody] LoginInput input)
    {
        try
        {
            var output = await _loginUseCase.Execute(input);
            return Ok(output);
        }
        catch (EntityNotFoundException error)
        {
            return Unauthorized(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to login."));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenOutput>> RefreshToken([FromBody] RefreshTokenInput input)
    {
        try
        {
            var output = await _refreshTokenUseCase.Execute(input);
            return Ok(output);
        }
        catch (EntityNotFoundException error)
        {
            return Unauthorized(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to generate access token."));
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult<RefreshTokenOutput>> Logout([FromBody] LogoutInput input)
    {
        try
        {
            await _logoutUseCase.Execute(input);
            return NoContent();
        }
        catch (EntityNotFoundException error)
        {
            return Unauthorized(new ErrorPresenter(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorPresenter("Error to generate access token."));
        }
    }
}