using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using FluentValidation;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IValidator<UpdateOrderDto> _updateValidator;
    private readonly IValidator<CreateOrderItemDto> _createItemValidator;
    private readonly IValidator<UpdateOrderItemDto> _updateItemValidator;

    public OrderService(
        IOrderRepository repository,
        ICurrentUser currentUser,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IValidator<CreateOrderDto> createValidator,
        IValidator<UpdateOrderDto> updateValidator,
        IValidator<CreateOrderItemDto> createItemValidator,
        IValidator<UpdateOrderItemDto> updateItemValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _createItemValidator = createItemValidator;
        _updateItemValidator = updateItemValidator;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _repository.GetAllAsync();
        return orders.Where(o => _currentUser.CanAccessCustomer(o.CustomerId)).Select(ToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        // orders you can't access look the same as ones that don't exist
        return order == null || !_currentUser.CanAccessCustomer(order.CustomerId)
            ? null
            : ToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        // non-admins can only order for themselves, whatever the request says
        if (!_currentUser.IsAdmin)
            dto.CustomerId =
                _currentUser.CustomerId
                ?? throw new KeyNotFoundException("Customer not found.");

        await _createValidator.ValidateAndThrowAsync(dto);

        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new KeyNotFoundException("Customer not found.");

        var order = Order.Create(dto.CustomerId);

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product {itemDto.ProductId} not found.");

            order.AddItem(product, itemDto.Quantity);
        }

        await _repository.AddAsync(order);
        return ToDto(order);
    }

    public async Task UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(id);
        order.ChangeStatus(dto.Status);
        await _repository.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<OrderItemDto>> GetItemsAsync(int orderId)
    {
        var order = await GetOrderOrThrowAsync(orderId);
        return order.Items.Select(ToDto);
    }

    public async Task<OrderItemDto?> GetItemAsync(int orderId, int itemId)
    {
        var order = await GetOrderOrThrowAsync(orderId);
        var item = order.FindItem(itemId);
        return item == null ? null : ToDto(item);
    }

    public async Task<OrderItemDto> AddItemAsync(int orderId, CreateOrderItemDto dto)
    {
        await _createItemValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(orderId);

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        var item = order.AddItem(product, dto.Quantity);
        await _repository.UpdateAsync(order);
        return ToDto(item);
    }

    public async Task ChangeItemQuantityAsync(int orderId, int itemId, UpdateOrderItemDto dto)
    {
        await _updateItemValidator.ValidateAndThrowAsync(dto);

        var order = await GetOrderOrThrowAsync(orderId);
        order.ChangeItemQuantity(itemId, dto.Quantity);
        await _repository.UpdateAsync(order);
    }

    public async Task RemoveItemAsync(int orderId, int itemId)
    {
        var order = await GetOrderOrThrowAsync(orderId);
        order.RemoveItem(itemId);
        await _repository.UpdateAsync(order);
    }

    // orders you can't access look the same as ones that don't exist
    private async Task<Order> GetOrderOrThrowAsync(int orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null || !_currentUser.CanAccessCustomer(order.CustomerId))
            throw new KeyNotFoundException("Order not found.");
        return order;
    }

    private static OrderDto ToDto(Order order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(ToDto).ToList(),
            Total = order.Total,
        };

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
