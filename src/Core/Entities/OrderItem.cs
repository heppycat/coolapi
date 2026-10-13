using Core.Exceptions;

namespace Core.Entities;

// part of the Order aggregate; create and change it through Order
public class OrderItem
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }

    // price of the product at the time the item was added (snapshot, not a live link)
    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    private OrderItem() { } // EF

    internal OrderItem(int productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0.");
        Quantity = quantity;
    }
}
