using Microsoft.AspNetCore.Mvc;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(
        CreateCustomerDto dto)
    {
        var customer = await _customerService.CreateCustomerAsync(dto);

        return Ok(customer);
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
    {
        var customers = await _customerService.GetCustomersAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        CreateCustomerDto dto)
    {
        var updated = await _customerService.UpdateCustomerAsync(id, dto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var deleted = await _customerService.DeleteCustomerAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}