using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    // GET: api/orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> Get(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Post([FromBody] CreateOrderDto dto)
    {
        var created = await _orderService.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT: api/orders/5  (changes the status)
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] UpdateOrderDto dto)
    {
        await _orderService.UpdateOrderAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/orders/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }
}
