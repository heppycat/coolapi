using Core.Exceptions;

namespace Core.Entities;

// aggregate root: order items are only ever changed through the order
public class Order
{
    private readonly List<OrderItem> _items = new();

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public IReadOnlyCollection<OrderItem> Items => _items;

    public decimal Total => _items.Sum(i => i.LineTotal);

    private Order() { } // EF

    public static Order Create(int customerId) =>
        new()
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
        };

    public OrderItem AddItem(Product product, int quantity)
    {
        EnsurePending();
        // snapshot the price at the time it was added
        var item = new OrderItem(product.Id, quantity, product.Price);
        _items.Add(item);
        return item;
    }

    public OrderItem? FindItem(int itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    public void ChangeItemQuantity(int itemId, int quantity)
    {
        EnsurePending();
        var item = FindItem(itemId) ?? throw new KeyNotFoundException("Order item not found.");
        item.SetQuantity(quantity);
    }

    public void RemoveItem(int itemId)
    {
        EnsurePending();
        var item = FindItem(itemId);
        if (item != null)
            _items.Remove(item);
    }

    public void ChangeStatus(OrderStatus status)
    {
        Status = status;
    }

    // lines can only change while the order is still pending
    private void EnsurePending()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException(
                $"Items can only be changed while the order is Pending (current status: {Status})."
            );
    }
}
