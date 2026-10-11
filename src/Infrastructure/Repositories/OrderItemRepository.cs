using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;

    public OrderItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems.Where(i => i.OrderId == orderId).ToListAsync();
    }

    public async Task<OrderItem?> GetByIdAsync(int id)
    {
        return await _context.OrderItems.FindAsync(id);
    }

    public async Task AddAsync(OrderItem item)
    {
        await _context.OrderItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(OrderItem item)
    {
        _context.OrderItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
