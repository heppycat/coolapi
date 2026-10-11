using Core.Entities;

namespace Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}
