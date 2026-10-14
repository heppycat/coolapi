using Application.DTOs;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task UpdateOrderAsync(int id, UpdateOrderDto dto);
    Task DeleteOrderAsync(int id);

    // order items are part of the Order aggregate, so they're handled here too
    Task<IEnumerable<OrderItemDto>> GetItemsAsync(int orderId);
    Task<OrderItemDto?> GetItemAsync(int orderId, int itemId);
    Task<OrderItemDto> AddItemAsync(int orderId, CreateOrderItemDto dto);
    Task ChangeItemQuantityAsync(int orderId, int itemId, UpdateOrderItemDto dto);
    Task RemoveItemAsync(int orderId, int itemId);
}
