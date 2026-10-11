using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user == null
            ? null
            : new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
            };
    }

    public async Task UpdateUserRoleAsync(int id, UpdateUserRoleDto dto)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Role = dto.Role;
        await _repository.UpdateAsync(user);
    }
}
