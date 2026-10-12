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

    public OrderService(
        IOrderRepository repository,
        ICurrentUser currentUser,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IValidator<CreateOrderDto> createValidator,
        IValidator<UpdateOrderDto> updateValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product {itemDto.ProductId} not found.");

            order.Items.Add(
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price, // snapshot the price at order time
                }
            );
        }

        await _repository.AddAsync(order);
        return ToDto(order);
    }

    public async Task UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await _repository.GetByIdAsync(id);
        if (order == null || !_currentUser.CanAccessCustomer(order.CustomerId))
            throw new KeyNotFoundException("Order not found.");

        order.Status = dto.Status;
        await _repository.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static OrderDto ToDto(Order order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order
                .Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice,
                })
                .ToList(),
            Total = order.Items.Sum(i => i.Quantity * i.UnitPrice),
        };
}
