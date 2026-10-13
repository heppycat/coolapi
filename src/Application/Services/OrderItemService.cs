using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using FluentValidation;

namespace Application.Services;

// items belong to the Order aggregate: load the order, change it through its methods, save the order
public class OrderItemService : IOrderItemService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateOrderItemDto> _createValidator;
    private readonly IValidator<UpdateOrderItemDto> _updateValidator;

    public OrderItemService(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        IProductRepository productRepository,
        IValidator<CreateOrderItemDto> createValidator,
        IValidator<UpdateOrderItemDto> updateValidator
    )
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId)
    {
        var order = await GetOrderOrThrowAsync(orderId);
        return order.Items.Select(ToDto);
    }

    public async Task<OrderItemDto?> GetOrderItemByIdAsync(int orderId, int id)
    {
        var order = await GetOrderOrThrowAsync(orderId); // also enforces ownership
        var item = order.FindItem(id);
        return item == null ? null : ToDto(item);
    }

    public async Task<OrderItemDto> CreateOrderItemAsync(int orderId, CreateOrderItemDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(orderId);

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        var item = order.AddItem(product, dto.Quantity);
        await _orderRepository.UpdateAsync(order);
        return ToDto(item);
    }

    public async Task UpdateOrderItemAsync(int orderId, int id, UpdateOrderItemDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(orderId);
        order.ChangeItemQuantity(id, dto.Quantity);
        await _orderRepository.UpdateAsync(order);
    }

    public async Task DeleteOrderItemAsync(int orderId, int id)
    {
        var order = await GetOrderOrThrowAsync(orderId);
        order.RemoveItem(id);
        await _orderRepository.UpdateAsync(order);
    }

    private async Task<Order> GetOrderOrThrowAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || !_currentUser.CanAccessCustomer(order.CustomerId))
            throw new KeyNotFoundException("Order not found.");
        return order;
    }

    private static OrderItemDto ToDto(OrderItem item) =>
        new()
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal,
        };
}
