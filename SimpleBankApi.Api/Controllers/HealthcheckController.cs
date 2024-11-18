using Microsoft.AspNetCore.Mvc;

namespace SimpleBankApi.Api.Controllers;

[ApiController]
[Route("healthcheck")]
public class HealthcheckController
{
    public ActionResult<string> Healthcheck() => "health";
}