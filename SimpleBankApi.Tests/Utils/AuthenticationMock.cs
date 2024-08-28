using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SimpleBankApi.Tests;

public static class AuthenticationMock
{
    public static void AuthenticateUser(int accountNumber, ControllerBase controller)
    {
        var userClaims = new List<Claim>()
        {
            new(ClaimTypes.Name, accountNumber.ToString()),
        };
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuthType"))
            }
        };
    }
}
