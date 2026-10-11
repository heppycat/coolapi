using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth;

// wraps ASP.NET's PasswordHasher (PBKDF2, salted, versioned) without pulling full Identity into the model
public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
            != PasswordVerificationResult.Failed;
}
