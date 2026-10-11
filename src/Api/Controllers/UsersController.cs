using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET api/users/me - any signed-in user
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(sub, out var id))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(id);
        return user == null ? Unauthorized() : Ok(user);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return Ok(await _userService.GetAllUsersAsync());
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}/role")]
    public async Task<ActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        await _userService.UpdateUserRoleAsync(id, dto);
        return NoContent();
    }
}
