using Application.DTOs;

namespace Application.Interfaces;

public interface IOrderItemService
{
    Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId);
    Task<OrderItemDto?> GetOrderItemByIdAsync(int orderId, int id);
    Task<OrderItemDto> CreateOrderItemAsync(int orderId, CreateOrderItemDto dto);
    Task UpdateOrderItemAsync(int orderId, int id, UpdateOrderItemDto dto);
    Task DeleteOrderItemAsync(int orderId, int id);
}
