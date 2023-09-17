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
            var customers = await _context.Customers.AsNoTracking().Where(c => c.Active).ToListAsync();
            return Ok(customers);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}", Name = "GetProduct")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var customer = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Active && c.Id == id);
            if (customer == null) return NotFound();
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
                .FirstOrDefaultAsync(c => c.Active && c.Cpf.Equals(cpf));
            if (customer == null) return NotFound();
            return Ok(customer);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomerCreateDto newCustomerDto)
    {
        try
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Cpf.Equals(newCustomerDto.Cpf));
            if (existingCustomer != null) return BadRequest();
            var newCustomer = new Customer().Hydrate(newCustomerDto);
            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();
            return new CreatedAtRouteResult("GetProduct", new { id = newCustomer.Id }, null);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] CustomerUpdateDto updatedCustomerDto)
    {
        try
        {
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Active && c.Id == id);
            if (existingCustomer == null) return NotFound();
            existingCustomer.Update(updatedCustomerDto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Active && c.Id == id);
            if (customer == null) return NotFound();
            customer.Inactivate();
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}