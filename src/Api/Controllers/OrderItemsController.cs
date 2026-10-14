using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/orders/{orderId}/items")]
[Authorize]
public class OrderItemsController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderItemsController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: api/orders/5/items
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetItems(int orderId)
    {
        var items = await _orderService.GetItemsAsync(orderId);
        return Ok(items);
    }

    // GET: api/orders/5/items/3
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderItemDto>> Get(int orderId, int id)
    {
        var item = await _orderService.GetItemAsync(orderId, id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    // POST: api/orders/5/items
    [HttpPost]
    public async Task<ActionResult<OrderItemDto>> Post(
        int orderId,
        [FromBody] CreateOrderItemDto dto
    )
    {
        var created = await _orderService.AddItemAsync(orderId, dto);
        return CreatedAtAction(nameof(Get), new { orderId, id = created.Id }, created);
    }

    // PUT: api/orders/5/items/3  (changes the quantity)
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int orderId, int id, [FromBody] UpdateOrderItemDto dto)
    {
        await _orderService.ChangeItemQuantityAsync(orderId, id, dto);
        return NoContent();
    }

    // DELETE: api/orders/5/items/3
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int orderId, int id)
    {
        await _orderService.RemoveItemAsync(orderId, id);
        return NoContent();
    }
}
