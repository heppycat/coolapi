using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using FluentValidation;

namespace Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ICurrentUser _currentUser;

    private readonly IValidator<CreateCustomerDto> _createValidator;

    public CustomerService(
        ICustomerRepository repository,
        ICurrentUser currentUser,
        IValidator<CreateCustomerDto> createValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _createValidator = createValidator;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        //fetch data from db (repo)
        var customers = await _repository.GetAllAsync();

        //business logic later ...

        return customers
            .Where(c => _currentUser.CanAccessCustomer(c.Id))
            .Select(p => new CustomerDto
        {
            Id = p.Id,
            Name = p.Name,
            Email = p.Email,
        });
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        // customers you can't access look the same as ones that don't exist
        return customer == null || !_currentUser.CanAccessCustomer(customer.Id)
            ? null
            : new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
            };
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var customer = new Customer { Name = dto.Name, Email = dto.Email };

        await _repository.AddAsync(customer);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };
    }

    public async Task UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
            throw new KeyNotFoundException("Customer not found.");

        customer.Name = dto.Name;
        customer.Email = dto.Email;
        await _repository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
