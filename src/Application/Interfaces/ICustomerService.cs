using Application.DTOs;

namespace Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task DeleteCustomerAsync(int id);
}
