using System.Security.Claims;

namespace SimpleBankApi.Api.Validators;

public static class AccountNumberAccessValidator
{
    public static void UserCanAccess(ClaimsPrincipal authenticatedUser, int accountNumber)
    {
        if (authenticatedUser.Identity?.Name != accountNumber.ToString())
            throw new UnauthorizedAccessException("User is not authorized to access account number.");
    }
}