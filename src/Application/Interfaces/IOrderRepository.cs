using Core.Entities;

namespace Application.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(int id);
}
