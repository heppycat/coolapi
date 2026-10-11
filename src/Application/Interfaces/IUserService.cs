using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task UpdateUserRoleAsync(int id, UpdateUserRoleDto dto);
}
