using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // GET: api/<CustomersController>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var costumers = await _customerService.GetAllCustomersAsync();
        return Ok(costumers);
    }

    // GET api/<CustomersController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> Get(int id)
    {
        var costumer = await _customerService.GetCustomerByIdAsync(id);
        if (costumer == null)
            return NotFound();
        return Ok(costumer);
    }

    // POST api/<CustomersController>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Post([FromBody] CreateCustomerDto dto)
    {
        var created = await _customerService.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            await _customerService.UpdateCustomerAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}
