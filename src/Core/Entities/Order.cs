namespace Core.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
