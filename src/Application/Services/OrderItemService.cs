using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using FluentValidation;

namespace Application.Services;

public class OrderItemService : IOrderItemService
{
    private readonly IOrderItemRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateOrderItemDto> _createValidator;
    private readonly IValidator<UpdateOrderItemDto> _updateValidator;

    public OrderItemService(
        IOrderItemRepository repository,
        ICurrentUser currentUser,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IValidator<CreateOrderItemDto> createValidator,
        IValidator<UpdateOrderItemDto> updateValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId)
    {
        await GetOrderOrThrowAsync(orderId);
        var items = await _repository.GetByOrderIdAsync(orderId);
        return items.Select(ToDto);
    }

    public async Task<OrderItemDto?> GetOrderItemByIdAsync(int orderId, int id)
    {
        await GetOrderOrThrowAsync(orderId); // also enforces ownership
        var item = await _repository.GetByIdAsync(id);
        // an item only "exists" under the order it belongs to
        return item == null || item.OrderId != orderId ? null : ToDto(item);
    }

    public async Task<OrderItemDto> CreateOrderItemAsync(int orderId, CreateOrderItemDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(orderId);
        EnsureEditable(order);

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        var item = new OrderItem
        {
            OrderId = orderId,
            ProductId = product.Id,
            Quantity = dto.Quantity,
            UnitPrice = product.Price, // snapshot the price at the time it was added
        };
        await _repository.AddAsync(item);
        return ToDto(item);
    }

    public async Task UpdateOrderItemAsync(int orderId, int id, UpdateOrderItemDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(orderId);
        EnsureEditable(order);

        var item = await _repository.GetByIdAsync(id);
        if (item == null || item.OrderId != orderId)
            throw new KeyNotFoundException("Order item not found.");

        item.Quantity = dto.Quantity;
        await _repository.UpdateAsync(item);
    }

    public async Task DeleteOrderItemAsync(int orderId, int id)
    {
        var order = await GetOrderOrThrowAsync(orderId);
        EnsureEditable(order);

        var item = await _repository.GetByIdAsync(id);
        if (item != null && item.OrderId == orderId)
            await _repository.DeleteAsync(id);
    }

    private async Task<Order> GetOrderOrThrowAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || !_currentUser.CanAccessCustomer(order.CustomerId))
            throw new KeyNotFoundException("Order not found.");
        return order;
    }

    // lines can only change while the order is still pending
    private static void EnsureEditable(Order order)
    {
        if (order.Status != OrderStatus.Pending)
            throw new ValidationException(
                $"Items can only be changed while the order is Pending (current status: {order.Status})."
            );
    }

    private static OrderItemDto ToDto(OrderItem item) =>
        new()
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.Quantity * item.UnitPrice,
        };
}
