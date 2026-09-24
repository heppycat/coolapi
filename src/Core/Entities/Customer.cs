namespace Core.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // add later when we have the Order
    //public ICollection<Order> Orders = new List<Order>;
}
