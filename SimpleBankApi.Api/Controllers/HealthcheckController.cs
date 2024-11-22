using Microsoft.AspNetCore.Mvc;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
[Route("healthcheck")]
public class HealthcheckController
{
    [HttpGet]
    public ActionResult<string> Healthcheck() => "health";
}