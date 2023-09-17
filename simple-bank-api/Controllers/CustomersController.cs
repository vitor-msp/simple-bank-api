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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();
            return Ok(customers);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var customer = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null) return BadRequest();
            return Ok(customer);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("bycpf/{cpf}")]
    public async Task<IActionResult> GetByCpf(string cpf)
    {
        try
        {
            var customer = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Cpf.Equals(cpf));
            if (customer == null) return BadRequest();
            return Ok(customer);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
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