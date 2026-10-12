using Core.Entities;

namespace Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? CustomerId { get; set; }
}
