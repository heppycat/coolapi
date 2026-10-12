using System.Security.Claims;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Auth;

namespace Api;

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User ?? new ClaimsPrincipal();
    }

    public bool IsAdmin => _principal.IsInRole(nameof(UserRole.Admin));

    public int? CustomerId =>
        int.TryParse(_principal.FindFirstValue(JwtTokenGenerator.CustomerIdClaim), out var id)
            ? id
            : null;
}
