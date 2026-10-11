using Core.Entities;

namespace Application.Interfaces;

public interface IOrderItemRepository
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
    Task<OrderItem?> GetByIdAsync(int id);
    Task AddAsync(OrderItem item);
    Task UpdateAsync(OrderItem item);
    Task DeleteAsync(int id);
}
