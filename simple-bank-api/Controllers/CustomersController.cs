using Context;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

public class CustomersController : ControllerBase
{
    private readonly BankContext _context;

    public CustomersController(BankContext context)
    {
        _context = context;
    }
}