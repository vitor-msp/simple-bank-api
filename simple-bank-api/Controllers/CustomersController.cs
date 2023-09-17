using Context;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Controllers;

[ApiController]
[Route("customers")]
public class CustomersController : ControllerBase
{
    private readonly BankContext _context;

    public CustomersController(BankContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomerDto newCustomerDto)
    {
        try
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Cpf.Equals(newCustomerDto.Cpf));
            if (existingCustomer != null) return BadRequest();
            var newCustomer = new Customer().Hydrate(newCustomerDto);
            _context.Add(newCustomer);
            await _context.SaveChangesAsync();
            return Ok();
            // return new CreatedAtRouteResult("GetProduct", new { id = newCustomer.Id }, newCustomer);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}