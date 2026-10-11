using Application.DTOs;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task UpdateOrderAsync(int id, UpdateOrderDto dto);
    Task DeleteOrderAsync(int id);
}
