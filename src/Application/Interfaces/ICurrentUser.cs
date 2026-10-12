namespace Application.Interfaces;

public interface ICurrentUser
{
    bool IsAdmin { get; }

    // the customer the signed-in user acts as; null for admins
    int? CustomerId { get; }

    // admins can access every customer's data, everyone else only their own
    bool CanAccessCustomer(int customerId) => IsAdmin || CustomerId == customerId;
}
